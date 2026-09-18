using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PosCs.Hosting
{
    public sealed class TcpOwinHost : IDisposable
    {
        private readonly TcpListener _listener;
        private readonly Func<IDictionary<string, object>, Task> _app;
        private readonly CancellationTokenSource _stopping;
        private readonly Task _acceptLoop;
        private bool _disposed;

        private TcpOwinHost(Uri uri, Func<IDictionary<string, object>, Task> app)
        {
            _app = app;
            _stopping = new CancellationTokenSource();
            _listener = new TcpListener(IPAddress.Loopback, uri.Port);
            _listener.Start();
            _acceptLoop = Task.Run((Func<Task>)AcceptLoop);
        }

        public static TcpOwinHost Start(string url, Func<IDictionary<string, object>, Task> app)
        {
            return new TcpOwinHost(new Uri(url), app);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _stopping.Cancel();
            _listener.Stop();
            try { _acceptLoop.Wait(1000); } catch { }
            _stopping.Dispose();
        }

        private async Task AcceptLoop()
        {
            while (!_stopping.IsCancellationRequested)
            {
                TcpClient client = null;
                try
                {
                    client = await _listener.AcceptTcpClientAsync();
                    var acceptedClient = client;
                    var ignored = Task.Run(() => HandleClient(acceptedClient));
                    client = null;
                }
                catch
                {
                    if (!_stopping.IsCancellationRequested)
                        Console.Error.WriteLine("[API ERR] TCP host accept loop failed.");
                }
                finally
                {
                    if (client != null) client.Close();
                }
            }
        }

        private async Task HandleClient(TcpClient client)
        {
            try
            {
                using (client)
                using (var stream = client.GetStream())
                {
                    RequestData request;
                    try
                    {
                        request = await ReadRequest(stream);
                        if (request == null) return;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("[API ERR] TCP host failed to read request: " + ex);
                        await WriteError(stream, 400, "Bad Request");
                        return;
                    }

                    var responseBody = new MemoryStream();
                    var responseHeaders = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                    var environment = CreateEnvironment(client, request, responseHeaders, responseBody);

                    try
                    {
                        await _app(environment);
                        await WriteResponse(stream, request.Method, environment, responseHeaders, responseBody);
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("[API ERR] TCP host request failed: " + ex);
                        await WriteError(stream, 500, "Internal Server Error");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("[API ERR] TCP host client failed: " + ex);
            }
        }

        private IDictionary<string, object> CreateEnvironment(TcpClient client, RequestData request, IDictionary<string, string[]> responseHeaders, Stream responseBody)
        {
            var local = (IPEndPoint)client.Client.LocalEndPoint;
            var remote = (IPEndPoint)client.Client.RemoteEndPoint;

            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                { "owin.Version", "1.0" },
                { "owin.RequestMethod", request.Method },
                { "owin.RequestScheme", "http" },
                { "owin.RequestPathBase", "" },
                { "owin.RequestPath", request.Path },
                { "owin.RequestQueryString", request.QueryString },
                { "owin.RequestProtocol", request.Protocol },
                { "owin.RequestHeaders", request.Headers },
                { "owin.RequestBody", request.Body },
                { "owin.CallCancelled", _stopping.Token },
                { "owin.ResponseStatusCode", 200 },
                { "owin.ResponseReasonPhrase", "OK" },
                { "owin.ResponseHeaders", responseHeaders },
                { "owin.ResponseBody", responseBody },
                { "server.RemoteIpAddress", remote.Address.ToString() },
                { "server.RemotePort", remote.Port.ToString(CultureInfo.InvariantCulture) },
                { "server.LocalIpAddress", local.Address.ToString() },
                { "server.LocalPort", local.Port.ToString(CultureInfo.InvariantCulture) }
            };
        }

        private static async Task<RequestData> ReadRequest(Stream stream)
        {
            var headerBytes = new List<byte>();
            var matched = 0;
            var delimiter = new byte[] { 13, 10, 13, 10 };

            while (matched < delimiter.Length)
            {
                var value = stream.ReadByte();
                if (value < 0)
                {
                    if (headerBytes.Count == 0) return null;
                    throw new InvalidOperationException("Unexpected end of request headers.");
                }

                var current = (byte)value;
                headerBytes.Add(current);
                matched = current == delimiter[matched] ? matched + 1 : (current == delimiter[0] ? 1 : 0);

                if (headerBytes.Count > 65536) throw new InvalidOperationException("Request headers are too large.");
            }

            var headerText = Encoding.ASCII.GetString(headerBytes.ToArray());
            var lines = headerText.Split(new[] { "\r\n" }, StringSplitOptions.None);
            if (lines.Length == 0 || string.IsNullOrWhiteSpace(lines[0])) throw new InvalidOperationException("Missing request line.");

            var requestLine = lines[0].Split(' ');
            if (requestLine.Length < 3) throw new InvalidOperationException("Invalid request line.");

            var headers = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrEmpty(line)) continue;
                var separator = line.IndexOf(':');
                if (separator <= 0) continue;
                var name = line.Substring(0, separator).Trim();
                var value = line.Substring(separator + 1).Trim();
                string[] existing;
                if (headers.TryGetValue(name, out existing))
                {
                    var values = new string[existing.Length + 1];
                    Array.Copy(existing, values, existing.Length);
                    values[values.Length - 1] = value;
                    headers[name] = values;
                }
                else
                {
                    headers[name] = new[] { value };
                }
            }

            var bodyLength = GetContentLength(headers);
            var bodyBytes = new byte[bodyLength];
            var offset = 0;
            while (offset < bodyLength)
            {
                var read = await stream.ReadAsync(bodyBytes, offset, bodyLength - offset);
                if (read == 0) throw new InvalidOperationException("Unexpected end of request body.");
                offset += read;
            }

            var target = requestLine[1];
            var queryIndex = target.IndexOf('?');
            var path = queryIndex >= 0 ? target.Substring(0, queryIndex) : target;
            var queryString = queryIndex >= 0 ? target.Substring(queryIndex + 1) : "";

            return new RequestData
            {
                Method = requestLine[0],
                Path = Uri.UnescapeDataString(path),
                QueryString = queryString,
                Protocol = requestLine[2],
                Headers = headers,
                Body = new MemoryStream(bodyBytes)
            };
        }

        private static int GetContentLength(IDictionary<string, string[]> headers)
        {
            string[] values;
            int length;
            if (!headers.TryGetValue("Content-Length", out values) || values.Length == 0) return 0;
            return int.TryParse(values[0], NumberStyles.None, CultureInfo.InvariantCulture, out length) && length > 0 ? length : 0;
        }

        private static async Task WriteResponse(Stream stream, string method, IDictionary<string, object> environment, IDictionary<string, string[]> headers, MemoryStream body)
        {
            var statusCode = environment.ContainsKey("owin.ResponseStatusCode") ? (int)environment["owin.ResponseStatusCode"] : 200;
            var reason = environment.ContainsKey("owin.ResponseReasonPhrase") ? Convert.ToString(environment["owin.ResponseReasonPhrase"], CultureInfo.InvariantCulture) : GetReasonPhrase(statusCode);
            var bodyBytes = body.ToArray();

            if (!headers.ContainsKey("Content-Length")) headers["Content-Length"] = new[] { bodyBytes.Length.ToString(CultureInfo.InvariantCulture) };
            if (!headers.ContainsKey("Connection")) headers["Connection"] = new[] { "close" };

            var writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };
            await writer.WriteLineAsync("HTTP/1.1 " + statusCode.ToString(CultureInfo.InvariantCulture) + " " + reason);
            foreach (var header in headers)
            {
                foreach (var value in header.Value)
                {
                    await writer.WriteLineAsync(header.Key + ": " + value);
                }
            }
            await writer.WriteLineAsync();
            if (!string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase))
            {
                await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
            }
        }

        private static async Task WriteError(Stream stream, int statusCode, string reason)
        {
            var bodyBytes = Encoding.UTF8.GetBytes(reason);
            var writer = new StreamWriter(stream, Encoding.ASCII) { NewLine = "\r\n", AutoFlush = true };
            await writer.WriteLineAsync("HTTP/1.1 " + statusCode.ToString(CultureInfo.InvariantCulture) + " " + reason);
            await writer.WriteLineAsync("Content-Type: text/plain; charset=utf-8");
            await writer.WriteLineAsync("Content-Length: " + bodyBytes.Length.ToString(CultureInfo.InvariantCulture));
            await writer.WriteLineAsync("Connection: close");
            await writer.WriteLineAsync();
            await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
        }

        private static string GetReasonPhrase(int statusCode)
        {
            switch (statusCode)
            {
                case 200: return "OK";
                case 201: return "Created";
                case 204: return "No Content";
                case 301: return "Moved Permanently";
                case 302: return "Found";
                case 304: return "Not Modified";
                case 400: return "Bad Request";
                case 401: return "Unauthorized";
                case 403: return "Forbidden";
                case 404: return "Not Found";
                case 405: return "Method Not Allowed";
                case 500: return "Internal Server Error";
                default: return "OK";
            }
        }

        private sealed class RequestData
        {
            public string Method { get; set; }
            public string Path { get; set; }
            public string QueryString { get; set; }
            public string Protocol { get; set; }
            public IDictionary<string, string[]> Headers { get; set; }
            public Stream Body { get; set; }
        }
    }
}
