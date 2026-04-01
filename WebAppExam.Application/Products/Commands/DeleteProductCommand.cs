using System;
using System.Windows.Input;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Products.Commands;

public class DeleteProductCommand(Ulid id, string wareHouseId) : ICommand<Ulid>
{
    public Ulid ProductId { get; set; } = id;
    public string WareHouseId { get; set; } = wareHouseId;
}
