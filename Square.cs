using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocketAndNetCore.Web
{
    public class PokerCard
    {
        public int Id { get; set; }
        public string Color { get; set; }

        public static IEnumerable<PokerCard> GetInitialPokerCards()
        {
            var colors = new string[] { "red", "green", "blue" };
            var pokerCards = new List<PokerCard>();
            for (int i = 0; i < 10; i++)
            {
                var random = new Random();
                pokerCards.Add(new PokerCard()
                {
                    Id = i,
                    Color = colors[(random.Next(1, 3)) - 1]
                });
            }
            return pokerCards;
        }
    }

    public class PokerCardChangeRequest
    {
        public int Id { get; set; }
        public string Color { get; set; }
        public string Name { get; set; }

        public static PokerCardChangeRequest FromJson(string json)
        {
            return JsonConvert.DeserializeObject<PokerCardChangeRequest>(json);
        }
    }
}
