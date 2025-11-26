using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using FoxEndpoints.Benchmarks.Benchmarks;

namespace FoxEndpoints.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        // Run all benchmarks if no args provided
        if (args.Length == 0)
        {
            BenchmarkRunner.Run<SimpleGetBenchmark>(config);
            BenchmarkRunner.Run<RouteParameterBenchmark>(config);
            BenchmarkRunner.Run<JsonPostBenchmark>(config);
            BenchmarkRunner.Run<MixedOperationsBenchmark>(config);
        }
        else
        {
            // Allow running specific benchmarks via command line
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
    }
}