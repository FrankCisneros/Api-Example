using Microsoft.EntityFrameworkCore;
using ApiProject.Models;
using System.IO;

namespace ApiProject.Services;

public class LogService
{
    private readonly Context _context;
    private readonly string _logFilePath;

    public LogService(Context context)
    {
        _context = context;
        _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");
    }

    public async Task Log(LogLevel level, string message)
    {
        // Local Save File
        using (StreamWriter writer = new StreamWriter(_logFilePath, true))
        {
            await writer.WriteLineAsync($"{DateTime.Now}: {level} - {message}");
        }

        // Database Save
        var log = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level.ToString(),
            Message = message
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();
    }
}

public enum LogLevel
{
    Info,
    Warning,
    Error
}