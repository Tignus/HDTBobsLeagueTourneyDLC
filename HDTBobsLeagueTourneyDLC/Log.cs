using System;
using System.Runtime.CompilerServices;
using HdtLog = Hearthstone_Deck_Tracker.Utility.Logging.Log;

namespace HDTBobsLeagueTourneyDLC
{
    public class Log
    {
        private const string PREFIX = "Bob's League DLC Plugin - ";

        public static void Debug(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
        {
            HdtLog.Debug(PREFIX + message, memberName, sourceFilePath);
        }

        public static void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
        {
            HdtLog.Info(PREFIX + message, memberName, sourceFilePath);
        }

        public static void Warn(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
        {
            HdtLog.Warn(PREFIX + message, memberName, sourceFilePath);
        }

        public static void Error(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
        {
            HdtLog.Error(PREFIX + message, memberName, sourceFilePath);
        }

        public static void Error(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
        {
            HdtLog.Error(PREFIX + ex.ToString(), memberName, sourceFilePath);
        }
    }
}
