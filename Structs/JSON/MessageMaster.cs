using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace i_ConVerificationSystem.Structs.JSON
{
    [JsonObject]
    class MessageMaster
    {
        [JsonProperty("messages")]
        public Messages messages { get; set; }

        public class Messages
        {
            [JsonProperty("message")]
            public List<Message> message { get; set; }
        }

        public class Message
        {
            [JsonProperty("-ID")]
            public string ID { get; set; }
            [JsonProperty("-MESSAGE")]
            public string MESSAGE { get; set; }
        }
    }
}
