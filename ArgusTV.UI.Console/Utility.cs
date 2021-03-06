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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

using Microsoft.Win32;

using ArgusTV.Common;
using ArgusTV.DataContracts;
using ArgusTV.UI.Console.Panels;
using ArgusTV.UI.Console.Wizards;
using ArgusTV.UI.Process;
using ArgusTV.ServiceProxy;

namespace ArgusTV.UI.Console
{
    internal static class Utility
    {
        public static int SetDateTimePickerValue(MainForm mainForm, DateTimePicker dateTimePicker, string key)
        {
            int? seconds = Proxies.ConfigurationService.GetIntValue(ConfigurationModule.Scheduler, key).Result;
            if (!seconds.HasValue)
            {
                seconds = 0;
            }
            dateTimePicker.Value = dateTimePicker.MinDate.AddSeconds((double)seconds.Value);
            return seconds.Value;
        }

        public static Recording CreateSampleRecording()
        {
            Recording sampleRecording = new Recording();
            sampleRecording.Title = "The Flintstones";
            sampleRecording.SubTitle = "Son of Rockzilla";
            sampleRecording.SeriesNumber = 4;
            sampleRecording.EpisodeNumber = 24;
            sampleRecording.EpisodeNumberDisplay = "24";
            sampleRecording.ProgramStartTime = DateTime.Today.AddHours(16);
            sampleRecording.ProgramStopTime = sampleRecording.ProgramStartTime.AddMinutes(30);
            sampleRecording.RecordingStartTime = sampleRecording.ProgramStartTime.AddMinutes(-2);
            sampleRecording.RecordingStopTime = sampleRecording.ProgramStopTime.AddMinutes(5);
            sampleRecording.ChannelDisplayName = "BBC1";
            sampleRecording.ScheduleName = "Flintstones (Any Time)";
            sampleRecording.Category = "Animation";
            sampleRecording.Flags |= GuideProgramFlags.StandardAspectRatio;
            return sampleRecording;
        }

        public static void ShowRecordingFormatsHelp(IWin32Window owner)
        {
            MessageBox.Show(owner, "The following variables can be used as %%VAR%% :" + Environment.NewLine + Environment.NewLine +
                "CHANNEL\tThe channel name" + Environment.NewLine +
                "SCHEDULE\tThe schedule name" + Environment.NewLine +
                "CATEGORY\tThe program category or '#' if none" + Environment.NewLine +
                "TITLE\t\tThe title (no episode title and/or number)" + Environment.NewLine +
                "LONGTITLE\tThe title including episode and/or number" + Environment.NewLine +
                "EPISODETITLE\tThe episode title or '#' if none" + Environment.NewLine +
                "EPISODENUMBERDISPLAY\tThe episode number as displayed or '#' if none" + Environment.NewLine +
                "EPISODENUMBER\tThe episode number or '#' if none" + Environment.NewLine +
                "EPISODENUMBER2\tThe episode number in two digits or '00' if none" + Environment.NewLine +
                "EPISODENUMBER3\tThe episode number in three digits or '000' if none" + Environment.NewLine +
                "SERIES\t\tThe series/season number or '#' if none" + Environment.NewLine +
                "SERIES2\tThe series/season number in two digits or '00' if none" + Environment.NewLine +
                "DATE\t\tThe airing date in the format yyyy-MM-dd" + Environment.NewLine +
                "YEAR\t\tThe airing year" + Environment.NewLine +
                "MONTH\t\tThe airing month as a number" + Environment.NewLine +
                "DAY\t\tThe airing day" + Environment.NewLine +
                "DAYOFWEEK\tThe airing day of the week" + Environment.NewLine +
                "HOURS\t\tThe airing time's hours in 24H format" + Environment.NewLine +
                "HOURS12\t\tThe airing time's hours in 12H format AM/PM" + Environment.NewLine +
                "MINUTES\t\tThe airing time's minutes", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void EnsureMinimumTime(DateTime startTime, int milliseconds)
        {
            TimeSpan loadingTime = DateTime.Now - startTime;
            TimeSpan minimumLoadingTime = new TimeSpan(0, 0, 0, 0, milliseconds);
            if (loadingTime < minimumLoadingTime)
            {
                System.Threading.Thread.Sleep(minimumLoadingTime - loadingTime);
            }
        }

        #region Recordings Metadata

        private const string _metaDataStreamName = "ArgusTV.xml";
        private const string _metaDataExtension = ".arg";

        private static XmlSerializer _recordingSerializer;
        private static XmlSerializer _tvRecordingSerializer;

        private static XmlSerializer RecordingSerializer
        {
            get
            {
                if (_recordingSerializer == null)
                {
                    _recordingSerializer = new XmlSerializer(typeof(Recording));
                }
                return _recordingSerializer;
            }
        }

        private static XmlSerializer TvRecordingSerializer
        {
            get
            {
                if (_tvRecordingSerializer == null)
                {
                    _tvRecordingSerializer = new XmlSerializer(typeof(TvRecording));
                }
                return _tvRecordingSerializer;
            }
        }

        public static Recording GetRecordingMetadata(string recordingFileName)
        {
            FileStream stream = OpenRecordingAdsStream(recordingFileName, _metaDataStreamName);
            if (stream == null)
            {
                stream = GetRecordingMetaDataFromMetaDataFile(recordingFileName, _metaDataExtension);
                if (stream == null)
                {
                    stream = OpenRecordingAdsStream(recordingFileName, "ForTheRecord.xml");
                    if (stream == null)
                    {
                        stream = GetRecordingMetaDataFromMetaDataFile(recordingFileName, ".4tr");
                    }
                }
            }
            return GetRecordingMetadataFromStream(stream);
        }

        public static void WriteRecordingMetadataFile(string destinationFileName, Recording recording)
        {
            try
            {
                using (Stream stream = new FileStream(Path.Combine(Path.GetDirectoryName(destinationFileName),
                        Path.GetFileNameWithoutExtension(destinationFileName) + _metaDataExtension),
                    FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
                {
                    _recordingSerializer.Serialize(stream, recording);
                    stream.Close();
                }
            }
            catch { }
        }

        private static FileStream GetRecordingMetaDataFromMetaDataFile(string recordingFileName, string metaDataExtension)
        {
            FileStream stream = null;
            try
            {
                stream = new FileStream(Path.Combine(Path.GetDirectoryName(recordingFileName),
                    Path.GetFileNameWithoutExtension(recordingFileName) + metaDataExtension), FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch { }
            return stream;
        }

        public static Recording GetRecordingMetadataFromAds(string recordingFileName)
        {
            FileStream stream = OpenRecordingAdsStream(recordingFileName, _metaDataStreamName);
            if (stream == null)
            {
                stream = OpenRecordingAdsStream(recordingFileName, "ForTheRecord.xml");
            }
            return GetRecordingMetadataFromStream(stream);
        }

        private static FileStream OpenRecordingAdsStream(string recordingFileName, string metaDataStreamName)
        {
            FileStream stream = null;
            try
            {
                FileAdsStreams adsStreams = new FileAdsStreams(recordingFileName);
                adsStreams.Add(metaDataStreamName);
                stream = adsStreams[metaDataStreamName].Open(FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch
            {
            }
            return stream;
        }

        private static Recording GetRecordingMetadataFromStream(FileStream stream)
        {
            Recording recording = null;
            bool setUtcTimes = false;
            if (stream != null)
            {
                try
                {
                    recording = (Recording)RecordingSerializer.Deserialize(stream);
                    setUtcTimes = recording.ProgramStartTimeUtc.Year <= 1900;
                }
                catch
                {
                    try
                    {
                        stream.Position = 0;
                        TvRecording tvRecording = (TvRecording)TvRecordingSerializer.Deserialize(stream);
                        recording = new Recording()
                        {
                            Actors = tvRecording.Actors,
                            Category = tvRecording.Category,
                            ChannelDisplayName = tvRecording.TvChannelDisplayName,
                            ChannelId = tvRecording.TvChannelId,
                            ChannelType = tvRecording.ChannelType,
                            Description = tvRecording.Description,
                            Director = tvRecording.Director,
                            EpisodeNumber = tvRecording.EpisodeNumber,
                            EpisodeNumberDisplay = tvRecording.EpisodeNumberDisplay,
                            EpisodeNumberTotal = tvRecording.EpisodeNumberTotal,
                            EpisodePart = tvRecording.EpisodePart,
                            EpisodePartTotal = tvRecording.EpisodePartTotal,
                            IsPartialRecording = tvRecording.IsPartialRecording,
                            IsPartOfSeries = tvRecording.IsPartOfSeries,
                            IsPremiere = tvRecording.IsPremiere,
                            IsRepeat = tvRecording.IsRepeat,
                            KeepUntilMode = tvRecording.KeepUntilMode,
                            KeepUntilValue = tvRecording.KeepUntilValue,
                            LastWatchedPosition = tvRecording.LastWatchedPosition,
                            LastWatchedTime = tvRecording.LastWatchedTime,
                            ProgramStartTime = tvRecording.ProgramStartTime,
                            ProgramStopTime = tvRecording.ProgramStopTime,
                            Rating = tvRecording.Rating,
                            RecordingId = tvRecording.TvRecordingId,
                            RecordingStartTime = tvRecording.RecordingStartTime,
                            RecordingStopTime = tvRecording.RecordingStopTime,
                            ScheduleId = tvRecording.TvScheduleId,
                            ScheduleName = tvRecording.TvScheduleName,
                            SchedulePriority = tvRecording.SchedulePriority,
                            SeriesNumber = tvRecording.SeriesNumber,
                            StarRating = tvRecording.StarRating,
                            SubTitle = tvRecording.SubTitle,
                            Title = tvRecording.Title
                        };
                        setUtcTimes = true;
                    }
                    catch
                    {
                        recording = null;
                    }
                }
                finally
                {
                    stream.Close();
                    stream.Dispose();
                }

                if (setUtcTimes)
                {
                    recording.ProgramStartTimeUtc = recording.ProgramStartTime.ToUniversalTime();
                    recording.ProgramStopTimeUtc = recording.ProgramStopTime.ToUniversalTime();
                    recording.RecordingStartTimeUtc = recording.RecordingStartTime.ToUniversalTime();
                    if (recording.RecordingStopTime.HasValue)
                    {
                        recording.RecordingStopTimeUtc = recording.RecordingStopTime.Value.ToUniversalTime();
                    }
                }
            }
            return recording;
        }

        #endregion

        #region Program Context Menu

        public static void ContextCreateNewSchedule(ContentPanel panel, ArgusTV.WinForms.Controls.ProgramContextMenuStrip.CreateNewScheduleEventArgs e)
        {
            EditSchedulePanel editPanel = new EditSchedulePanel();
            editPanel.Schedule = e.Schedule;
            editPanel.OpenPanel(panel);
        }

        public static void ContextEditSchedule(ContentPanel panel, ArgusTV.WinForms.Controls.ProgramContextMenuStrip.EditScheduleEventArgs e)
        {
            try
            {
                EditSchedulePanel editPanel = new EditSchedulePanel();
                editPanel.Schedule = Proxies.SchedulerService.GetScheduleById(e.ScheduleId).Result;
                editPanel.OpenPanel(panel);
            }
            catch (Exception ex)
            {
                MessageBox.Show(panel, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static bool ContextDeleteSchedule(ContentPanel panel, ArgusTV.WinForms.Controls.ProgramContextMenuStrip.EditScheduleEventArgs e)
        {
            try
            {
                if (DialogResult.Yes == MessageBox.Show(panel, "Are you sure you want to delete this recording's schedule?", "Delete Schedule", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                {
                    Proxies.SchedulerService.DeleteSchedule(e.ScheduleId);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(panel, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public static bool ContextCancelProgram(ContentPanel panel, ArgusTV.WinForms.Controls.ProgramContextMenuStrip.CancelProgramEventArgs e)
        {
            try
            {
                if (e.Cancel)
                {
                    Proxies.SchedulerService.CancelUpcomingProgram(e.ScheduleId, e.GuideProgramId, e.ChannelId, e.StartTime);
                }
                else
                {
                    Proxies.SchedulerService.UncancelUpcomingProgram(e.ScheduleId, e.GuideProgramId, e.ChannelId, e.StartTime);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(panel, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public static bool ContextAddRemoveProgramHistory(ContentPanel panel, ArgusTV.WinForms.Controls.ProgramContextMenuStrip.AddRemoveProgramHistoryEventArgs e)
        {
            try
            {
                if (e.AddToHistory)
                {
                    if (DialogResult.Yes == MessageBox.Show(panel, "Are you sure you want to add this recording to its schedule's" + Environment.NewLine + "history of previously recorded programs?", "Add To History", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                    {
                        Proxies.ControlService.AddToPreviouslyRecordedHistory(e.UpcomingProgram);
                        return true;
                    }
                }
                else
                {
                    if (DialogResult.Yes == MessageBox.Show(panel, "Are you sure you want to remove this recording from its schedule's" + Environment.NewLine + "history of previously recorded programs?", "Add To History", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2))
                    {
                        Proxies.ControlService.RemoveFromPreviouslyRecordedHistory(e.UpcomingProgram);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(panel, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public static bool ContextSetProgramPriority(ContentPanel panel, ArgusTV.WinForms.Controls.ProgramContextMenuStrip.SetProgramPriorityEventArgs e)
        {
            try
            {
                Proxies.SchedulerService.SetUpcomingProgramPriority(e.UpcomingProgramId, e.StartTime, e.Priority);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(panel, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public static bool ContextSetProgramPrePostRecord(ContentPanel panel, ArgusTV.WinForms.Controls.ProgramContextMenuStrip.SetProgramPrePostRecordEventArgs e)
        {
            try
            {
                if (e.IsPreRecord)
                {
                    Proxies.SchedulerService.SetUpcomingProgramPreRecord(e.UpcomingProgramId, e.StartTime, e.Seconds);
                }
                else
                {
                    Proxies.SchedulerService.SetUpcomingProgramPostRecord(e.UpcomingProgramId, e.StartTime, e.Seconds);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(panel, ex.Message, null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        #endregion
    }
}
