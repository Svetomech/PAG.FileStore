using System;
using System.Diagnostics;
using System.Linq;

namespace PAG.FileStore.Runner
{
  public static class Program
  {
    public const int N = 10_000;

    public static void Main(string[] args)
    {
      var store = new FileStore<ExampleProduct>();

      Console.Write($"Populating store with {N} objects... ");
      Console.WriteLine("{0}ms", BenchmarkMs(() => Populate(store, N)));

      Console.Write($"Saving store with {N} objects to disk... ");
      Console.WriteLine("{0}ms", BenchmarkMs(store.Save));

      Console.Write($"Clearing store with {N} objects from disk... ");
      Console.WriteLine("{0}ms", BenchmarkMs(store.ClearAndSave));

      Console.Write($"Saving store with {N} objects to disk (on each add)... ");
      store.FileAdded += (s, e) => store.Save(); // VERY SLOW
      Console.WriteLine("{0}ms", BenchmarkMs(() => Populate(store, N)));

      Console.Write($"Searching store with {N} objects... ");
      store.Add(new ExampleProduct
      {
        Id = Guid.NewGuid(),
        Name = "WhateverCheaper",
        Price = 11.00M // will be the last (!) and only product with this price
      });
      ExampleProduct foundProduct = null;
      Console.Write("{0}ms - ", BenchmarkMs(()
        => foundProduct = store.Where(p => p.Price == 11.00M).FirstOrDefault()));
      Console.WriteLine(foundProduct);

      var store2 = new FileStore<ExampleProduct>(); // uses the same file on disk

      Console.Write($"Loading store with {N} objects from disk... ");
      Console.WriteLine("{0}ms", BenchmarkMs(store2.Load));

      Console.Write($"Removing last file from store with {N} objects... ");
      store2.FileRemoved += (s, e) => store2.Save();
      Console.WriteLine("{0}ms", BenchmarkMs(() => store2.Remove(foundProduct)));
    }

    private static void Populate(FileStore<ExampleProduct> store, int n)
    {
      for (int i = 0; i < n; ++i)
      {
        var product = new ExampleProduct
        {
          Id = Guid.NewGuid(),
          Name = "Whatever",
          Price = 12.00M
        };

        store.Add(product);
      }
    }

    private static long BenchmarkMs(Action benchAction)
    {
      Stopwatch watch = Stopwatch.StartNew();
      benchAction();
      watch.Stop();

      return watch.ElapsedMilliseconds;
    }
  }
}
