namespace ESCPOS_NET;

public class PrinterOptions
{
    private int IdleTimeoutSeconds { get; set; } = 60;
    private int StatusPollingIntervalMilliseconds { get; set; } = 500;
}
