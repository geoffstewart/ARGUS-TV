/*
 *	Copyright (C) 2007-2014 ARGUS TV
 *	http://www.argus-tv.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSNPSharp;

using ArgusTV.Common.Logging;
using ArgusTV.DataContracts;
using ArgusTV.ServiceProxy;
using System.Net;

namespace ArgusTV.Messenger.Msn
{
    internal class MsnThread : ArgusTV.Common.Threading.WorkerThread
    {
        private const string _threadName = "MSN-bot";
        private const int _maxMessageLength = 256;

        private const int _checkAlertsIntervalSeconds = 30;

        private static MsnThread _msnThread;

        private MSNPSharp.Messenger _messenger;
        private MsnConversations _msnConversations;
        private DateTime _nextPersonalMessageRefreshTime;
        private DateTime _nextAlertsCheckTime;
        private bool _refreshMessageAndStatus;

        private AutoResetEvent _wakeUpEvent;

        private bool _contactsValidated;

        private object _alertsLock = new object();
        private object _notificationsLock = new object();

        private List<UpcomingProgram> _upcomingAlerts;
        private AddressList _alertContactFilter;
        private AddressList _notificationContactFilter;

        public MsnThread(IMCommands imCommands)
            : base(_threadName)
        {
            _msnThread = this;
            _messenger = new MSNPSharp.Messenger();
            _msnConversations = new MsnConversations(_messenger, imCommands);
        }

        #region Static Methods

        public static string HandleAlertsCommand(object conversation, IList<string> arguments)
        {
            MsnConversation msnConversation = conversation as MsnConversation;
            if (_msnThread != null
                && msnConversation != null)
            {
                return _msnThread.InternalHandleAlertsCommand(msnConversation, arguments);
            }
            return String.Empty;
        }

        public static string HandleNotificationsCommand(object conversation, IList<string> arguments)
        {
            MsnConversation msnConversation = conversation as MsnConversation;
            if (_msnThread != null
                && msnConversation != null)
            {
                return _msnThread.InternalHandleNotificationsCommand(msnConversation, arguments);
            }
            return String.Empty;
        }
        #endregion

        protected override void Run()
        {
            Logger.Write(_threadName + " thread started.");

            _wakeUpEvent = new AutoResetEvent(false);
            try
            {
                Logger.Write("Starting service.");

                _messenger.ConnectionEstablished += NameserverProcessor_ConnectionEstablished;
                _messenger.ConnectingException += NameserverProcessor_ConnectingException;
                _messenger.Nameserver.ExceptionOccurred += Nameserver_ExceptionOccurred;
                _messenger.Nameserver.AuthenticationError += Nameserver_AuthenticationError;
                _messenger.Nameserver.ServerErrorReceived += Nameserver_ServerErrorReceived;
                _messenger.Nameserver.SignedIn += Nameserver_SignedIn;
                _messenger.MessageManager.TextMessageReceived += MessageManager_TextMessageReceived;
                _messenger.Nameserver.ContactOnline += Nameserver_ContactOnline;
                _messenger.Nameserver.ContactOffline += Nameserver_ContactOffline;

                _nextPersonalMessageRefreshTime = DateTime.Now.AddSeconds(5);
                _nextAlertsCheckTime = DateTime.Now.AddSeconds(_checkAlertsIntervalSeconds);
                do
                {
                    try
                    {
                        EnsureArgusTVConnection();
                        if (Proxies.IsInitialized)
                        {
                            EnsureEventListenerTaskStarted();

                            if (_messenger.Connected)
                            {
                                if (_messenger.Nameserver.IsSignedIn)
                                {
                                    if (_refreshMessageAndStatus)
                                    {
                                        RefreshBotPersonalMessage();
                                        _refreshMessageAndStatus = false;
                                    }

                                    // If needed, make sure our contact list is up-to-date.
                                    if (!_contactsValidated)
                                    {
                                        ValidateContacts();
                                        _contactsValidated = true;
                                    }

                                    if (DateTime.Now >= _nextPersonalMessageRefreshTime)
                                    {
                                        _nextPersonalMessageRefreshTime = DateTime.MaxValue;
                                        RefreshBotPersonalMessage();
                                    }

                                    if (_messenger.Owner.Name != "ARGUS TV")
                                    {
                                        _messenger.Owner.Name = "ARGUS TV";
                                    }

                                    if (DateTime.Now >= _nextAlertsCheckTime)
                                    {
                                        HandleAlerts();
                                        _nextAlertsCheckTime = DateTime.Now.AddSeconds(_checkAlertsIntervalSeconds);
                                    }
                                }
                                else
                                {
                                    //Logger.Verbose("Connected to Windows Live, but not signed in yet...");
                                }
                            }
                            else
                            {
                                _messenger.Credentials.Account = Proxies.ConfigurationService.GetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnAccount).Result;
                                _messenger.Credentials.Password = Proxies.ConfigurationService.GetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnPassword).Result;
                                if (!String.IsNullOrEmpty(_messenger.Credentials.Account))
                                {
                                    Logger.Info("Connecting to Windows Live as '{0}'...", _messenger.Credentials.Account);
                                    _messenger.Connect();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString());
                        if (_messenger.Connected)
                        {
                            DisconnectMessenger();
                        }
                    }
                } while (0 != WaitHandle.WaitAny(new WaitHandle[] { this.StopThreadEvent, _wakeUpEvent }, 1000, false));

                if (_messenger.Connected)
                {
                    DisconnectMessenger();
                }

                Logger.Write("Service ended.");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                using (ServiceController controller = new ServiceController("ArgusTVMessenger"))
                {
                    try { controller.Stop(); }
                    catch { }
                }
            }
            finally
            {
                _wakeUpEvent.Close();
                _wakeUpEvent = null;

                StopEventListenerTask();
            }

            Logger.Write(_threadName + " thread ended.");
        }

        private void NameserverProcessor_ConnectionEstablished(object sender, EventArgs e)
        {
        }

        private void NameserverProcessor_ConnectingException(object sender, ExceptionEventArgs e)
        {
        }

        private void Nameserver_ExceptionOccurred(object sender, ExceptionEventArgs e)
        {
        }

        private void Nameserver_AuthenticationError(object sender, ExceptionEventArgs e)
        {
        }

        private void DisconnectMessenger()
        {
            _msnConversations.CloseAllConversations();
            _messenger.Disconnect();
            _refreshMessageAndStatus = false;
        }

        private void EnsureArgusTVConnection()
        {
            if (!Proxies.IsInitialized)
            {
                ServerSettings serverSettings = new ServerSettings();
                serverSettings.ServerName = Properties.Settings.Default.ArgusTVServerName;
                serverSettings.Transport = ServiceTransport.Http;
                serverSettings.Port = Properties.Settings.Default.ArgusTVHttpPort;
                try
                {
                    Proxies.Initialize(serverSettings, true);
                    UpdateAlertMinutesSetting();
                }
                catch (ArgusTVNotFoundException ex)
                {
                    Logger.Error(ex.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to connect to ARGUS TV: " + ex.Message);
                }
            }
        }

        private void Nameserver_SignedIn(object sender, EventArgs e)
        {
            Logger.Info("Signed in to Windows Live.");

            _messenger.Owner.Status = PresenceStatus.Online;
            _messenger.Owner.UpdateDisplayImage(Properties.Resources.MsnBotAvatar);
            _messenger.Owner.UpdateRoamingProfileSync(Properties.Resources.MsnBotAvatar);
            _refreshMessageAndStatus = true;
        }

        private void Nameserver_ServerErrorReceived(object sender, MSNErrorEventArgs e)
        {
            if (e.MSNError != MSNError.AlreadyInMode
                && e.MSNError != MSNError.AlreadyLoggedIn)
            {
                Logger.Error("MSN Error: {0}", e.MSNError);
                DisconnectMessenger();
            }
        }

        private void MessageManager_TextMessageReceived(object sender, TextMessageArrivedEventArgs e)
        {
            MsnConversation conversation = _msnConversations.EnsureConversation(e.Sender);
            conversation.HandleIncomingMessage(e.TextMessage);
        }

        private void Nameserver_ContactOnline(object sender, ContactStatusChangedEventArgs e)
        {
            _msnConversations.CloseConversation(e.Contact);
        }

        private void Nameserver_ContactOffline(object sender, ContactStatusChangedEventArgs e)
        {
            _msnConversations.CloseConversation(e.Contact);
        }

        private void RefreshBotPersonalMessage()
        {
            if (_messenger.Owner != null)
            {
                string message = "Idle";
                PresenceStatus status = PresenceStatus.Online;

                var activeRecordings = Proxies.ControlService.GetActiveRecordings().Result;
                if (activeRecordings.Count > 0)
                {
                    message = "Recording";
                    status = PresenceStatus.Busy;
                }
                else
                {
                    var liveStreams = Proxies.ControlService.GetLiveStreams().Result;
                    if (liveStreams.Count > 0)
                    {
                        message = "Streaming";
                        status = PresenceStatus.Away;
                    }
                    else
                    {
                        UpcomingRecording upcomingRecording = Proxies.ControlService.GetNextUpcomingRecording(false).Result;
                        if (upcomingRecording != null)
                        {
                            message = "Waiting for next scheduled recording";
                        }
                    }
                }
                if (_messenger.Owner.PersonalMessage == null
                    || _messenger.Owner.PersonalMessage.Message != message)
                {
                    _messenger.Owner.PersonalMessage = new PersonalMessage(message);
                }
                if (_messenger.Owner.Status != status)
                {
                    _messenger.Owner.Status = status;
                }
            }
        }

        #region Contact List

        private void ValidateContacts()
        {
            Logger.Verbose("Validating Windows Live contacts...");

            bool reconnect = false;

            AddressList contactAddresses = new AddressList(
                Proxies.ConfigurationService.GetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnContactList).Result);

            List<Contact> contactsToRemove = new List<Contact>();
            foreach (Contact contact in _messenger.ContactList.Allowed)
            {
                if (!contactAddresses.Contains(contact.Account))
                {
                    contactsToRemove.Add(contact);
                }
            }
            foreach (Contact contact in contactsToRemove)
            {
                if (contact.Guid != Guid.Empty)
                {
                    contact.AppearOffline = true;
                    Logger.Write("Blocked contact: " + contact.Account);
                }
            }

            foreach (string mailAddress in contactAddresses)
            {
                Contact contact = FindContactInList(mailAddress);
                if (contact == null
                    || contact.Guid == Guid.Empty)
                {
                    _messenger.ContactService.AddNewContact(mailAddress, "ARGUS TV is inviting you.");
                    Logger.Write("Invited contact: " + contact.Account);
                    reconnect = true;
                }
                else if (contact.AppearOffline)
                {
                    contact.AppearOffline = false;
                    Logger.Write("Unblocked contact: " + contact.Account);
                    reconnect = true;
                }
            }

            if (reconnect)
            {
                Thread.Sleep(1000);
                DisconnectMessenger();
            }
        }

        private Contact FindContactInList(string mailAddress)
        {
            foreach (Contact contact in _messenger.ContactList.All)
            {
                if (String.Equals(contact.Account, mailAddress, StringComparison.CurrentCultureIgnoreCase))
                {
                    return contact;
                }
            }
            return null;
        }

        #endregion

        #region Alerts

        private int _alertMinutes = 10;

        private void UpdateAlertMinutesSetting()
        {
            int? value = Proxies.ConfigurationService.GetIntValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MinutesBeforeAlert).Result;
            if (value.HasValue)
            {
                _alertMinutes = value.Value;
                _recentlyAlerted.Clear();
            }
        }

        private void HandleAlerts()
        {
            List<UpcomingProgram> upcomingAlerts;
            AddressList alertContactFilter;
            lock (_alertsLock)
            {
                if (_upcomingAlerts == null)
                {
                    _upcomingAlerts = Proxies.SchedulerService.GetAllUpcomingPrograms(ScheduleType.Alert, false).Result;
                }
                EnsureAlertContactFilter();
                upcomingAlerts = _upcomingAlerts;
                alertContactFilter = _alertContactFilter;
            }

            bool sentAlert = false;
            foreach (UpcomingProgram upcomingAlert in upcomingAlerts)
            {
                if (upcomingAlert.StopTime > DateTime.Now
                    && upcomingAlert.StartTime.AddMinutes(-_alertMinutes) <= DateTime.Now)
                {
                    if (!IsRecentlyAlerted(upcomingAlert))
                    {
                        if (sentAlert)
                        {
                            // We just sent out an alert, seems this is needed to give the system
                            // some extra time :-(
                            Thread.Sleep(100);
                        }
                        if (BroadcastAlert(alertContactFilter, upcomingAlert))
                        {
                            _recentlyAlerted.Add(upcomingAlert);
                            sentAlert = true;
                        }
                    }
                }
            }
        }

        private List<UpcomingProgram> _recentlyAlerted = new List<UpcomingProgram>();

        private bool IsRecentlyAlerted(UpcomingProgram upcomingAlert)
        {
            bool result = false;
            List<UpcomingProgram> alertsToRemove = new List<UpcomingProgram>();
            foreach (UpcomingProgram recentlyAlerted in _recentlyAlerted)
            {
                if (recentlyAlerted.UpcomingProgramId == upcomingAlert.UpcomingProgramId)
                {
                    result = true;
                }
                else if (recentlyAlerted.StopTime < DateTime.Now.AddMinutes(-_alertMinutes))
                {
                    alertsToRemove.Add(recentlyAlerted);
                }
            }
            foreach (UpcomingProgram alertToRemove in alertsToRemove)
            {
                _recentlyAlerted.Remove(alertToRemove);
            }
            return result;
        }

        private bool BroadcastAlert(AddressList alertContactFilter, UpcomingProgram upcomingAlert)
        {
            bool result = false;

            if (_messenger.Nameserver.IsSignedIn)
            {
                foreach (Contact contact in _messenger.ContactList.Allowed)
                {
                    if (!alertContactFilter.ContainsAddress(contact.Account) &&
                        BroadcastingAllowed(contact.Status) )
                    {                        
                        StringBuilder text = new StringBuilder();
                        if (upcomingAlert.StartTime > DateTime.Now)
                        {
                            text.AppendLine("ALERT! This program is about to start:");
                        }
                        else
                        {
                            text.AppendLine("ALERT! This program has already started:");
                        }
                        Utility.AppendProgramDetails(text, upcomingAlert.Channel, upcomingAlert);

                        if (result)
                        {
                            // We just sent out an alert, seems this is needed to give the system
                            // some extra time :-(
                            Thread.Sleep(100);
                        }
                        BroadcastMessage(contact, text.ToString());
                        result = true;
                    }
                }
            }
            return result;
        }

        private void BroadcastRecording(AddressList addressList, string title, Recording recording, bool showDescription)
        {
            StringBuilder message = new StringBuilder(title);
            message.Append(" ");
            message.Append(recording.ProgramStartTime.ToShortTimeString());
            message.Append("-");
            message.Append(recording.ProgramStopTime.ToShortTimeString());
            message.Append(" (");
            message.Append(recording.ChannelDisplayName);
            message.Append(") ");
            message.Append(recording.CreateProgramTitle());
            if (showDescription)
            {
                string description = recording.CreateCombinedDescription(false);
                if (!String.IsNullOrEmpty(description))
                {
                    message.Append(Environment.NewLine).Append(Environment.NewLine);
                    message.Append(description);
                }
            }
            if (message.Length >= _maxMessageLength)
            {
                message.Length = _maxMessageLength - 4;
                message.Append("...");
            }
            BroadcastMessageToAddressList(addressList, message.ToString());
        }

        private bool BroadcastMessageToAddressList(AddressList addressList, string message)
        {
            bool result = false;

            if (_messenger.Nameserver.IsSignedIn)
            {
                foreach (Contact contact in _messenger.ContactList.Allowed)
                {
                    if (!addressList.ContainsAddress(contact.Account) &&
                        BroadcastingAllowed(contact.Status))
                    {                        
                        if (result)
                        {
                            // We just sent out a message, seems this is needed to give the system
                            // some extra time :-(
                            Thread.Sleep(100);
                        }
                        BroadcastMessage(contact, message);
                        result = true;
                    }
                }
            }
            return result;
        }

        private void BroadcastMessage(Contact contact, string message)
        {
            IMBotMessage imTextMessage = new IMBotMessage(message, true);
            BroadcastMessage(contact, imTextMessage);
        }

        private void BroadcastMessage(Contact contact, IMBotMessage message)
        {
            MsnConversation conversation = _msnConversations.EnsureConversation(contact);
            conversation.SendTextMessage(message.ToMsnMessage());
        }

        private bool BroadcastingAllowed(PresenceStatus presenceStatus)
        {
            bool broadcastingAllowed = false;

            switch (presenceStatus)
            {
                case PresenceStatus.Online:
                case PresenceStatus.Away:
                case PresenceStatus.BRB:
                case PresenceStatus.Idle:
                case PresenceStatus.Phone:
                case PresenceStatus.Unknown:
                    broadcastingAllowed = true;
                    break;
            }
            return broadcastingAllowed;
        }

        private string InternalHandleAlertsCommand(MsnConversation msnConversation, IList<string> arguments)
        {
            string contactMail = msnConversation.Contact.Account;
            lock (_alertsLock)
            {
                EnsureAlertContactFilter();

                bool settingChanged = false;
                string onOff = arguments.Count > 0 ? arguments[0].Trim() : String.Empty;
                if (onOff == "off")
                {
                    settingChanged = _alertContactFilter.AddAddress(contactMail);
                }
                else if (onOff == "on")
                {
                    settingChanged = _alertContactFilter.RemoveAddress(contactMail);
                }
                if (settingChanged)
                {
                    Proxies.ConfigurationService.SetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnAlertFilterList, _alertContactFilter.ToString()).Wait();
                }
                return _alertContactFilter.ContainsAddress(contactMail) ? "Alerts are off." : "Alerts are on.";
            }
        }

        private void EnsureAlertContactFilter()
        {
            if (_alertContactFilter == null)
            {
                _alertContactFilter = new AddressList(
                    Proxies.ConfigurationService.GetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnAlertFilterList).Result);
            }
        }

        private string InternalHandleNotificationsCommand(MsnConversation msnConversation, IList<string> arguments)
        {
            string contactMail = msnConversation.Contact.Account;
            lock (_notificationsLock)
            {
                EnsureNotificationContactFilter();

                bool settingChanged = false;
                string onOff = arguments.Count > 0 ? arguments[0].Trim() : String.Empty;
                if (onOff == "off")
                {
                    settingChanged = _notificationContactFilter.AddAddress(contactMail);
                }
                else if (onOff == "on")
                {
                    settingChanged = _notificationContactFilter.RemoveAddress(contactMail);
                }
                if (settingChanged)
                {
                    Proxies.ConfigurationService.SetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnNotificationFilterList, _notificationContactFilter.ToString()).Wait();
                }
                return _notificationContactFilter.ContainsAddress(contactMail) ? "Notifications are off." : "Notifications are on.";
            }
        }

        private void EnsureNotificationContactFilter()
        {
            if (_notificationContactFilter == null)
            {
                _notificationContactFilter = new AddressList(
                    Proxies.ConfigurationService.GetStringValue(ConfigurationModule.Messenger, ConfigurationKey.Messenger.MsnNotificationFilterList).Result);
            }
        }

        #endregion

        #region Events Listener

        private readonly string _eventsClientId = Dns.GetHostName() + "-1a5cfc2df9d14595b2fedc52166aeffc"; // Unique for the Messenger!
        private bool _eventListenerSubscribed;
        private Task _eventListenerTask;
        private CancellationTokenSource _connectionCancellationTokenSource;

        private void EnsureEventListenerTaskStarted()
        {
            if (_eventListenerTask == null)
            {
                _connectionCancellationTokenSource = new CancellationTokenSource();
                _eventListenerTask = new Task(() => HandleServiceEvents(_connectionCancellationTokenSource.Token),
                    _connectionCancellationTokenSource.Token, TaskCreationOptions.LongRunning);
                _eventListenerTask.Start();
            }
        }

        private void StopEventListenerTask()
        {
            try
            {
                if (_connectionCancellationTokenSource != null)
                {
                    _connectionCancellationTokenSource.Cancel();
                    _eventListenerTask.Wait();
                }
            }
            catch
            {
            }
            finally
            {
                if (_eventListenerTask != null)
                {
                    _eventListenerTask.Dispose();
                    _eventListenerTask = null;
                }
                if (_connectionCancellationTokenSource != null)
                {
                    _connectionCancellationTokenSource.Dispose();
                    _connectionCancellationTokenSource = null;
                }
            }
        }

        private int _eventsErrorCount = 0;

        private void HandleServiceEvents(CancellationToken cancellationToken)
        {
            for (;;)
            {
                if (Proxies.IsInitialized)
                {
                    IList<ServiceEvent> events = null;
                    if (!_eventListenerSubscribed)
                    {
                        try
                        {
                            Proxies.CoreService.SubscribeServiceEvents(_eventsClientId, EventGroup.RecordingEvents | EventGroup.ScheduleEvents | EventGroup.SystemEvents).Wait();
                            _eventListenerSubscribed = true;
                            _eventsErrorCount = 0;
                        }
                        catch
                        {
                        }
                    }
                    if (_eventListenerSubscribed)
                    {
                        try
                        {
                            events = Proxies.CoreService.GetServiceEvents(_eventsClientId, cancellationToken).Result;
                            if (events == null)
                            {
                                _eventListenerSubscribed = false;
                            }
                            else
                            {
                                ProcessEvents(events);
                            }
                        }
                        catch
                        {
                            if (++_eventsErrorCount > 5)
                            {
                                _eventListenerSubscribed = false;
                            }
                        }
                    }
                }
                if (cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(_eventListenerSubscribed ? 0 : 2)))
                {
                    break;
                }
            }

            if (Proxies.IsInitialized
                && _eventListenerSubscribed)
            {
                try
                {
                    Proxies.CoreService.UnsubscribeServiceEvents(_eventsClientId).Wait();
                }
                catch
                {
                }
                _eventListenerSubscribed = false;
            }
        }

        private void ProcessEvents(IList<ServiceEvent> events)
        {
            foreach (var @event in events)
            {
                if (@event.Name == ServiceEventNames.UpcomingRecordingsChanged)
                {
                    OnUpcomingRecordingsChanged();
                }
                else if (@event.Name == ServiceEventNames.UpcomingAlertsChanged)
                {
                    OnUpcomingAlertsChanged();
                }
                else if (@event.Name == ServiceEventNames.RecordingStarted)
                {
                    OnRecordingStarted((Recording)@event.Arguments[0]);
                }
                else if (@event.Name == ServiceEventNames.RecordingEnded)
                {
                    OnRecordingEnded((Recording)@event.Arguments[0]);
                }
                else if (@event.Name == ServiceEventNames.ConfigurationChanged)
                {
                    OnConfigurationChanged((string)@event.Arguments[0], (string)@event.Arguments[1]);
                }
                else if (@event.Name == ServiceEventNames.LiveStreamStarted
                    || @event.Name == ServiceEventNames.LiveStreamTuned
                    || @event.Name == ServiceEventNames.LiveStreamEnded
                    || @event.Name == ServiceEventNames.LiveStreamAborted)
                {
                    OnLiveStreamChanged((LiveStream)@event.Arguments[0]);
                }
            }
        }

        private void OnUpcomingRecordingsChanged()
        {
            RefreshBotPersonalMessage();
        }

        private void OnUpcomingAlertsChanged()
        {
            lock (_alertsLock)
            {
                _upcomingAlerts = null;
                _wakeUpEvent.Set();
            }
        }

        private void OnRecordingStarted(Recording recording)
        {
            lock (_notificationsLock)
            {
                EnsureNotificationContactFilter();
            }
            BroadcastRecording(_notificationContactFilter, "Recording started", recording, true);
            RefreshBotPersonalMessage();
        }

        private void OnRecordingEnded(Recording recording)
        {
            lock (_notificationsLock)
            {
                EnsureNotificationContactFilter();
            }
            BroadcastRecording(_notificationContactFilter, "Recording ended", recording, false);
            RefreshBotPersonalMessage();
        }

        private void OnLiveStreamChanged(LiveStream liveStream)
        {
            RefreshBotPersonalMessage();
        }

        private void OnConfigurationChanged(string module, string key)
        {
            if (module == ConfigurationModule.Messenger)
            {
                switch (key)
                {
                    case ConfigurationKey.Messenger.MsnContactList:
                        _contactsValidated = false;
                        _wakeUpEvent.Set();
                        break;

                    case ConfigurationKey.Messenger.MsnAccount:
                    case ConfigurationKey.Messenger.MsnPassword:
                        DisconnectMessenger();
                        Thread.Sleep(250);
                        _wakeUpEvent.Set();
                        break;

                    case ConfigurationKey.Messenger.MinutesBeforeAlert:
                        UpdateAlertMinutesSetting();
                        break;

                    case ConfigurationKey.Messenger.MsnAlertFilterList:
                        lock (_alertsLock)
                        {
                            _alertContactFilter = null;
                        }
                        break;
                }
            }
        }

        #endregion
    }
}
