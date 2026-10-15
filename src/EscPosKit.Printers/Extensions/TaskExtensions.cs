using Microsoft.Extensions.Logging;

namespace ESCPOS_NET.Extensions;

public static class TaskExtensions
{
    public static void LogExceptions(this Task task)
    {
        task.ContinueWith(
            t =>
            {
                var aggException = t.Exception.Flatten();
                foreach (var exception in aggException.InnerExceptions)
                {
                    Logging.Logger?.LogError(exception, "Uncaught Thread Exception.");
                }
            },
            TaskContinuationOptions.OnlyOnFaulted
        );
    }
}
