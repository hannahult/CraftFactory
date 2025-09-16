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
        private Employee? _currentUser = null;
        private readonly AuthService _authService;
        private readonly CustomerService _customerService;

        public Menu(ProductService productService, OrderService orderService, AuthService authService, CustomerService customerService)
        {
            _productService = productService;
            _orderService = orderService;
            _authService = authService;
            _customerService = customerService;
        }

        public async Task RunAsync()
        {
            await LoginAsync();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("Craft Orders Management");
                Console.WriteLine("1. View Orders");
                Console.WriteLine("2. View Products");
                Console.WriteLine("3. Add Order");
                Console.WriteLine("0. Exit");
                Console.WriteLine("9. Logout");
                Console.Write("Select an option: ");
                var choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":

                        if (_currentUser == null)
                        {
                            Console.WriteLine("Please login first.");
                            Pause();
                            break;
                        }

                        await ViewOrdersAsync();
                        break;

                    case "2":
                        await ViewProductsAsync();
                        break;
                    case "3":

                        if (_currentUser == null)
                        {
                            Console.WriteLine("Please login first.");
                            Pause();
                            break;
                        }

                        await AddOrderAsync();
                        break;
                    case "9":
                        _currentUser = null;
                        await LoginAsync();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private async Task LoginAsync()
        {
            while (_currentUser == null)
            {
                Console.Clear();
                Console.WriteLine("=== Employee Login ===");
                Console.WriteLine("Login as Employee = E or Continue as guest = G");
                var userType = (Console.ReadLine() ?? "").Trim().ToUpper();

                if (userType == "G")
                {
                    Console.WriteLine("Continuing as guest. Some features may be restricted.");
                    Pause();
                    return;
                }

                Console.Write("Email: ");
                var email = (Console.ReadLine() ?? "").Trim();
                Console.Write("Password: ");
                var password = (Console.ReadLine() ?? "").Trim();

                _currentUser = await _authService.LoginAsync(email, password);

                if (_currentUser == null)
                {
                    Console.WriteLine("Invalid credentials. Press any key to try again...");
                    Console.ReadKey();
                }
            }
            Console.WriteLine($"Welcome, {_currentUser.Email}!");
            Pause();
        }

        private async Task ViewOrdersAsync()
        {
            Console.Clear();
            var orders = await _orderService.GetLatestAsync();
            //Console.Clear();
            Console.WriteLine($"(debug) fetched {orders.Count} orders from DB");
            foreach (var o in orders)
            {
                Console.WriteLine($"Order {o.OrderId} | {o.Customer?.Name} | {o.Status}");
                foreach (var l in o.OrderLines)
                    Console.WriteLine($"  - {l.Product?.Name} x{l.Quantity} ({l.UnitPrice:C})");
            }
            Pause();
        }

        private async Task AddOrderAsync()

        {
            var products = await _productService.GetAllProductsAsync();
            if (products.Count == 0)
            {
                Console.WriteLine("No products added. Add products first.");
                Pause();
                return;
            }

            var lines = new List<(int productId, int qty)>();

            while (true)
            {
                Console.Clear();
                Console.WriteLine("=====Create Order=====");
                Console.WriteLine("Available products:");
                foreach (var p in products)
                    Console.WriteLine($"{p.ProductId}. {p.Name} ({p.Sku}) - {p.Price:C}");

                Console.Write("\nChoice (ID / 0 = Done / R = Regret / C = Cancel): ");
                var pick = (Console.ReadLine() ?? "").Trim();

                if (string.Equals(pick, "C", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Cancelled. No order created.");
                    Pause();
                    return;
                }

                if (string.Equals(pick, "R", StringComparison.OrdinalIgnoreCase))
                {
                    if (lines.Count > 0)
                    {
                        lines.RemoveAt(lines.Count - 1);
                        Console.WriteLine("Last line Removed.");
                    }
                    else
                    {
                        Console.WriteLine("There's nothing to regret.");
                    }
                    PrintCartSummary(lines, products);
                    continue;
                }

                if (!int.TryParse(pick, out int productId))
                {
                    Console.WriteLine("Invalid choice, try igain.");
                    continue;
                }

                if (productId == 0) // klar
                {
                    if (lines.Count == 0)
                    {
                        Console.WriteLine("No line added. Cancelling.");
                        Pause();
                        return;
                    }

                    Customer? customer = null;
                    int customerId = 0;

                    while (customer == null)
                    {
                        Console.WriteLine("Enter CustomerId:");
                        var inputId = Console.ReadLine();

                        if (!int.TryParse(inputId, out customerId) || customerId <= 0)
                        {
                            Console.WriteLine("Invalid input. Try again...");
                            continue;
                        }

                        customer = await _customerService.GetCustomerByIdAsync(customerId);
                        if (customer == null)
                        {
                            Console.WriteLine("Customer not found. Try again...");
                            continue;
                        }
                    }

                    // Order comfirmation
                    Console.Clear();
                    Console.WriteLine("\n=== Order summary ===");
                    var total = PrintCartSummary(lines, products);
                    Console.Write($"For customer {customer.Name}");

                    Console.Write($"Save order (total {total:C})? (Y/N): ");
                    var confirm = (Console.ReadLine() ?? "").Trim();
                    if (!confirm.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Cancelled. No order created.");
                        Pause();
                        return;
                    }

                    var order = await _orderService.CreateAsync(customerId, lines: lines);
                    Console.Clear();
                    Console.WriteLine($"\nOrder created: {order.OrderId}");
                    Pause();
                    return;
                }

                var prod = products.FirstOrDefault(p => p.ProductId == productId);
                if (prod == null)
                {
                    Console.WriteLine("Invalid product-ID.");
                    continue;
                }

                Console.Write($"Quantity for {prod.Name}: ");
                if (!int.TryParse(Console.ReadLine(), out int qty) || qty <= 0)
                {
                    Console.WriteLine("Invalid quantity.");
                    continue;
                }

                lines.Add((productId, qty));
                Console.WriteLine($"Added: {prod.Name} x{qty}");
                PrintCartSummary(lines, products);
            }
        }

        private decimal PrintCartSummary(List<(int productId, int qty)> lines, List<Product> products)
        {
            if (lines.Count == 0)
            {
                Console.WriteLine("\n(Shopping Cart empty)\n");
                return 0m;
            }

            Console.WriteLine("\nShopping Cart:");
            decimal total = 0m;
            foreach (var (pid, q) in lines)
            {
                var p = products.First(x => x.ProductId == pid);
                var row = p.Price * q;
                total += row;
                Console.WriteLine($" - {p.Name} x{q} á {p.Price:C} = {row:C}");
            }
            Console.WriteLine($"Sum: {total:C}\n");
            return total;
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
