namespace VendingMachine.Models;

public readonly struct PurchaseResult(
    string message,
    bool isSuccess = false,
    Dictionary<int, int>? change = null
)
{
    public bool IsSuccess { get; init; } = isSuccess;
    public string Message { get; init; } = message;
    public Dictionary<int, int>? Change { get; init; } = change;
}