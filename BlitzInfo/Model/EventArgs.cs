using System;
using System.Collections.Generic;
using BlitzInfo.Model.Entities;

namespace BlitzInfo.Model
{
    public class BlitzEventArgs : EventArgs
    {
        public enum EventMood
        {
            OK,
            AWAIT,
            ERR,
            COMMAND,
            INFORMATION,
            SERVER
        }

        private EventMood eventMood;
        public EventMood mood;
        public string msg;
        public string msgheader;
        private string p;
        public bool saved;
        public DateTime timestamp;

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
    }

    public class NoSettlemEventArgs : EventArgs
    {
        public string ProbedAddress;
    }

    public class BadgeEventArgs : EventArgs
    {
        public enum BadgeType
        {
            CONNECTED,
            CONNECTION_ERROR
        }

        public BadgeType badgeType;

        public BadgeEventArgs(BadgeType badgeType)
        {
            this.badgeType = badgeType;
        }
    }

    public class ProcessEventArgs : EventArgs
    {
        public enum ProcessEvent
        {
            PROCESS_FINISHED,
            PROCESS_STARTED
        }

        public int count;
        public string extra;
        public ProcessEvent processevent;
        public Process.ProcessKind processkind;
    }

    public class SingleStrokeReceivedEventArgs : EventArgs
    {
        public Stroke Stroke { get; set; }
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
        public List<Stroke> Strokes { get; set; }
    }
}