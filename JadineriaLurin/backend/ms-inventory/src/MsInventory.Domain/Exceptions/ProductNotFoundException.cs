namespace MsInventory.Domain.Exceptions;

public class ProductNotFoundException : DomainException
{
    public ProductNotFoundException(Guid id)
        : base($"No existe un producto con id '{id}'.")
    {
    }

    public ProductNotFoundException(string sku)
        : base($"No existe un producto con SKU '{sku}'.")
    {
    }
}
