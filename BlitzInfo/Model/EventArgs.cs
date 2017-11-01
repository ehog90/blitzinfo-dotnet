using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using BlitzInfo.Model.Entities;

namespace BlitzInfo.Model
{
    public class BlitzEventArgs : EventArgs
    {
        public BlitzEventArgs(string _msg, string _msgheader, EventMood _mood)
        {
            msgheader = _msgheader;
            msg = _msg;
            mood = _mood;
            timestamp = DateTime.Now;
            saved = true;
        }
        public BlitzEventArgs(string _msg, string _msgheader, EventMood _mood, bool save)
        {
            msgheader = _msgheader;
            msg = _msg;
            mood = _mood;
            timestamp = DateTime.Now;
            saved = save;
        }
        public enum EventMood { OK, AWAIT, ERR, COMMAND, INFORMATION, SERVER};
        public string msg;
        public string msgheader;
        public EventMood mood;
        public DateTime timestamp;
        public bool saved;
        private string p;
        private EventMood eventMood;
    }

    public class NoSettlemEventArgs : EventArgs
    {
        public string ProbedAddress;
    }

    public class BadgeEventArgs : EventArgs
    {
        public enum BadgeType {CONNECTED,CONNECTION_ERROR};
        public BadgeType badgeType;
        public BadgeEventArgs(BadgeType badgeType)
        {
            this.badgeType = badgeType;
        }

    }
    
    public class ProcessEventArgs : EventArgs
    {
        public int count;
        public enum ProcessEvent {PROCESS_FINISHED, PROCESS_STARTED}
        public ProcessEvent processevent;
        public Process.ProcessKind processkind;
        public string extra;
    }
    public class SingleStrokeReceivedEventArgs : EventArgs
    {
        public Entities.Stroke Stroke { get; set; }
    }
    public class SingleServerLogEventArgs : EventArgs
    {
        public ServerLog ServerLog { get; set; }
    }

    public class MultipleServerLogEventArgs : EventArgs
    {
        public List<ServerLog> ServerLogs { get; set; }
    }
    public class MultipleStrokeReceivedEventArgs : EventArgs
    {
        public List<Entities.Stroke> Strokes { get; set; }
    }
}
