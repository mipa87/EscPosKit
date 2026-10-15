using System.Net;
using System.Net.Sockets;

namespace ESCPOS_NET.Tests.Printers;

public class TcpConnectionTests
{
    [Fact]
    public async Task ReceivedBytesAreExposedThroughReadStreamInOrder()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        // Three separate sends exercise SuperSimpleTcp's buffer reuse between DataReceived events.
        var chunks = new[]
        {
            new byte[] { 1, 2, 3 },
            new byte[] { 4, 5 },
            new byte[] { 6, 7, 8, 9 },
        };
        var serverTask = Task.Run(
            async () =>
            {
                using var client = await listener.AcceptTcpClientAsync();
                var stream = client.GetStream();
                foreach (var chunk in chunks)
                {
                    await stream.WriteAsync(chunk);
                    await stream.FlushAsync();
                    await Task.Delay(50);
                }
            },
            TestContext.Current.CancellationToken
        );

        var connection = new TCPConnection($"127.0.0.1:{port}");
        connection.ConnectWithRetries(2000);
        await serverTask;

        var expected = chunks.SelectMany(c => c).ToArray();
        var actual = new byte[expected.Length];
        var total = 0;
        while (total < expected.Length)
        {
            var read = connection.ReadStream.Read(actual, total, expected.Length - total);
            Assert.True(read > 0, $"ReadStream returned {read} after {total} bytes");
            total += read;
        }

        Assert.Equal(expected, actual);
    }
}
