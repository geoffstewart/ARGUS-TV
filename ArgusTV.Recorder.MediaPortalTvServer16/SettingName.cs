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
using System.Text;

namespace ArgusTV.Recorder.MediaPortalTvServer
{
    internal static class SettingName
    {
        public const string ServerName = "ArgusTV_ServerName";
        public const string Port = "ArgusTV_Port";
        public const string ResetTvServerOnResume = "ArgusTV_ResetTvServerOnResume";
        public const string EpgSyncOn = "ArgusTV_EpgSyncOn";
        public const string EpgSyncAutoCreateChannels = "ArgusTV_EpgSyncAutoCreateChannels";
        public const string EpgSyncAutoCreateChannelsWithGroup = "ArgusTV_EpgSyncAutoCreateChannelsWithGroup";
        public const string EpgSyncAllHours = "ArgusTV_EpgSyncAllHours";
        public const string RecorderTunerTcpPort = "ArgusTV_RecorderTunerTcpPort";
    }
}