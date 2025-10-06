namespace VendingMachine.Models;

public enum AdminAction
{
    Exit = 0,
    CollectEarnings = 1,
    AddProduct = 2,
    AddCoins = 3,
    RemoveProduct = 4,
    ShowProducts = 5
}

public enum CustomerAction
{
    Exit = 0,
    ViewProducts = 1,
    InsertCoin = 2,
    SelectProduct = 3,
    CancelAndReturn = 4
}

public enum AdminCollectAction
{
    CollectEarnings = 1,
    CollectAllMoney = 2,
    Exit = 0
}