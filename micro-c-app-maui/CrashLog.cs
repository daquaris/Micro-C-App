using System;
using System.IO;

namespace micro_c_app_maui;

// TEMPORARY diagnostic logger for tracking down the scanner "works once, fails on reopen" bug -
// writes to a small local file that ScannerPage displays on-screen, so a failure can be reported
// back without needing adb. Remove once the root cause is confirmed and fixed.
public static class CrashLog
{
    private static readonly string LogFilePath = Path.Combine(FileSystem.AppDataDirectory, "scanner-diagnostic.log");
    private static readonly object Sync = new();

    public static void Write(string message)
    {
        try
        {
            lock (Sync)
            {
                File.AppendAllText(LogFilePath, $"[{DateTime.Now:T}] {message}{Environment.NewLine}");

                var info = new FileInfo(LogFilePath);
                if (info.Exists && info.Length > 64 * 1024)
                {
                    var lines = File.ReadAllLines(LogFilePath);
                    var keep = lines.Length > 200 ? lines[^200..] : lines;
                    File.WriteAllLines(LogFilePath, keep);
                }
            }
        }
        catch
        {
            // Diagnostic logging must never itself crash the app.
        }
    }

    public static string ReadLast(int lines = 8)
    {
        try
        {
            lock (Sync)
            {
                if (!File.Exists(LogFilePath))
                {
                    return "(no diagnostic log entries yet)";
                }

                var all = File.ReadAllLines(LogFilePath);
                var tail = all.Length > lines ? all[^lines..] : all;
                return tail.Length > 0 ? string.Join(Environment.NewLine, tail) : "(log empty)";
            }
        }
        catch (Exception ex)
        {
            return $"(failed to read log: {ex.Message})";
        }
    }
}
