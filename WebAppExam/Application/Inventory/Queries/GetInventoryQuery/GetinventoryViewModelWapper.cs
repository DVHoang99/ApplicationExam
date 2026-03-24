namespace WebAppExam.Application.Inventory.Queries.GetInventoryQuery
{
    public class GetinventoryViewModelWapper
    {
        public int Total { get; set; }
        public List<GetInventoryViewModel> Data { get; set; } = new List<GetInventoryViewModel>();
    }
}
