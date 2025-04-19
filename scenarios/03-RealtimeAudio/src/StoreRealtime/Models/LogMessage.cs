namespace StoreRealtime.Models;
public class LogMessage
{
    public LogMessage(string message) {
        Message = message;
        DateTime = DateTime.Now;
    }

    public string Message { get; set; } = string.Empty;
    public DateTime DateTime { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return DateTime.ToString("G") + " - " + Message;
    }
}
