namespace WebAppExam.Domain.ViewModels
{
    public class GetinventoryViewModelWapper
    {
        public int Total { get; set; }
        public List<GetInventoryViewModel> Data { get; set; } = new List<GetInventoryViewModel>();
    }
}
