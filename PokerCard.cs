using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocketAndNetCore.Web
{
    public class PokerCard
    {
        public int Value { get; set; }
        public bool Disabled { get; set; }
        public string Name { get; set; }

        public static IEnumerable<PokerCard> GetInitialPokerCards()
        {
            var pokerCards = new List<PokerCard>();
            

            return pokerCards;
        }
    }

    public class PokerCardChangeRequest
    {
        public int Value { get; set; }
        public bool Disabled { get; set; }
        public string Name { get; set; }

        public static PokerCardChangeRequest FromJson(string json)
        {
            return JsonConvert.DeserializeObject<PokerCardChangeRequest>(json);
        }
    }
}
