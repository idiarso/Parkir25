namespace ParkIRC.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
