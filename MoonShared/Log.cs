using System.Diagnostics;
using StardewModdingAPI;

namespace MoonShared
{
	internal class Log
	{
		public static IMonitor Monitor;

        public static void Init(IMonitor monitor)
        {
            Monitor = monitor;
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public static void DebugOnlyLog(string str)
        {
            Log.Monitor.Log(str, LogLevel.Debug);
        }

        [DebuggerHidden]
        [Conditional("DEBUG")]
        public static void DebugOnlyLog(string str, bool pred)
        {
            if (pred)
                Log.Monitor.Log(str, LogLevel.Debug);
        }

        public static void Debug(string str, bool isDebug = true)
		{
			Monitor.Log(str,
				isDebug ? LogLevel.Debug : LogLevel.Trace);
		}
		public static void Alert(string str)
		{
			Monitor.Log(str, LogLevel.Alert);
		}
		public static void Error(string str)
		{
			Monitor.Log(str, LogLevel.Error);
		}
		public static void Info(string str)
		{
			Monitor.Log(str, LogLevel.Info);
		}
		public static void Trace(string str)
		{
			Monitor.Log(str, LogLevel.Trace);
		}
		public static void Warn(string str)
		{
			Monitor.Log(str, LogLevel.Warn);
		}
	}
}
