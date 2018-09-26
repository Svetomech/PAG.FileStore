using System;
using System.Collections.Generic;

namespace PAG.FileStore.Runner
{
  public class ExampleProduct : IProduct, IEquatable<ExampleProduct>
  {
    public ExampleProduct()
    {
      CreatedAt = DateTime.Now;
    }

    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }

    #region Equality (VS-generated)
    public override bool Equals(object obj)
        => Equals(obj as ExampleProduct);

    public bool Equals(ExampleProduct other)
        => other != null && Id.Equals(other.Id);

    public override int GetHashCode()
        => 2108858624 + EqualityComparer<Guid>.Default.GetHashCode(Id);

    public override string ToString()
        => Name;

    public static bool operator ==(ExampleProduct product1, ExampleProduct product2)
        => EqualityComparer<ExampleProduct>.Default.Equals(product1, product2);

    public static bool operator !=(ExampleProduct product1, ExampleProduct product2)
        => !(product1 == product2);
    #endregion
  }
}
