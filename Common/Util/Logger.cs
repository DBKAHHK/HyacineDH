using System.Diagnostics;
using Spectre.Console;

namespace EggLink.DanhengServer.Util;

public class Logger(string moduleName)
{
    private static FileInfo? _logFile;
    private static FileInfo? _debugLogFile;
    private static readonly object Lock = new();

    public void Log(string message, LoggerLevel level)
    {
        lock (Lock)
        {
            AnsiConsole.Write(new Markup($"[[[bold deepskyblue3_1]{DateTime.Now:HH:mm:ss}[/]]] " +
                             $"[[[bold hotpink3_1]{moduleName}[/]]] [[[{(ConsoleColor)level}]{level}[/]]] {message.Replace("[", "[[").Replace("]", "]]")}\n"));


            var logMessage = $"[{DateTime.Now:HH:mm:ss}] [{moduleName}] [{level}] {message}";
            PluginEventCommon.InvokeOnConsoleLog(logMessage);

            if (level == LoggerLevel.DEBUG)
            {
                WriteToDebugFile(logMessage);
                return;
            }

            WriteToDebugFile(logMessage);
            WriteToFile(logMessage);
        }
    }

    public void Info(string message, Exception? e = null)
    {
        Log(message, LoggerLevel.INFO);
        if (e != null)
        {
            Log(e.Message, LoggerLevel.INFO);
            Log(e.StackTrace!, LoggerLevel.INFO);
        }
    }

    public void Warn(string message, Exception? e = null)
    {
        Log(message, LoggerLevel.WARN);
        if (e != null)
        {
            Log(e.Message, LoggerLevel.WARN);
            Log(e.StackTrace!, LoggerLevel.WARN);
        }
    }

    public void Error(string message, Exception? e = null)
    {
        Log(message, LoggerLevel.ERROR);
        if (e != null)
        {
            Log(e.Message, LoggerLevel.ERROR);
            Log(e.StackTrace!, LoggerLevel.ERROR);
        }
    }

    public void Fatal(string message, Exception? e = null)
    {
        Log(message, LoggerLevel.FATAL);
        if (e != null)
        {
            Log(e.Message, LoggerLevel.FATAL);
            Log(e.StackTrace!, LoggerLevel.FATAL);
        }
    }

    public void Debug(string message, Exception? e = null)
    {
        Log(message, LoggerLevel.DEBUG);
        if (e != null)
        {
            Log(e.Message, LoggerLevel.DEBUG);
            Log(e.StackTrace!, LoggerLevel.DEBUG);
        }
    }

    public static void SetLogFile(FileInfo file)
    {
        _logFile = file;
    }

    public static void SetDebugLogFile(FileInfo file)
    {
        _debugLogFile = file;
    }

    public static void WriteToFile(string message)
    {
        try
        {
            if (_logFile == null) throw new Exception("LogFile is not set");
            using var sw = _logFile.AppendText();
            sw.WriteLine(message);
        }
        catch
        {
        }
    }

    public static void WriteToDebugFile(string message)
    {
        try
        {
            if (_debugLogFile == null) throw new Exception("DebugLogFile is not set");
            using var sw = _debugLogFile.AppendText();
            sw.WriteLine(message);
        }
        catch
        {
        }
    }

    public static Logger GetByClassName()
    {
        return new Logger(new StackTrace().GetFrame(1)?.GetMethod()?.ReflectedType?.Name ?? "");
    }
}

public enum LoggerLevel
{
    INFO = ConsoleColor.Cyan,
    WARN = ConsoleColor.Yellow,
    ERROR = ConsoleColor.Red,
    FATAL = ConsoleColor.DarkRed,
    DEBUG = ConsoleColor.Blue
}