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
            using var http = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:8246") };
            var json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


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

                        try
                        {
                            await CreateIncidentAsync(http, o.OrderId, "REQUEUE_STALE", "Warning", "Order was InProgress too long and was re-queued to New.");
                        }
                        catch {  }
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
                            EasyModbus.ModbusClient
                            modbusClient = new EasyModbus.ModbusClient("127.0.0.1", 502);
                            try
                            {
                                modbusClient.Connect();
                            }
                            catch (Exception ex)
                            {
                                await UpdateStatusAsync(http, order.OrderId, "Failed");
                                await CreateIncidentAsync(http, order.OrderId, "OT_UNREACHABLE", "Error", $"Could not connect to OT: {ex.Message}");
                                continue;
                            }


                            await UpdateStatusAsync(http, order.OrderId, "InProgress");

                            ushort orderCode = BitConverter.ToUInt16(order.OrderId.ToByteArray(), 0);

                            modbusClient.WriteSingleRegister(0, orderCode);

                            Console.WriteLine($"Sent {order.OrderId} (code={orderCode})");
                            modbusClient.WriteSingleCoil(0, true);

                            await UpdateStatusAsync(http, order.OrderId, "Completed");
                            Console.WriteLine($"Completed {order.OrderId} (code={orderCode})");
                        }
                        catch (Exception exOrder)
                        {
                            Console.WriteLine($"Order ERROR: {exOrder.Message}");
           
                            try { await UpdateStatusAsync(http, order.OrderId, "Failed"); } catch { }

                            try
                            {
                                await CreateIncidentAsync(http, order.OrderId, "MODBUS_SEND_FAIL", "Error", exOrder.Message);
                            }
                            catch {  }

                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: {ex.Message}");
                    await CreateIncidentAsync(http, null, "API_FAIL", "Error", ex.Message);              
                }

                Thread.Sleep(2000); 
            }
        }
    
        private static async Task UpdateStatusAsync(HttpClient http, Guid orderId, string status)
        {
            try
            {
                var req = new UpdateOrderStatusDto { Status = status };
                var resp = await http.PutAsJsonAsync(string.Format("/api/v1/orders/{0}/status", orderId), req);
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                try
                {
                    CreateIncidentAsync(http, orderId, "API_UPDATE_STATUS_FAIL", "Error", ex.Message).Wait();
                }
                catch { }
                throw;
            }
        }

        static async Task CreateIncidentAsync(HttpClient http, Guid? orderId, string code, string severity, string message)
        {
            var payload = new { OrderId = orderId, Code = code, Severity = severity, Message = message };
            try
            {
                var resp = await http.PostAsJsonAsync("/api/v1/incidents", payload);
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                
            }

        }

    }
}
