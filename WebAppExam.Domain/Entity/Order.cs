using System.Net.Http.Headers;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Entity;
using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain
{
    public class Order : EntityBase, IAggregateRoot
    {
        public Ulid CustomerId { get; set; }
        public int TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string Address { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Reason { get; set; } = string.Empty;

        private readonly List<OrderDetail> _details = new();
        public IReadOnlyCollection<OrderDetail> Details => _details.AsReadOnly();

        protected Order() { }

        public Order(Ulid customerId, string address, string customerName, string phoneNumber)
        {
            Id = Ulid.NewUlid();
            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.Draft;
            Address = address;
            CustomerName = customerName;
            PhoneNumber = phoneNumber;
        }

        public OrderDetail AddOrUpdateItem(Ulid productId, int unitPrice, int quantity, Ulid wereHouseId)
        {
            OrderDetail affectedItem;
            var existingItem = _details.SingleOrDefault(x => x.ProductId == productId && x.WareHouseId == wereHouseId);

            if (existingItem != null)
            {
                var oldQuantity = existingItem.Quantity;
                existingItem.UpdateQuantity(quantity);
                affectedItem = new OrderDetail(productId, unitPrice, existingItem.Quantity - oldQuantity, wereHouseId);
            }
            else
            {
                _details.Add(new OrderDetail(productId, unitPrice, quantity, wereHouseId));
                affectedItem = new OrderDetail(productId, unitPrice, quantity, wereHouseId);
            }
            RecalculateTotal();

            return affectedItem;
        }

        public OrderDetail? RemoveItem(Ulid productId, string wareHouseId)
        {
            var parsedWareHouseId = Ulid.Parse(wareHouseId);

            var item = _details.SingleOrDefault(x => x.ProductId == productId && x.WareHouseId == parsedWareHouseId);

            if (item == null)
            {
                return null;
            }
            var affectedItem = new OrderDetail(productId, item.Price, -item.Quantity, parsedWareHouseId);
            _details.Remove(item);
            RecalculateTotal();
            return affectedItem;
        }
        private void RecalculateTotal()
        {
            TotalAmount = _details.Sum(x => x.Price * x.Quantity);
        }
        public void UpdateOrderGeneralInformation(Ulid customerId, string address, string customerName, string phoneNumber)
        {
            Address = address;
            CustomerName = customerName;
            PhoneNumber = phoneNumber;
            CustomerId = customerId;
        }

        public void DeleteOrder()
        {
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public void UpdateOrderStatus(OrderStatus status, string reason)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
            Reason = reason;
        }
    }
}
