using IT.CraftOrders.Data;
using IT.CraftOrders.Models;
using IT.CraftOrders.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.CraftOrders.UI
{
    public class Menu
    {
        private readonly ProductService _productService;
        private readonly OrderService _orderService;

        public Menu(ProductService productService, OrderService orderService)
        {
            _productService = productService;
            _orderService = orderService;  
        }

        public async Task RunAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Craft Orders Management");
                Console.WriteLine("1. View Orders");
                Console.WriteLine("2. View Products");
                Console.WriteLine("3. Add Order");
                Console.WriteLine("0. Exit");
                Console.Write("Select an option: ");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        await ViewOrdersAsync();
                        break;
                    case "2":
                        await ViewProductsAsync();
                        break;
                    case "3":
                        //await AddOrderAsync();
                        return;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private async Task ViewOrdersAsync()
        {
            Console.Clear();
            var orders = await _orderService.GetLatestAsync();
            foreach (var o in orders)
            {
                Console.WriteLine($"Order {o.OrderId} | {o.Customer?.Name} | {o.Status}");
                foreach (var l in o.OrderLines)
                    Console.WriteLine($"  - {l.Product?.Name} x{l.Quantity} ({l.UnitPrice:C})");
            }
            Pause();
        }

        private async Task ViewProductsAsync()
        {
            Console.Clear();
            var products = await _productService.GetAllProductsAsync();
            foreach (var p in products)
                Console.WriteLine($"{p.ProductId}. {p.Name} ({p.Sku}) - {p.Price:C}");
            Pause();
        }

        private void Pause()
        {
            Console.WriteLine("\nPress any key...");
            Console.ReadKey();
        }
    }
}
