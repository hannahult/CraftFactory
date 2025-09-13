using EasyModbus;
using Integration.DataBridge.Dtos;
using System;
using System.Buffers.Text;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace Integration.DataBridge
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var handler = new HttpClientHandler
            {
  
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            using var http = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7246") };
            var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            EasyModbus.ModbusClient
            modbusClient = new EasyModbus.ModbusClient("127.0.0.1", 502); 
            modbusClient.Connect();

            while (true)
            {
                try
                {
                    // Catching orders with status "InProgress" and setting them back to "New"

                    var inprog = await http.GetFromJsonAsync<OrderSummaryDto[]>(
                    string.Format("/api/v1/orders?status={0}&take=100", "InProgress"),
                    json) ?? Array.Empty<OrderSummaryDto>();

                    foreach (var o in inprog)
                    {
                        await UpdateStatusAsync(http, o.OrderId, "New");
                    }


                    var news = await http.GetFromJsonAsync<OrderSummaryDto[]>(
                        string.Format("/api/v1/orders?status={0}&take=100", "New"), json
                    ) ?? Array.Empty<OrderSummaryDto>();

                    if (news.Length == 0)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    foreach (var order in news)
                    {
                        try
                        {

                            await UpdateStatusAsync(http, order.OrderId, "InProgress");

                            ushort orderCode = BitConverter.ToUInt16(order.OrderId.ToByteArray(), 0);

                            modbusClient.WriteSingleRegister(0, orderCode);

                            Console.WriteLine($"Sent {order.OrderId} (code={orderCode})");
                            modbusClient.WriteSingleCoil(0, true);

                            await UpdateStatusAsync(http, order.OrderId, "Completed");
                        }
                        catch (Exception exOrder)
                        {
                            Console.WriteLine($"Order ERROR: {exOrder.Message}");
           
                            try { await UpdateStatusAsync(http, order.OrderId, "Failed"); } catch { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                }

                Thread.Sleep(2000); 
            }
        }
    
        private static async Task UpdateStatusAsync(HttpClient http, Guid orderId, string status)
        {
            var req = new UpdateOrderStatusDto { Status = status };
            var resp = await http.PutAsJsonAsync(string.Format("/api/v1/orders/{0}/status", orderId), req);
            resp.EnsureSuccessStatusCode();
        }
    }
}
