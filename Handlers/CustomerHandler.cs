using VendingMachine.Models;

namespace VendingMachine.Handlers;

public class CustomerHandler(VendingMachine machine) : IRoleHandler
{
    public void Handle()
    {
        while (true)
        {
            PrintCustomerMenu(machine);
            if (machine.IsEmptyMachineCoins())
            {
                Console.WriteLine(
                    "ПРЕДУПРЕЖДЕНИЕ: " +
                    "В автомате нет монет. Любые операции требующие сдачи будут отменены. Дождитесь добавления монет."
                );
            }

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
                case CustomerAction.Exit:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        Console.WriteLine($"Доступные номиналы (в копейках): {string.Join(", ", CoinDenominations.Denominations)}");
        var userDenomination = Console.ReadLine() ?? string.Empty;
        if (!int.TryParse(userDenomination, out var coin) ||
            coin <= 0 ||
            !CoinDenominations.Denominations.Contains(coin))
        {
            Console.WriteLine("Неверный ввод.");
            return;
        }

        Console.Write("Введите количество монет (например 1): ");
        var userAmount = Console.ReadLine() ?? string.Empty;
        if (!int.TryParse(userAmount, out var amount) || amount <= 0)
        {
            Console.WriteLine("Неверный ввод.");
            return;
        }

        try
        {
            machine.InsertCoin(coin, amount);
            Console.WriteLine($"Вставлено: {Utils.FormatMoney(coin * amount)}");
            Console.WriteLine($"Сумма в автомате: {Utils.FormatMoney(machine.InsertedAmountCents)}");
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
        var idStr = Console.ReadLine() ?? string.Empty;
        if (!int.TryParse(idStr, out var id) || id <= 0)
        {
            Console.WriteLine("Неверный ID.");
            return;
        }

        var result = machine.BuyProduct(id);
        Console.WriteLine(result.Message);
        if (!result.IsSuccess || result.Change == null) return;
        if (result.Change.Count > 0)
        {
            Console.WriteLine("Сдача:");
            Utils.PrintCoins(result.Change);
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