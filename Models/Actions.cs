namespace VendingMachine.Models;

public enum AdminAction
{
    Exit = 0,
    CollectEarnings = 1,
    AddProduct = 2,
    RemoveProduct = 3,
    ShowProducts = 4
}

public enum CustomerAction
{
    Exit = 0,
    ViewProducts = 1,
    InsertCoin = 2,
    SelectProduct = 3,
    CancelAndReturn = 4
}