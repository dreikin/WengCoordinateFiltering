﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateFiltering
{
    class Program
    {
        static void Main(string[] args)
        {
            Random randomSource = new Random();
            IList<Customer> customers = GenerateCustomers(40000, randomSource);
            IList<Provider> providers = GenerateProviders(922000, randomSource);

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            foreach(var customer in customers)
            {
                IList<Provider> nearestNProviders = FindNearestNProviders(customer, providers, 200);
            }
            timer.Stop();
            Console.WriteLine($"Elapsed time: {timer.Elapsed}");

            return;

        }

        private static IList<Provider> FindNearestNProviders(Customer customer, IList<Provider> providers, int count)
        {
            IList<Provider> nearbyProviders = new List<Provider>(count * 2);

            while (nearbyProviders.Count < count && nearbyProviders.Count < providers.Count)
            {
                throw new NotImplementedException("Create two lists of geohash boxes (old and new).  Add to list in concentric rings.  Subtract old list from new list to get new boxes to evaluate.");
            }
        }

        private static IList<Customer> GenerateCustomers(int count, Random randomSource)
        {
            IList<Customer> customers = new List<Customer>(count);
            for (int i = 0; i < count; i++)
            {
                customers.Add(new Customer(i, new Location(randomSource.NextDouble() * 180 - 90, randomSource.NextDouble() * 360 - 180)));
            }

            return customers;
        }

        private static IList<Provider> GenerateProviders(int count, Random randomSource, int geohashPrecision = 9)
        {
            IList<Provider> providers = new List<Provider>();
            for (int i = 0; i < count; i++)
            {
                providers.Add(new Provider(i, new Location(randomSource.NextDouble() * 180 - 90, randomSource.NextDouble() * 360 - 180)));
            }

            return providers;
        }
    }
}
