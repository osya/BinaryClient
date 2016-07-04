using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinaryClient.JSONTypes;
using Newtonsoft.Json;

namespace BinaryClient
{
    public class BinaryWs
    {
        private readonly ClientWebSocket _ws = new ClientWebSocket();
        private readonly Uri _uri = new Uri("wss://ws.binaryws.com/websockets/v3");

        public async Task<Auth> Authorize(string key)
        {
            SendRequest($"{{\"authorize\":\"{key}\"}}").Wait();
            var jsonAuth = await StartListen();
            return JsonConvert.DeserializeObject<Auth>(jsonAuth);
        }

        public async Task SendRequest(string data)
        {
            while (_ws.State == WebSocketState.Connecting) { };
            if (_ws.State != WebSocketState.Open)
            {
                throw new Exception("Connection is not open.");
            }

            var reqAsBytes = Encoding.UTF8.GetBytes(data);
            var ticksRequest = new ArraySegment<byte>(reqAsBytes);

            await this._ws.SendAsync(ticksRequest,
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        public async Task<string> StartListen()
        {
            while (_ws.State == WebSocketState.Open)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer.Array), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                    var str = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
                    return str;
                }
            }
            return "Connection Closed!";
        }

        public async Task Connect()
        {
            await _ws.ConnectAsync(_uri, CancellationToken.None);
        }
    }
}