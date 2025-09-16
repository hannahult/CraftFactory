using EasyModbus;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;


namespace OT.RobotArmSim
{
    internal class Program
    {
        private static bool orderReceived = false;
        private static int currentOrderId = 0;

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
                    Console.WriteLine($" New order received! OrderId={currentOrderId}");
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

            modbusServer.CoilsChanged += (int startAddress, int numberOfCoils) =>
            {
                Console.WriteLine($"CoilsChanged event fired at {DateTime.Now}");
                Console.WriteLine($"  Start Address: {startAddress}");
                Console.WriteLine($"  Number of Coils: {numberOfCoils}");

                const int maxCoilAddress = 1999; 

                for (int i = 0; i < numberOfCoils; i++)
                {
                    int address = startAddress + i;
       
                    if (address >= 0 && address <= maxCoilAddress) 
                    {
                        Console.WriteLine($"    Coil[{address}] changed to: {modbusServer.coils[address]}");
                    }
                    else
                    {
                        Console.WriteLine($"    Warning: Attempted to access Coil[{address}] which is out of bounds.");
                    }
                }

                if (modbusServer.coils[1])
                {
                    currentOrderId = modbusServer.holdingRegisters[1];
                    orderReceived = true;
                    Task.Delay(100).ContinueWith(_ => modbusServer.coils[1] = false);
                }
            };

            modbusServer.HoldingRegistersChanged += (int startAddress, int numberOfRegisters) =>
            {
                Console.WriteLine($"HoldingRegistersChanged event fired at {DateTime.Now}");
                Console.WriteLine($"  Start Address: {startAddress}");
                Console.WriteLine($"  Number of Registers: {numberOfRegisters}");
               

                const int maxRegisterAddress = 1999; 

                for (int i = 0; i < numberOfRegisters; i++)
                {
                    int address = startAddress + i;
                    if (address >= 0 && address <= maxRegisterAddress) // Ensure we don't go out of bounds
                    {
                        Console.WriteLine($"    HoldingRegister[{address}] changed to: {modbusServer.holdingRegisters[address]}");
                    }
                    else
                    {
                        Console.WriteLine($"    Warning: Attempted to access HoldingRegister[{address}] which is out of bounds.");
                    }
                }
            };

            modbusServer.holdingRegisters[0] = 123;
            modbusServer.holdingRegisters[1] = 456;

            modbusServer.holdingRegisters[10] = 789; 

            modbusServer.inputRegisters[0] = 999;
            modbusServer.discreteInputs[0] = true;

            try
            {
                Console.WriteLine($"Starting EasyModbus TCP Slave on port {port}...");
                modbusServer.Listen();

                Console.WriteLine("EasyModbus TCP Slave started. Press any key to exit.");
                Console.ReadKey(); 

                Console.WriteLine("Stopping EasyModbus TCP Slave...");
                Console.WriteLine("EasyModbus TCP Slave stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
