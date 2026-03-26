using System;
using System.Windows.Input;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Products.Commands;

public class UpdateProductCommand(Ulid id) : ICommand<Ulid>
{
    public Ulid ProductId { get; set; } = id;
    public string Name { get; set; }
    public string Description { get; set; }
    public int Price { get; set; }
    public List<InventoryDTO> Inventories { get; set; }
}
