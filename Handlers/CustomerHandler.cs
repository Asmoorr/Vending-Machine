using VendingMachine.Models;

namespace VendingMachine.Handlers;

public class CustomerHandler(VendingMachine machine) : IRoleHandler
{
    public void Handle()
    {
        PrintCustomerMenu(machine);
        Console.Write("Выбор: ");
        var input = Console.ReadLine() ?? string.Empty;

        if (!Enum.TryParse<CustomerAction>(input.Trim(), ignoreCase: true, out var action) ||
            !Enum.IsDefined(typeof(CustomerAction), action))
        {
            Console.Clear();
            Console.WriteLine("Неверный выбор.");
            return;
        }

        Console.Clear();

        if (action == CustomerAction.Exit) return;

        switch (action)
        {
            case CustomerAction.ViewProducts:
                Utils.ViewProducts(machine);
                break;
            case CustomerAction.InsertCoin:
                CustomerInsertCoin(machine);
                break;
            case CustomerAction.SelectProduct:
                CustomerSelectProduct(machine);
                break;
            case CustomerAction.CancelAndReturn:
                CustomerCancelAndReturn(machine);
                break;
        }
    }

    private static void PrintCustomerMenu(VendingMachine machine)
    {
        Console.WriteLine("\n--- CUSTOMER MENU ---");
        Console.WriteLine("1. Просмотреть товары");
        Console.WriteLine("2. Вставить монету");
        Console.WriteLine("3. Выбрать товар по ID");
        Console.WriteLine("4. Отмена и возврат монет");
        Console.WriteLine("0. Выйти в главное меню");
        Console.WriteLine($"Внесено: {Utils.FormatMoney(machine.InsertedAmountCents)}");
    }

    private static void CustomerInsertCoin(VendingMachine machine)
    {
        Console.WriteLine("Доступные номиналы (в копейках): " + string.Join(", ", CoinDenominations.Denominations));
        Console.Write("Введите номинал (например 100): ");
        var userDenomination = Console.ReadLine() ?? string.Empty;
        if (!int.TryParse(userDenomination, out var coin))
        {
            Console.WriteLine("Неверный ввод.");
            return;
        }

        Console.Write("Введите количество монет (например 1): ");
        var userAmount = Console.ReadLine() ?? string.Empty;
        if (!int.TryParse(userAmount, out var amount))
        {
            Console.WriteLine("Неверный ввод.");
            return;
        }

        try
        {
            machine.InsertCoin(coin, amount);
            Console.WriteLine($"Вставлено: {Utils.FormatMoney(machine.InsertedAmountCents)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static void CustomerSelectProduct(VendingMachine machine)
    {
        Utils.ViewProducts(machine);
        Console.Write("Введите ID товара: ");
        if (!int.TryParse(Console.ReadLine(), out var id))
        {
            Console.WriteLine("Неверный ID.");
            return;
        }

        var (success, msg, change) = machine.BuyProduct(id);
        Console.WriteLine(msg);
        if (!success || change == null) return;
        if (change.Count > 0)
        {
            Console.WriteLine("Сдача:");
            Utils.PrintCoins(change);
        }
        else
        {
            Console.WriteLine("Сдача: 0");
        }
    }

    private static void CustomerCancelAndReturn(VendingMachine machine)
    {
        var ret = machine.CancelAndReturnCoins();
        Console.WriteLine("Возвращаем монеты:");
        Utils.PrintCoins(ret);
    }
}