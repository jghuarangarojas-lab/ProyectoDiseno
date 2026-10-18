using Auth.Domain.Common;

namespace Auth.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public ICollection<User> Users { get; private set; } = new List<User>();

    private Role()
    {
    }

    public Role(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || !Names.All.Contains(name))
        {
            throw new Domain.Exceptions.DomainException($"El rol '{name}' no es válido.");
        }

        Name = name;
    }

    public static class Names
    {
        public const string Admin = "ADMIN";
        public const string Botanist = "BOTANIST";
        public const string Gardener = "GARDENER";
        public const string Cashier = "CASHIER";
        public const string Warehouse = "WAREHOUSE";
        public const string Delivery = "DELIVERY";

        public static readonly IReadOnlyCollection<string> All =
            new[] { Admin, Botanist, Gardener, Cashier, Warehouse, Delivery };
    }
}
