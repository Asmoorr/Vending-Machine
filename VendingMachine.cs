namespace VendingMachine
{
    public class VendingMachine
    {
        private readonly List<Product> _products = [];
        private readonly Dictionary<int, int> _machineCoins = new();
        private readonly Dictionary<int, int> _insertedCoins;
        private int _collectedCents;
        private readonly string _adminPassword;

        public VendingMachine(
            List<Product> initialProducts,
            string adminPassword,
            Dictionary<int, int>? initialMachineCoins = null
        )
        {
            foreach (var product in initialProducts)
            {
                _products.Add(product);
            }

            foreach (var denomination in CoinDenominations.Denominations)
            {
                if (initialMachineCoins != null && initialMachineCoins.TryGetValue(denomination, out var value))
                {
                    _machineCoins[denomination] = value;
                }
                else
                {
                    _machineCoins[denomination] = 5; // if no initial coins are provided, set to 5
                }
            }

            _insertedCoins = new Dictionary<int, int>();
            foreach (var value in CoinDenominations.Denominations)
            {
                _insertedCoins[value] = 0;
            }

            _adminPassword = adminPassword;
            _collectedCents = 0;
        }

        public IEnumerable<Product> ListProducts() => _products.Select(p => p);

        private int GetNextProductId() => _products.Count == 0 ? 1 : _products.Max(product => product.Id) + 1;
        public int InsertedAmountCents => _insertedCoins.Sum(kv => kv.Key * kv.Value);

        public Dictionary<int, int> GetMachineCoins() => new(_machineCoins);

        public int GetCollectedCents() => _collectedCents;


        public void InsertCoin(int coinDenomination, int coinAmount)
        {
            if (!CoinDenominations.Denominations.Contains(coinDenomination))
                throw new ArgumentException("Неподдерживаемый номинал монеты.");
            _insertedCoins[coinDenomination] += coinAmount;
        }

        public Dictionary<int, int> CancelAndReturnCoins()
        {
            var returned = new Dictionary<int, int>(_insertedCoins);
            foreach (var denomination in CoinDenominations.Denominations)
                _insertedCoins[denomination] = 0;
            return returned;
        }

        public (bool success, string message, Dictionary<int, int>?change) BuyProduct(int productId)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);
            var targetProduct = _products.FirstOrDefault(product => product.Id == productId);
            if (targetProduct == null)
                return (false, "Товар не найден.", null);
            if (targetProduct.Quantity <= 0)
                return (false, "Товар отсутствует в наличии.", null);

            var inserted = InsertedAmountCents;
            if (inserted < targetProduct.PriceCents)
                return (
                    false,
                    $"Недостаточно средств. Нужно" +
                    $"{Utils.FormatMoney(targetProduct.PriceCents)}," +
                    $"внесено {Utils.FormatMoney(inserted)}.",
                    null
                );

            var changeNeeded = inserted - targetProduct.PriceCents;

            var pool = new Dictionary<int, int>(_machineCoins);
            foreach (var kv in _insertedCoins)
                pool[kv.Key] = pool.GetValueOrDefault(kv.Key, 0) + kv.Value;

            var change = MakeChange(changeNeeded, pool);
            if (change == null)
            {
                // Не можем выдать точную сдачу — отклоняем покупку
                return (false, "Невозможно выдать точную сдачу. Операция отменена.", null);
            }

            // Успешная покупка:
            // 1) уменьшить количество товара
            var idx = _products.FindIndex(p => p.Id == productId);
            _products[idx] = targetProduct with { Quantity = targetProduct.Quantity - 1 };

            // 2) вставленные монеты добавляются в машину
            foreach (var kv in _insertedCoins)
                _machineCoins[kv.Key] = _machineCoins.GetValueOrDefault(kv.Key, 0) + kv.Value;

            // 3) выдать сдачу: уменьшить соответствующие монеты в машине
            foreach (var kv in change)
                _machineCoins[kv.Key] = _machineCoins.GetValueOrDefault(kv.Key, 0) - kv.Value;

            // 4) увеличить собранные средства на цену товара
            _collectedCents += targetProduct.PriceCents;

            // 5) очистить insertedCoins
            foreach (var k in CoinDenominations.Denominations)
                _insertedCoins[k] = 0;

            return (true, $"Выдан товар: {targetProduct.Name}.", change);
        }

        private static Dictionary<int, int>? MakeChange(int amount, Dictionary<int, int> pool)
        {
            var need = new Dictionary<int, int>();
            var remaining = amount;
            foreach (var coin in CoinDenominations.Denominations.OrderByDescending(x => x))
            {
                if (remaining <= 0) break;
                var take = Math.Min(remaining / coin, pool.GetValueOrDefault(coin, 0));
                if (take <= 0) continue;
                need[coin] = take;
                remaining -= take * coin;
            }

            return remaining != 0 ? null : need;
        }

        public bool CheckAdminPassword(string password) => _adminPassword == password;

        public int CollectEarnings()
        {
            var revenue = _collectedCents;
            _collectedCents = 0;
            return revenue;
        }

        public string AddProduct(string name, int priceCents, int quantity)
        {
            var productId = GetNextProductId();
            var newProduct = new Product(productId, name, priceCents, quantity);
            _products.Add(newProduct);
            return newProduct.Name;
        }


        public bool RemoveProductByName(string name)
        {
            var idx = _products.FindIndex(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (idx < 0) return false;
            _products.RemoveAt(idx);
            return true;
        }
    }
}