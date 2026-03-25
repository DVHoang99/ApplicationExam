namespace WebAppExam.Domain.ViewModels
{
    public class OrderDetailViewModel
    {
        public string OrderId { get; set; }
        public CustomerViewModel Customer { get; set; }
        public List<ProductViewModel> Products { get; set; } 

        public int total { get; set; }
    }
}
