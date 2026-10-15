namespace ESCPOS_NET.Tests.Imaging;

public static class RepositoryRoot
{
    public static string Find()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (
            directory is not null
            && !File.Exists(Path.Combine(directory.FullName, "EscPosKit.slnx"))
        )
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new InvalidOperationException(
                "EscPosKit.slnx not found above " + AppContext.BaseDirectory
            );
    }
}
