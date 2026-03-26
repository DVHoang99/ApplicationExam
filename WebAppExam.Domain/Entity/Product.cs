using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain
{
    public class Product : EntityBase, IAggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        private readonly List<Inventory> _inventories = new();
        public IReadOnlyCollection<Inventory> Inventories => _inventories.AsReadOnly();

        protected Product(string name, string description)
        {
            Id = Ulid.NewUlid();
            Name = name;
            Description = description;

        }

        public void AddOrUpdateInventory(Ulid InventoryId, int price, int stock, Ulid productId)
        {
            var existingItem = _inventories.SingleOrDefault(x => x.Id == InventoryId);

            if (existingItem != null)
            {
                existingItem.UpdatePrice(price);
                existingItem.UpdateStock(stock);
            }
            else
            {
                _inventories.Add(new Inventory(productId, price, stock));
            }
        }
    }
}