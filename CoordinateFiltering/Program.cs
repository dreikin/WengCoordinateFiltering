﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoordinateFiltering
{
    class Program
    {
        static void Main(string[] args)
        {
            var workingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "TEMP\\CoordinateFiltering");


            //RunRandomDataTests(workingDirectory);

            if (args.Length == 1)
            {
                RunProviderCsvDataTests(args[0], workingDirectory);
            }

            return;
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

        private static IList<Provider> GetProvidersFromCsv(string inputFile)
        {
            /*
             * Provider CSV format expected is:
             *   ID|Latitude|Longitude|Category|DedupeKey
             * with types:
             *   int|double|double|string|hex-string
             *   
             * DedupeKey is optional, everything else is required.
             */
            IList<Provider> providers = new List<Provider>();
            foreach (string line in File.ReadLines(inputFile))
            {
                var fields = line.Split('|');
                var id = Int32.Parse(fields[0]);
                var location = new Location(Double.Parse(fields[1]), Double.Parse(fields[2]));
                var category = fields[3];
                var dedupeKey = fields[4];
                providers.Add(new Provider(id, location, category, dedupeKey));
            }

            return providers;
        }

        private static void RunRandomDataTests(string workingDirectory)
        {
            // Set up output paths.
            var outputPaths = new Dictionary<string, string>();
            outputPaths.Add("naivePath", Path.Combine(workingDirectory, "RandomData\\Naive"));
            outputPaths.Add("naiveParallelPath", Path.Combine(workingDirectory, "RandomData\\NaiveParallel"));
            outputPaths.Add("kdTreePath", Path.Combine(workingDirectory, "RandomData\\KdTree"));
            outputPaths.Add("kdTreeParallelPath", Path.Combine(workingDirectory, "RandomData\\KdTreeParallel"));

            // Generate data for tests.
            Random randomSource = new Random();
            IList<Customer> customers = GenerateCustomers(40000, randomSource);
            IList<Provider> providers = GenerateProviders(922000, randomSource);

            // Run tests.
            RunDataTests(customers, providers, outputPaths, "Randomly generated");
            Console.ReadLine();
        }

        private static void RunProviderCsvDataTests(string inputFile, string workingDirectory)
        {
            var outputPaths = new Dictionary<string, string>();
            outputPaths.Add("naivePath", Path.Combine(workingDirectory, "CsvData\\Naive"));
            outputPaths.Add("naiveParallelPath", Path.Combine(workingDirectory, "CsvData\\NaiveParallel"));
            outputPaths.Add("kdTreePath", Path.Combine(workingDirectory, "CsvData\\KdTree"));
            outputPaths.Add("kdTreeParallelPath", Path.Combine(workingDirectory, "CsvData\\KdTreeParallel"));

            // Generate data for tests.
            Random randomSource = new Random();
            IList<Customer> customers = GenerateCustomers(40000, randomSource);
            var providers = GetProvidersFromCsv(Path.Combine(workingDirectory, inputFile));

            // Run tests.
            RunDataTests(customers, providers, outputPaths, "CSV of providers, randomly generated customers");
            Console.ReadLine();
        }

        private static void RunDataTests(IList<Customer> customers, IList<Provider> providers, IDictionary<string, string> outputPaths, string dataType)
        {
            // Comment blocks in accordance with what you want to test.

            Accord.Collections.KDTree<Provider> providersTree = FindNearestProviders.CartesianProviderKdTreeFromList(providers);
            Tests.TestData result;

            Console.WriteLine($"Data type: {dataType}");

            // K Nearest Neighbors tests
            Console.WriteLine("* K Nearest Neighbors");
            for (int count = 200; count <= 800; count *= 2)
            {

                /*
                 * Naive algorithm tests.
                 */
                //result = Tests.FindNearestNProvidersNaive(customers, providers, count);
                //Console.WriteLine($"Naive method (K={count}): {result.Elapsed}");
                //PrintNearestProviders(result.NearestProviders, count, outputPaths["naivePath"]);

                //result = Tests.FindNearestNProvidersNaiveParallel(customers, providers, count);
                //Console.WriteLine($"Naive method, Parallel.ForEach (K={count}): {result.Elapsed}");
                //PrintNearestProviders(result.NearestProviders, count, outputPaths["naiveParallelPath"]);

                /*
                 * k-d tree tests.
                 */
                result = Tests.FindNearestNProvidersKdTree(customers, providersTree, count);
                Console.WriteLine($"Kd-Tree Method (K={count}): {result.Elapsed}");
                PrintNearestProviders(result.NearestProviders, count, outputPaths["kdTreePath"]);

                result = Tests.FindNearestNProvidersKdTreeParallel(customers, providersTree, count);
                Console.WriteLine($"Kd-Tree, Parallel.ForEach Method (K={count}): {result.Elapsed}");
                PrintNearestProviders(result.NearestProviders, count, outputPaths["kdTreeParallelPath"]);
            }

            // Neighbors Within Radius R tests
            Console.WriteLine("* Neighbors Within Radius R");
            for (double distance = 1; distance < 129; distance *= 2)
            {
                double radius = getBubbleRadius(distance);

                /*
                 * k-d tree tests.
                 */
                result = Tests.FindProvidersWithinRadiusKdTree(customers, providersTree, radius);
                Console.WriteLine($"Kd-Tree Method (R={radius}): {result.Elapsed}");
                PrintNearestProviders(result.NearestProviders, distance, outputPaths["kdTreePath"]);

                result = Tests.FindProvidersWithinRadiusKdTreeParallel(customers, providersTree, radius);
                Console.WriteLine($"Kd-Tree, Parallel.ForEach Method (R={radius}): {result.Elapsed}");
                PrintNearestProviders(result.NearestProviders, distance, outputPaths["kdTreeParallelPath"]);
            }
        }

        private static void PrintNearestProviders(SortedDictionary<Customer, IList<KeyValuePair<double, Provider>>> nearestProviders, int count, string outputPath)
        {
            outputPath = outputPath + $"-{count}.txt";
            var output = new StreamWriter(File.Open(outputPath, FileMode.Create, FileAccess.Write));
            output.WriteLine("customer|provider|distance");
            foreach (var customer in nearestProviders)
            {
                foreach (var provider in customer.Value)
                {
                    output.WriteLine($"{customer.Key.Id}|{provider.Value.Id}|{provider.Key}");
                }
            }
            output.Close();
        }

        private static void PrintNearestProviders(SortedDictionary<Customer, IList<KeyValuePair<double, Provider>>> nearestProviders, double radius, string outputPath)
        {
            outputPath = outputPath + $"-{radius}km.txt";
            var output = new StreamWriter(File.Open(outputPath, FileMode.Create, FileAccess.Write));
            output.WriteLine("customer|provider|distance");
            foreach (var customer in nearestProviders)
            {
                foreach (var provider in customer.Value)
                {
                    output.WriteLine($"{customer.Key.Id}|{provider.Value.Id}|{provider.Key}");
                }
            }
            output.Close();
        }

        private static double getBubbleRadius(double kilometers)
        {
            // This method assumes we're normalizing the sphere to a unit circle.
            double earthsRadiusKm = 6371; // Approximately, according to Google.
            double arcInRadians = kilometers / earthsRadiusKm;

            return 2 * Math.Sin(arcInRadians / 2);
        }
    }
}