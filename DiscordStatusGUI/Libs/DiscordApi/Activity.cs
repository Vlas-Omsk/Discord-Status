using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordStatusGUI.Libs.DiscordApi
{
    public struct Activity
    {
        public string ProfileName { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public string State { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string ApplicationID { get; set; }
        public string ImageLargeKey { get; set; }
        public string ImageLargeText { get; set; }
        public string ImageSmallKey { get; set; }
        public string ImageSmallText { get; set; }
        public string PartySize { get; set; }
        public string PartyMax { get; set; }
        public bool IsAvailableForChange { get; set; }

        private ActivityType? _ActivityType;
        public ActivityType ActivityType
        {
            get => (_ActivityType ?? (_ActivityType = ActivityType.Game)).GetValueOrDefault();
            set => _ActivityType = value;
        }

        private object _SavedState;
        public object SavedState
        {
            get => _SavedState ?? (_SavedState = this);
            set => _SavedState = value;
        }

        public override string ToString()
        {
            return ProfileName;
        }
    }
}
