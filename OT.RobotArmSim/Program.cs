using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using EasyModbus;


namespace OT.RobotArmSim
{
    internal class Program
    {
        private static bool orderReceived = false;
        private static int currentOrderId = 0;
        private static int currentQuantity = 0;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Simulated Robot Arm with Modbus support");

            Thread modbusThread = new Thread(StartRobotArmServer);
            modbusThread.IsBackground = true;
            modbusThread.Start();

            while (true)
            {
                if (orderReceived)
                {
                    Console.WriteLine($" New order received! OrderId={currentOrderId}, Quantity={currentQuantity}");
                    Console.WriteLine("...Executing order...");
                    Thread.Sleep(3000); // Simulating Work
                    Console.WriteLine("Order completed");
                    orderReceived = false;
                }

                Thread.Sleep(500);
            }
        }

        public static void StartRobotArmServer()
        {
            int port = 502; 
            ModbusServer modbusServer = new ModbusServer();
            modbusServer.Port = port;

            modbusServer.CoilsChanged += (startAddress, numberOfCoils) =>
            {
                if (startAddress == 0 && modbusServer.coils[0]) // Coil 0 = Start
                {
                    currentOrderId = modbusServer.holdingRegisters[0];
                    currentQuantity = modbusServer.holdingRegisters[1];
                    orderReceived = true;

                    // Auto-reset coil
                    modbusServer.coils[0] = false;
                }
            };

            try
            {
                Console.WriteLine($"Starting Robot Arm Server on port {port}...");
                modbusServer.Listen();
                Console.WriteLine("Server is running. Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

    }
}
