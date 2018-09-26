using System;

namespace PAG.FileStore.Runner
{
  public interface IProduct
  {
    DateTime CreatedAt { get; set; }
    Guid Id { get; set; }
    string Name { get; set; }
    decimal Price { get; set; }
  }
}