using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MQTTnet;
using OT.RobotArmSim.Models;

namespace OT.RobotArmSim
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("test.mosquitto.org", 1883)
                .WithCleanSession()
                .Build();

            const string topicStart = "craft/ot/cmd/start";
            const string topicStatus = "craft/ot/status";
            const string topicAlarm = "craft/ot/alarm";

            var minMs = 2000;
            var maxMs = 5000;
            var failChance = 0.15;

            var mqttClientFactory = new MqttClientFactory();
            using var mqtt = mqttClientFactory.CreateMqttClient();

            mqtt.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    if (e.ApplicationMessage.Topic != topicStart) return;

                    var json = Helpers.Utf8(e.ApplicationMessage.Payload);
                    Console.WriteLine($"[RX] {topicStart}: {json}");

                    var cmd = Helpers.TryParseJson<StartCommand>(json);
                    if (cmd is null)
                    {
                        Console.WriteLine("[ERR] Invalid start payload");
                        return;
                    }
                    // Immediately publish Running
                    await Helpers.PublishAsync(mqtt, topicStatus, new StatusMessage
                    {
                        OrderId = cmd.OrderId,
                        State = "Running",
                        Msg = $"Picking {cmd.Sku} x{cmd.Qty}…"
                    });

                    // Simulate work
                    var delay = Random.Shared.Next(minMs, Math.Max(minMs + 1, maxMs));
                    await Task.Delay(delay);

                    // Randomly fail
                    var failed = Random.Shared.NextDouble() < failChance;
                    if (failed)
                    {
                        await Helpers.PublishAsync(mqtt, topicAlarm, new AlarmMessage
                        {
                            OrderId = cmd.OrderId,
                            Code = "Jam",
                            Severity = "High",
                            Msg = "Feeder jam detected"
                        });

                        await Helpers.PublishAsync(mqtt, topicStatus, new StatusMessage
                        {
                            OrderId = cmd.OrderId,
                            State = "Failed",
                            Msg = "Production failed"
                        });
                    }
                    else
                    {
                        await Helpers.PublishAsync(mqtt, topicStatus, new StatusMessage
                        {
                            OrderId = cmd.OrderId,
                            State = "Completed",
                            Msg = "Production completed"
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERR] Processing message failed: {ex.Message}");
                }
            };

            mqtt.ConnectedAsync += async _ =>
            {
                Console.WriteLine("MQTT Connected");
                await mqtt.SubscribeAsync(topicStart);
                Console.WriteLine($"MQTT Subscribed: {topicStart}");
            };

            mqtt.DisconnectedAsync += async _ =>
            {
                Console.WriteLine("MQTT Disconnected");

                await Task.Delay(1000);
                try { await mqtt.ConnectAsync(options, CancellationToken.None); }
                catch { }
            };

            Console.WriteLine("MQTT Connecting…");
            await mqtt.ConnectAsync(options, CancellationToken.None);
            Console.WriteLine("Press any key to quit…");
            Console.ReadKey();

            try { await mqtt.DisconnectAsync(); } catch { }

        }


    }
}
