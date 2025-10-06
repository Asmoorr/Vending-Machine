using VendingMachine.Models;

namespace VendingMachine;

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

    public bool IsEmptyMachineCoins() => _machineCoins.All(kv => kv.Value == 0);

    public bool AddCoins(int denomination, int quantity)
    {
        if (!CoinDenominations.Denominations.Contains(denomination))
            throw new ArgumentException("Неподдерживаемый номинал монеты.");
        _machineCoins[denomination] = _machineCoins.GetValueOrDefault(denomination, 0) + quantity;
        return true;
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

    public PurchaseResult BuyProduct(int productId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(productId);
        var targetProduct = _products.FirstOrDefault(product => product.Id == productId);
        if (targetProduct == null)
            return new PurchaseResult
            {
                IsSuccess = false,
                Message = "Товар не найден.",
                Change = null
            };
        if (targetProduct.Quantity <= 0)
            return new PurchaseResult
            {
                IsSuccess = false,
                Message = "Товар отсутствует в наличии.",
                Change = null
            };

        var inserted = InsertedAmountCents;
        if (inserted < targetProduct.PriceCents)
            return new PurchaseResult
            {
                IsSuccess = false,
                Message = $"Недостаточно средств. Нужно " +
                          $"{Utils.FormatMoney(targetProduct.PriceCents)}, а " +
                          $"внесено {Utils.FormatMoney(inserted)}",
                Change = null
            };

        var changeNeeded = inserted - targetProduct.PriceCents;

        var pool = new Dictionary<int, int>(_machineCoins);
        foreach (var kv in _insertedCoins)
            pool[kv.Key] = pool.GetValueOrDefault(kv.Key, 0) + kv.Value;

        var change = MakeChange(changeNeeded, pool);
        if (change == null) // Can't make change because not enough coins in the machine
        {
            return new PurchaseResult
            {
                IsSuccess = false,
                Message = "Невозможно выдать точную сдачу. Операция отменена.",
                Change = null
            };
        }

        // Successful purchase
        var idx = _products.FindIndex(product => product.Id == productId);
        _products[idx] = targetProduct with { Quantity = targetProduct.Quantity - 1 };

        // Insert coins into the machine
        foreach (var kv in _insertedCoins)
            _machineCoins[kv.Key] = _machineCoins.GetValueOrDefault(kv.Key, 0) + kv.Value;

        // Return change
        foreach (var kv in change)
            _machineCoins[kv.Key] = _machineCoins.GetValueOrDefault(kv.Key, 0) - kv.Value;

        // Add product price to collected cents
        _collectedCents += targetProduct.PriceCents;

        // Clear inserted coins
        foreach (var k in CoinDenominations.Denominations)
            _insertedCoins[k] = 0;

        return new PurchaseResult
        {
            IsSuccess = true,
            Message = $"Выдан товар: {targetProduct.Name}.",
            Change = change
        };
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

    public int CollectInnerMoney()
    {
        var machineMoney = _machineCoins.Sum(kv => kv.Key * kv.Value);
        foreach (var kv in _machineCoins)
            _machineCoins[kv.Key] = 0;
        return machineMoney;
    }

    public void AddProduct(string name, int priceCents, int quantity)
    {
        var productId = GetNextProductId();
        var newProduct = new Product(productId, name, priceCents, quantity);
        _products.Add(newProduct);
    }


    public bool RemoveProductByName(string name)
    {
        var idx = _products.FindIndex(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return false;
        _products.RemoveAt(idx);
        return true;
    }
}