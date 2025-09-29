namespace VendingMachine
{
    public record Product(int Id, string Name, int PriceCents, int Quantity);

    public static class CoinDenominations
    {
        public static readonly int[] Denominations = [100, 50, 10, 5, 2, 1];
    }
}