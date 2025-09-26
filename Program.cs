namespace VendingMachine
{
    class Program
    {
        private const string AdminPassword = "pass";

        private static void Main(string[] args)
        {
            var products = new List<Product>
            {
                new Product(1, "Чипсы", 150, 5),
                new Product(2, "Шоколад", 120, 5),
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
            while (true)
            {
                Console.WriteLine("\nКто пришёл? (customer/admin/exit)");
                Console.Write("Выбор: ");
                var user = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

                if (user == "exit")
                    break;

                switch (user)
                {
                    case "admin":
                        HandleAdminRole(machine);
                        break;
                    case "customer":
                        HandleCustomerRole(machine);
                        break;
                    default:
                        Console.WriteLine("Неверный выбор роли.");
                        break;
                }
            }
        }

        // Admin
        private static void HandleAdminRole(VendingMachine machine)
        {
            Console.Write("Введите пароль администратора: ");
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
                var action = Console.ReadLine() ?? string.Empty;

                if (action == "0")
                    break;

                switch (action)
                {
                    case "1":
                        AdminCollectEarnings(machine);
                        break;
                    case "2":
                        AdminAddProduct(machine);
                        break;
                    case "3":
                        AdminRemoveProduct(machine);
                        break;
                    case "4":
                        AdminShowProducts(machine);
                        break;
                    default:
                        Console.WriteLine("Неверный выбор.");
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

            Console.Write("Цена (В формате: 'rubles.pennies'): ");
            var priceStr = Console.ReadLine() ?? string.Empty;
            if (!Utils.TryParseMoneyToCents(priceStr, out var priceCents))
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

        // ----------------- Customer -----------------
        private static void HandleCustomerRole(VendingMachine machine)
        {
            while (true)
            {
                PrintCustomerMenu(machine);
                Console.Write("Выбор: ");
                var c = Console.ReadLine() ?? string.Empty;

                if (c == "0")
                    break;

                switch (c)
                {
                    case "1":
                        CustomerViewProducts(machine);
                        break;
                    case "2":
                        CustomerInsertCoin(machine);
                        break;
                    case "3":
                        CustomerSelectProduct(machine);
                        break;
                    case "4":
                        CustomerCancelAndReturn(machine);
                        break;
                    default:
                        Console.WriteLine("Неверный выбор.");
                        break;
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

        private static void CustomerViewProducts(VendingMachine machine)
        {
            Utils.PrintProducts(machine.ListProducts());
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
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        private static void CustomerSelectProduct(VendingMachine machine)
        {
            Console.Write("Введите ID товара: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Неверный ID.");
                return;
            }

            var (success, msg, change) = machine.BuyProduct(id);
            Console.WriteLine(msg);
            if (success && change != null)
            {
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
        }

        private static void CustomerCancelAndReturn(VendingMachine machine)
        {
            var ret = machine.CancelAndReturnCoins();
            Console.WriteLine("Возвращаем монеты:");
            Utils.PrintCoins(ret);
        }
    }
}