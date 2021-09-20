using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

string[] urls = new string[] { "wss://stream.binance.com:9443/ws", "wss://dstream.binance.com/ws", "wss://fstream.binance.com/ws" };
//urls = urls.Concat(urls).ToArray();
//urls = urls.Concat(urls).ToArray();
List<WS> WebSockets = new List<WS>();

CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken ct = cts.Token;

Process p = Process.GetProcessesByName("WebSocketIssueDemo")[0];
double prevCpuMillis = p.TotalProcessorTime.TotalMilliseconds;

int ncores = Environment.ProcessorCount;
Console.WriteLine(ncores);
long prevMessageCount = 0;

for (int i = 0; i < 100; i++)
{
    for (int j = 0; j < 3; j++)
    {
        WS wS = new WS(urls[j]);
        wS.DoTrivialWork(ct);
        WebSockets.Add(wS);
    }
    double currentMillis = p.TotalProcessorTime.TotalMilliseconds;
    long messageCount = WS.Nmessages;
    double cpuUsage = Convert.ToInt64(0.25 * (currentMillis - prevCpuMillis) /  ncores) / 10;
    Console.WriteLine($"Number Sockets: {WS.Nsockets}, CPU Usage: {cpuUsage}%, Number messages/s: {0.25*(messageCount - prevMessageCount)}");
    prevMessageCount = messageCount;
    prevCpuMillis = currentMillis;
    Thread.Sleep(4000);
}

cts.Cancel();
Console.Read();
class WS
{
    ClientWebSocket ws;
    Uri uri;
    public static long Nsockets = 0;
    public static long Nmessages = 0;
    static string submessage = "{\"method\": \"SUBSCRIBE\", \"params\": [\"!bookTicker\"], \"id\": 0}";
    static string processName = "WebSocketIssueDemo";
    static Process p = Process.GetProcessesByName(processName)[0];
    static double previousCpuMillis = p.TotalProcessorTime.TotalMilliseconds;
    public WS(string url)
    {
        uri = new Uri(url);
        ws = new ClientWebSocket();
    }
    public async void DoTrivialWork(CancellationToken ct)
    {
        Interlocked.Increment(ref Nsockets);
        await ws.ConnectAsync(uri, ct);
        await ws.SendAsync(Encoding.UTF8.GetBytes(submessage), WebSocketMessageType.Text, true, ct);
        Memory<byte> buffer = new byte[1024];
        ValueWebSocketReceiveResult R;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                R = await ws.ReceiveAsync(buffer, ct);
                long N = Interlocked.Increment(ref Nmessages);
            }
        }
        catch (Exception ex)
        {
            Interlocked.Decrement(ref Nsockets);
        }
    }
};

//urls = urls.Concat(urls).ToArray();
//urls = urls.Concat(urls).ToArray();
//WS[] websockets = urls.Select(u => new WS(u)).ToArray();
//CancellationTokenSource cts = new CancellationTokenSource();
//CancellationToken ct = cts.Token;

//for (int i = 0; i < urls.Length; i++)
//{
//    websockets[i].DoTrivialWork(ct);
//    Thread.Sleep(250);
//}

//Process p = Process.GetProcessesByName("WebSocketIssueDemo")[0];
//double startCpuMillis = p.TotalProcessorTime.TotalMilliseconds;
//Thread.Sleep(5 * 1000); //let everything warm up
//startCpuMillis = p.TotalProcessorTime.TotalMilliseconds;
//Thread.Sleep(30 * 1000); //measure for 30 seconds
//double TotalCpuMillis = p.TotalProcessorTime.TotalMilliseconds - startCpuMillis;
//cts.Cancel();
//Console.WriteLine($"Average processor time in microseconds per message over 20 seconds: {1000 * TotalCpuMillis / WS.Nmessages}");
//Console.Read();

//class WS
//{
//    ClientWebSocket ws;
//    Uri uri;
//    public static long Nmessages = 0;
//    static string submessage = "{\"method\": \"SUBSCRIBE\", \"params\": [\"!bookTicker\"], \"id\": 0}";
//    static string processName = "WebSocketIssueDemo";
//    static Process p = Process.GetProcessesByName(processName)[0];
//    static double previousCpuMillis = p.TotalProcessorTime.TotalMilliseconds;
//    public WS(string url)
//    {
//        uri = new Uri(url);
//        ws = new ClientWebSocket();
//    }
//    public async void DoTrivialWork(CancellationToken ct)
//    {
//        await ws.ConnectAsync(uri, ct);
//        await ws.SendAsync(Encoding.UTF8.GetBytes(submessage), WebSocketMessageType.Text, true, ct);
//        Memory<byte> buffer = new byte[1024];
//        ValueWebSocketReceiveResult R;
//        try
//        {
//            while (!ct.IsCancellationRequested)
//            {
//                R = await ws.ReceiveAsync(buffer, ct);
//                long N = Interlocked.Increment(ref Nmessages);
//                if (N % 32768 == 0)
//                    PrintCpuTime();
//            }
//        }
//        catch (Exception ex) { }
//    }
//    static void PrintCpuTime()
//    {
//        double cpuMillis = p.TotalProcessorTime.TotalMilliseconds;
//        Console.Write($"CPU usage ms per 32k messages: {cpuMillis - previousCpuMillis}      \r");
//        previousCpuMillis = cpuMillis;
//    }
//};