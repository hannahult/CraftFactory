using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Protocol;
using System.Buffers;

namespace OT.RobotArmSim
{
    public class Helpers
    {
        public static JsonSerializerOptions JsonOpts { get; } = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public static async Task PublishAsync(IMqttClient client, string topic, object payload)
        {
            var json = JsonSerializer.Serialize(payload, JsonOpts);
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(json)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();
            await client.PublishAsync(msg);
            Console.WriteLine($"[TX] {topic}: {json}");
        }
        public static T? TryParseJson<T>(string json)
        {
            try { return JsonSerializer.Deserialize<T>(json, JsonOpts); }
            catch { return default; }
        }

        public static string Utf8(ReadOnlySequence<byte> payload)
        {
            return Encoding.UTF8.GetString(payload.FirstSpan);
        }
    }
}
