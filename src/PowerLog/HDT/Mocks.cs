using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HearthMirror;
using HearthMirror.Objects;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Stats;

namespace Hearthstone_Deck_Tracker.Utility.Logging
{
    [DebuggerStepThrough]
    public class Log
    {
        public static void Debug(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
            => Console.WriteLine(msg, LogType.Debug, memberName, sourceFilePath);

        public static void Info(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
            => Console.WriteLine(msg, LogType.Info, memberName, sourceFilePath);

        public static void Warn(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
            => Console.WriteLine(msg, LogType.Warning, memberName, sourceFilePath);

        public static void Error(string msg, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
            => Console.WriteLine(msg, LogType.Error, memberName, sourceFilePath);

        public static void Error(Exception ex, [CallerMemberName] string memberName = "", [CallerFilePath] string sourceFilePath = "")
            => Console.WriteLine(ex.ToString(), LogType.Error, memberName, sourceFilePath);
    }

    [Flags]
    public enum LogType
    {
        Debug,
        Info,
        Warning,
        Error
    }
}

namespace Hearthstone_Deck_Tracker.HsReplay
{
    internal class LogUploader
    {
        public static void Upload(string[] logLines, GameMetaData gameMetaData, GameStats game)
        {
        }
    }
}
namespace Hearthstone_Deck_Tracker.Importing
{
    public static class DeckImporter
    {
        private static ArenaInfo _arenaInfoCache;
        public static ArenaInfo ArenaInfoCache
        {
            get { return _arenaInfoCache ?? (_arenaInfoCache = Reflection.GetArenaDeck()); }
            set { _arenaInfoCache = value; }
        }
    }
}

namespace Hearthstone_Deck_Tracker.Live
{
    internal class LiveDataManager
    {
        public static void WatchBoardState() { }
        public static void Stop() { }
    }
}

namespace Hearthstone_Deck_Tracker.Utility.Analytics
{
    internal class Influx
    {
        public static void OnEndOfGameUploadError(string reason) { }
    }
}

namespace Hearthstone_Deck_Tracker.Utility.Extensions
{
    public static class TaskExtensions
    {
        public static void Forget(this Task task)
        {
        }
    }
}

namespace Hearthstone_Deck_Tracker.Windows
{

}

namespace MahApps.Metro.Controls.Dialogs
{

}

namespace Newtonsoft.Json { }

namespace Hearthstone_Deck_Tracker
{
    public static class Core
    {
        public static string Game;
        public static string Overlay;

        public static void UpdatePlayerCards(Boolean foo) { }
        public static void UpdateOpponentCards(Boolean foo) { }

        public static void UpdatePlayerCards() { }
        public static void UpdateOpponentCards() { }

        public static class MainWindow
        {
            public static int Visibility;
            public static int WindowState;
            public static void ActivateWindow() { }
            public static async Task<string> ShowMessage(string title, string msg) { return ""; }
        }
    }

    public static class Helper
    {
        public static string GetValidFilePath(string dir, string name, string extension)
        {
            var validDir = RemoveInvalidPathChars(dir);
            if (!Directory.Exists(validDir))
                Directory.CreateDirectory(validDir);

            if (!extension.StartsWith("."))
                extension = "." + extension;

            var path = validDir + "\\" + RemoveInvalidFileNameChars(name);
            if (File.Exists(path + extension))
            {
                var num = 1;
                while (File.Exists(path + "_" + num + extension))
                    num++;
                path += "_" + num;
            }

            return path + extension;
        }

        public static string RemoveInvalidPathChars(string s) => RemoveChars(s, Path.GetInvalidPathChars());
        public static string RemoveInvalidFileNameChars(string s) => RemoveChars(s, Path.GetInvalidFileNameChars());
        public static string RemoveChars(string s, char[] c) => new Regex($"[{Regex.Escape(new string(c))}]").Replace(s, "");
    }
}

namespace Hearthstone_Deck_Tracker.Utility.Toasts
{
    public static class ToastManager
    {
        internal static void ShowGameResultToast(string deckName, GameStats game)
        {
        }
    }
}

namespace Hearthstone_Deck_Tracker.Stats.CompiledStats
{
    public class ArenaStats
    {
        public static ArenaStats Instance { get; } = new ArenaStats();
        public void UpdateArenaRuns() { }
        public void UpdateArenaStats() { }
        public void UpdateArenaStatsHighlights() { }
    }
}