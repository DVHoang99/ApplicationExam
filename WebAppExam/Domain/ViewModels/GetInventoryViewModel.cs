using WebAppExam.Domain;

namespace WebAppExam.Domain.ViewModels
{
    public class GetInventoryViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public ProductStatus ProductStatus { get; set; }
        public static GetInventoryViewModel FromResult(Product product) => new GetInventoryViewModel
        {
            Id = product.Id.ToString(),
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Quantity = product.Quantity,
            ProductStatus = product.Price > 0 ? ProductStatus.Available : ProductStatus.OutOfStock,
        };
    }

    public enum ProductStatus
    { 
        OutOfStock = 0,
        Available = 1
    }
   
}
