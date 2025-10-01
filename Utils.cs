using System.Globalization;
using VendingMachine.Models;

namespace VendingMachine;

public static class Utils
{
    public static void ViewProducts(VendingMachine machine)
    {
        PrintProducts(machine.ListProducts());
    }

    public static void PrintProducts(IEnumerable<Product> products)
    {
        Console.WriteLine("\nID | Название        | Цена     | Кол-во");
        Console.WriteLine("----------------------------------------");
        foreach (var product in products)
        {
            var quantity = product.Quantity > 0 ? product.Quantity.ToString() : "Нет в наличии";
            Console.WriteLine(
                $"{product.Id,-3}| {product.Name,-15} | {FormatMoney(product.PriceCents),8} | {quantity}"
            );
        }
    }

    public static void PrintCoins(Dictionary<int, int> coins)
    {
        var total = coins.Sum(kv => kv.Key * kv.Value);
        Console.WriteLine($"Сумма: {FormatMoney(total)}");
        foreach (var kv in coins.OrderByDescending(k => k.Key))
            if (kv.Value > 0)
                Console.WriteLine($"{kv.Key} коп. x {kv.Value}");
    }

    public static bool TryParseMoneyToCents(string input, out int cents)
    {
        cents = 0;
        if (string.IsNullOrWhiteSpace(input)) return false;
        input = input.Trim().Replace(',', '.');
        if (!decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal val))
            return false;
        cents = (int)Math.Round(val * 100);
        return true;
    }

    public static string FormatMoney(int cents)
    {
        var rubles = cents / 100;
        var pennies = cents % 100;
        return $"{rubles}.{pennies:D2} руб.";
    }
}