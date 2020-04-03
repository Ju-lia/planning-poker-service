using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketAndNetCore.Web
{
    public class PokerCardService
    {
        private ConcurrentDictionary<string, WebSocket> _users = new ConcurrentDictionary<string, WebSocket>();
        private List<PokerCard> _pokerCards = new List<PokerCard>(PokerCard.GetInitialPokerCards());
        public async Task AddUser(WebSocket socket)
        {
            try
            {
                var name = GenerateName();
                var userAddedSuccessfully = _users.TryAdd(name, socket);
                while (!userAddedSuccessfully)
                {
                    name = GenerateName();
                    userAddedSuccessfully = _users.TryAdd(name, socket);
                }

                GiveUserTheirName(name, socket).Wait();
                AnnounceNewUser(name).Wait();
                SendPokerCards(socket).Wait();
                while (socket.State == WebSocketState.Open)
                {
                    var buffer = new byte[1024 * 4];
                    WebSocketReceiveResult socketResponse;
                    var package = new List<byte>();
                    do
                    {
                        socketResponse = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        package.AddRange(new ArraySegment<byte>(buffer, 0, socketResponse.Count));
                    } while (!socketResponse.EndOfMessage);
                    var bufferAsString = System.Text.Encoding.ASCII.GetString(package.ToArray());
                    if (!string.IsNullOrEmpty(bufferAsString))
                    {
                        var changeRequest = PokerCardChangeRequest.FromJson(bufferAsString);
                        await HandlePokerCardChangeRequest(changeRequest);
                    }
                }
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
            catch (Exception ex)
            { }
        }

        private string GenerateName()
        {
            var prefix = "WebUser";
            Random ran = new Random();
            var name = prefix + ran.Next(1, 1000);
            while (_users.ContainsKey(name))
            {
                name = prefix + ran.Next(1, 1000);
            }
            return name;
        }

        private async Task SendPokerCards(WebSocket socket)
        {
            var message = new SocketMessage<List<PokerCard>>()
            {
                MessageType = "pokerCards",
                Payload = _pokerCards
            };

            await Send(message.ToJson(), socket);
        }

        private async Task SendAll(string message)
        {
            await Send(message, _users.Values.ToArray());
        }

        private async Task Send(string message, params WebSocket[] socketsToSendTo)
        {
            var sockets = socketsToSendTo.Where(s => s.State == WebSocketState.Open);
            foreach (var theSocket in sockets)
            {
                var stringAsBytes = System.Text.Encoding.ASCII.GetBytes(message);
                var byteArraySegment = new ArraySegment<byte>(stringAsBytes, 0, stringAsBytes.Length);
                await theSocket.SendAsync(byteArraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private async Task GiveUserTheirName(string name, WebSocket socket)
        {
            var message = new SocketMessage<string>
            {
                MessageType = "name",
                Payload = name
            };
            await Send(message.ToJson(), socket);
        }

        private async Task AnnounceNewUser(string name)
        {
            var message = new SocketMessage<string>
            {
                MessageType = "announce",
                Payload = $"{name} has joined"
            };
            await SendAll(message.ToJson());
        }

        private async Task AnnouncePokerCardChange(PokerCardChangeRequest request)
        {
            var message = new SocketMessage<string>
            {
                MessageType = "announce",
                Payload = $"{request.Name} has changed poker-card to {request.Value}"
            };
            await SendAll(message.ToJson());
        }

        private async Task HandlePokerCardChangeRequest(PokerCardChangeRequest request)
        {
            var thePokerCard = _pokerCards.FirstOrDefault(sq => sq.Name == request.Name);
            if (thePokerCard == null)
            {
                thePokerCard = new PokerCard()
                {
                    Disabled = request.Disabled,
                    Name = request.Name
                };
                _pokerCards.Add(thePokerCard);
            }
            thePokerCard.Value = request.Value;
            await SendPokerCardsToAll();
            await AnnouncePokerCardChange(request);
        }

        private async Task SendPokerCardsToAll()
        {
            var message = new SocketMessage<List<PokerCard>>()
            {
                MessageType = "pokerCards",
                Payload = _pokerCards
            };

            await SendAll(message.ToJson());
        }
    }
}
