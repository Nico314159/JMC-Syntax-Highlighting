using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JMC.Extension.Server.Datas.Minecraft.Command
{
    internal class CommandNode
    {
        [JsonProperty("type")]
        [JsonRequired]
#pragma warning disable CS8618 // �ޏo���\�������C�s�� Null �I���ʕK�{��ܔ� Null ?�B���l���鍐�׉� Null�B
        public string Type { get; set; }
#pragma warning restore CS8618 // �ޏo���\�������C�s�� Null �I���ʕK�{��ܔ� Null ?�B���l���鍐�׉� Null�B
        [JsonProperty("children")]
        public Dictionary<string, CommandNode>? Childrens { get; set; }
        [JsonProperty("executable")]
        public bool? Executable { get; set; }
        [JsonProperty("parser")]
        public string? Parser { get; set; }

        [JsonProperty("propeties")]
        public List<Propety>? Propeties { get; set; }

        public class Propety
        {
            [JsonProperty("type")]
            public string? Type { get; set; }
            [JsonProperty("registry")]
            public string? Registry { get; set; }
            [JsonProperty("amount")]
            public string? Amount { get; set; }
            [JsonProperty("min")]
            public int? Min { get; set; }
            [JsonProperty("max")]
            public int? Max { get; set; }
        }
    }
}
