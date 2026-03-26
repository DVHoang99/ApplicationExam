using WebAppExam.Domain.Entity;

namespace WebAppExam.Domain
{
    public class Product : EntityBase, IAggregateRoot
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
         public int Price { get; set; }
        private readonly List<Inventory> _inventories = new();
        public IReadOnlyCollection<Inventory> Inventories => _inventories.AsReadOnly();

        protected Product() { }
        public Product(string name, string description, int price)
        {
            Id = Ulid.NewUlid();
            Name = name;
            Description = description;
            Price = price;
            CreatedAt = DateTime.UtcNow;
        }

        public void AddOrUpdateInventory(Ulid InventoryId, int price, int stock, Ulid productId)
        {
            var existingItem = _inventories.SingleOrDefault(x => x.Id == InventoryId);

            if (existingItem != null)
            {
                existingItem.UpdateStock(stock);
            }
            else
            {
                _inventories.Add(new Inventory(productId, price, stock));
            }
        }

        public void DeleteInventory(Ulid inventoryId)
        {
            var inventory = _inventories.SingleOrDefault(x => x.Id == inventoryId);

            if (inventory != null)
            {
                inventory.DeleteInventory();
            }
        }   
    }
}