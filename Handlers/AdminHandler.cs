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
                case AdminAction.AddCoins:
                    AdminAddCoins(machine);
                    break;
                case AdminAction.Exit:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static void PrintAdminMenu()
    {
        Console.WriteLine("\n--- ADMIN MENU ---");
        Console.WriteLine("1. Забрать деньги");
        Console.WriteLine("2. Добавить товар");
        Console.WriteLine("3. Добавить монеты");
        Console.WriteLine("4. Удалить товар по названию");
        Console.WriteLine("5. Показать товары и количество монет в автомате");
        Console.WriteLine("0. Выйти в главное меню");
    }

    private static void AdminCollectEarnings(VendingMachine machine)
    {
        Console.WriteLine("1. Забрать заработанные деньги");
        Console.WriteLine("2. Забрать все деньги");
        Console.WriteLine("0. Выйти в главное меню");
        Console.Write("Выбор: ");
        var input = Console.ReadLine() ?? string.Empty;
        Enum.TryParse<AdminCollectAction>(input.Trim(), ignoreCase: true, out var action);
        switch (action)
        {
            case AdminCollectAction.CollectEarnings:
                var collectedMoney = machine.CollectEarnings();
                Console.WriteLine($"Забрали {Utils.FormatMoney(collectedMoney)}");
                break;
            case AdminCollectAction.CollectAllMoney:
                collectedMoney = machine.CollectInnerMoney() + machine.CollectEarnings();
                Console.WriteLine($"Забрали {Utils.FormatMoney(collectedMoney)}");
                break;
            case AdminCollectAction.Exit:
                return;
            default:
                Console.WriteLine("Неверный выбор.");
                break;
        }
    }

    private static void AdminAddProduct(VendingMachine machine)
    {
        Console.Write("Название товара: ");
        var name = Console.ReadLine() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            Console.WriteLine("Название товара не может быть пустым.");
            return;
        }

        name = name.Trim();

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
        Console.WriteLine($"Выручка: {Utils.FormatMoney(machine.GetCollectedCents())}");
        Console.WriteLine("Монеты в автомате:");
        Utils.PrintCoins(machine.GetMachineCoins());
    }

    private static void AdminAddCoins(VendingMachine machine)
    {
        var denominations = CoinDenominations.Denominations;
        Console.Write($"Номинал монеты (один из: {string.Join(", ", denominations)}): ");
        var denomination = Convert.ToInt32(Console.ReadLine() ?? string.Empty);
        if (denomination <= 0 || !denominations.Contains(denomination))
        {
            Console.WriteLine("Неверный номинал монеты.");
            return;
        }

        Console.Write("Количество: ");
        if (!int.TryParse(Console.ReadLine(), out var quantity) || quantity <= 0)
        {
            Console.WriteLine("Неверное количество.");
            return;
        }

        var isAdded = machine.AddCoins(denomination, quantity);
        if (!isAdded)
        {
            Console.WriteLine($"Невозможно добавить {quantity} монет с номиналом {denomination}.");
            return;
        }

        Console.WriteLine($"Добавлено {quantity} монет с номиналом {denomination}");
    }
}