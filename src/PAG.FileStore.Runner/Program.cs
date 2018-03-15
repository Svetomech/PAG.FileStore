using System;
using System.Diagnostics;
using System.Linq;

namespace PAG.FileStore.Runner
{
    public static class Program
    {
        public const int N = 1000;

        public static void Main(string[] args)
        {
            var store = new FileStore<ExampleProduct>();

            Console.Write($"Populating store with {N} files... ");
            Console.WriteLine("{0}ms", Benchmark(() => Populate(store, N)));

            Console.Write($"Saving store with {N} files to disk... ");
            Console.WriteLine("{0}ms", Benchmark(store.Save));

            Console.Write($"Clearing store with {N} files from disk... ");
            Console.WriteLine("{0}ms", Benchmark(store.ClearAndSave));

            Console.Write($"Saving store with {N} files to disk (on each add)... ");
            store.FileAdded += (s, e) => store.Save(); // VERY SLOW
            Console.WriteLine("{0}ms", Benchmark(() => Populate(store, N)));

            Console.Write($"Searching store with {N} files... ");
            store.Add(new ExampleProduct { Id = Guid.NewGuid(), Name = "WhateverCheaper", Price = 11.00M }); ExampleProduct foundProduct = null;
            Console.Write("{0}ms - ", Benchmark(() => foundProduct = store.Where(p => p.Price == 11.00M).FirstOrDefault())); Console.WriteLine(foundProduct);
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

        private static long Benchmark(Action benchFunc)
        {
            var watch = Stopwatch.StartNew();
            benchFunc();
            watch.Stop();

            return watch.ElapsedMilliseconds;
        }
    }
}
