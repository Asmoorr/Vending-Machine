using VendingMachine.Models;

namespace VendingMachine.Handlers;

public class AdminHandler(VendingMachine machine) : IRoleHandler
{
    public void Handle()
    {
        Console.Write("Введите пароль администратора (pass по умолчанию): ");
        var pass = Console.ReadLine() ?? string.Empty;
        if (!machine.CheckAdminPassword(pass))
        {
            Console.WriteLine("Неверный пароль.");
            return;
        }

        while (true)
        {
            PrintAdminMenu();
            Console.Write("Выбор: ");
            var input = Console.ReadLine() ?? string.Empty;

            if (!Enum.TryParse<AdminAction>(input.Trim(), ignoreCase: true, out var action) ||
                !Enum.IsDefined(typeof(AdminAction), action))
            {
                Console.Clear();
                Console.WriteLine("Неверный выбор.");
                continue;
            }

            Console.Clear();

            if (action == AdminAction.Exit) break;

            switch (action)
            {
                case AdminAction.CollectEarnings:
                    AdminCollectEarnings(machine);
                    break;
                case AdminAction.AddProduct:
                    AdminAddProduct(machine);
                    break;
                case AdminAction.RemoveProduct:
                    AdminRemoveProduct(machine);
                    break;
                case AdminAction.ShowProducts:
                    AdminShowProducts(machine);
                    break;
            }
        }
    }

    private static void PrintAdminMenu()
    {
        Console.WriteLine("\n--- ADMIN MENU ---");
        Console.WriteLine("1. Забрать деньги");
        Console.WriteLine("2. Добавить товар");
        Console.WriteLine("3. Удалить товар по названию");
        Console.WriteLine("4. Показать товары");
        Console.WriteLine("0. Выйти в главное меню");
    }

    private static void AdminCollectEarnings(VendingMachine machine)
    {
        var cents = machine.CollectEarnings();
        Console.WriteLine($"Забрали {Utils.FormatMoney(cents)}");
    }

    private static void AdminAddProduct(VendingMachine machine)
    {
        Console.Write("Название товара: ");
        var name = Console.ReadLine() ?? string.Empty;

        Console.Write("Цена (В формате: 'rub.penny'): ");
        var priceStr = Console.ReadLine() ?? string.Empty;
        if (!Utils.TryParseMoneyToCents(priceStr, out var priceCents) || priceCents <= 0)
        {
            Console.WriteLine("Неверная цена.");
            return;
        }

        Console.Write("Количество: ");
        if (!int.TryParse(Console.ReadLine(), out var quantity) || quantity <= 0)
        {
            Console.WriteLine("Неверное количество.");
            return;
        }

        machine.AddProduct(name, priceCents, quantity);
        Console.WriteLine($"Добавлен товар {name}");
    }

    private static void AdminRemoveProduct(VendingMachine machine)
    {
        Utils.ViewProducts(machine);
        Console.Write("Название товара для удаления: ");
        var productName = Console.ReadLine() ?? string.Empty;
        Console.WriteLine(machine.RemoveProductByName(productName)
            ? "Товар удалён."
            : "Товар с таким названием не найден.");
    }

    private static void AdminShowProducts(VendingMachine machine)
    {
        Utils.PrintProducts(machine.ListProducts());
        Console.WriteLine($"Собрано: {Utils.FormatMoney(machine.GetCollectedCents())}");
        Console.WriteLine("Монеты в автомате:");
        Utils.PrintCoins(machine.GetMachineCoins());
    }
}