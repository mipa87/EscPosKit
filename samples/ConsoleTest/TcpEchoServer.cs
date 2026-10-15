using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ESCPOS_NET.ConsoleTest;

// Minimal stand-in for a network printer: echoes "E" for every line received so the
// NetworkPrinter status loop has something to read. Adapted from an MSDN sample.
internal static class TcpEchoServer
{
    public static void Run(int port)
    {
        Console.WriteLine($"Starting echo server on 127.0.0.1:{port} (Ctrl+C to stop)...");
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();

        while (true)
        {
            using var client = listener.AcceptTcpClient();
            using var stream = client.GetStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            using var reader = new StreamReader(stream, Encoding.ASCII);

            string inputLine;
            while ((inputLine = reader.ReadLine()) is not null)
            {
                writer.Write("E");
                Console.WriteLine("Echoing string: " + inputLine);
            }

            Console.WriteLine("Server saw disconnect from client.");
        }
    }
}
