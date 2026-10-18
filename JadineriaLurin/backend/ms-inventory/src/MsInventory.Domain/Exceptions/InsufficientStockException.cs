namespace MsInventory.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public string Sku { get; }
    public string ProductName { get; }
    public decimal Requested { get; }
    public decimal Available { get; }

    public InsufficientStockException(string sku, string productName, decimal requested, decimal available)
        : base($"Stock insuficiente para '{productName}' ({sku}): se solicitaron {requested} pero hay {available} disponibles.")
    {
        Sku = sku;
        ProductName = productName;
        Requested = requested;
        Available = available;
    }
}
