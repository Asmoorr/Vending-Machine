using VendingMachine.Handlers;
using VendingMachine.Models;

namespace VendingMachine;

class Program
{
    private const string AdminPassword = "pass";

    private static void Main()
    {
        var products = new List<Product>
        {
            new Product(1, "Чипсы", 150, 2),
            new Product(2, "Шоколад", 120, 1),
            new Product(3, "Вода", 90, 5)
        };

        var machine = new VendingMachine(
            products,
            AdminPassword,
            initialMachineCoins: new Dictionary<int, int>
            {
                [100] = 5, [50] = 5, [10] = 10, [5] = 10, [2] = 10, [1] = 50
            }
        );

        RunMainLoop(machine);

        Console.WriteLine("Выход. До свидания!");
    }


    private static void RunMainLoop(VendingMachine machine)
    {
        Console.Clear();
        while (true)
        {
            Console.WriteLine("\nКто пришёл?");
            Console.WriteLine("1. Customer");
            Console.WriteLine("2. Admin");
            Console.WriteLine("3. Exit");
            Console.Write("Выбор: ");
            var input = (Console.ReadLine() ?? string.Empty).Trim();

            var isParsed = Enum.TryParse<UserRoles>(input, ignoreCase: true, out var userRole);

            if (!isParsed || !Enum.IsDefined(typeof(UserRoles), userRole))
            {
                Console.Clear();
                Console.WriteLine("Неверный выбор роли.");
                continue;
            }

            if (userRole == UserRoles.Exit)
                break;

            Console.Clear();

            var handler = HandlerFactory.Create(userRole, machine);
            handler.Handle();
        }
    }
}