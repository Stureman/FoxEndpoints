# Benchmark Results Analysis Guide

## How to Run Benchmarks

### Quick Start
```bash
cd FoxEndpoints.Benchmarks
./run-benchmarks.sh
```

### Run Individual Benchmarks
```bash
# Simple GET requests
./run-benchmarks.sh SimpleGetBenchmark

# Route parameter binding
./run-benchmarks.sh RouteParameterBenchmark

# JSON POST requests
./run-benchmarks.sh JsonPostBenchmark

# Mixed operations (realistic workflow)
./run-benchmarks.sh MixedOperationsBenchmark
```

### Run from dotnet CLI
```bash
dotnet run -c Release
dotnet run -c Release -- --filter *SimpleGetBenchmark*
dotnet run -c Release -- --list flat
```

## Understanding the Results

### Key Metrics

#### Mean
The average time per operation. Lower is better.
```
Method                           Mean      Error     StdDev
FoxEndpoints_GetProducts       45.23 μs   0.891 μs   0.834 μs
MVC_GetProducts                52.17 μs   1.042 μs   0.975 μs
```

#### Memory Allocation
Total bytes allocated per operation. Lower is better.
```
Method                           Allocated
FoxEndpoints_GetProducts         5.12 KB
MVC_GetProducts                  6.84 KB
```

#### Rank
Performance ranking where 1 = fastest.
```
Method                           Rank
FoxEndpoints_GetProducts         1
MVC_GetProducts                  2
```

### Interpreting Results

**FoxEndpoints should show:**
- Lower mean execution time (faster)
- Lower memory allocations (more efficient)
- Rank 1 (baseline)

**MVC Controllers typically show:**
- Higher mean execution time (more middleware/abstractions)
- Higher memory allocations (more complex request pipeline)
- Rank 2

### What Each Benchmark Tests

#### SimpleGetBenchmark
- **What**: Basic routing and JSON response
- **Measures**: Overhead of framework routing layer
- **Expected**: FoxEndpoints ~10-20% faster due to minimal middleware

#### RouteParameterBenchmark
- **What**: Route parameter extraction (`/api/products/{id}`)
- **Measures**: Parameter binding efficiency
- **Expected**: Similar performance, slight edge to FoxEndpoints

#### JsonPostBenchmark
- **What**: JSON request deserialization
- **Measures**: Model binding and request processing
- **Expected**: Similar performance (both use System.Text.Json)

#### MixedOperationsBenchmark
- **What**: GET all → GET by ID → POST create (3 operations)
- **Measures**: Realistic API workflow performance
- **Expected**: FoxEndpoints 15-25% faster cumulative

## Result Files

After running benchmarks, results are saved to:
```
FoxEndpoints.Benchmarks/BenchmarkDotNet.Artifacts/results/
```

### File Types
- `*-report.html` - Interactive HTML report
- `*-report.csv` - CSV data for Excel/analysis
- `*-report-github.md` - Markdown table for GitHub
- `*.log` - Detailed execution logs

## Troubleshooting

### Benchmarks Take Too Long
By default, BenchmarkDotNet runs multiple warmup and measurement iterations.
For quick tests, use:
```bash
dotnet run -c Release -- --filter *SimpleGetBenchmark* --job short
```

### Inconsistent Results
Ensure:
- Close other applications
- Disable CPU throttling/power saving
- Run in Release mode
- Don't run in debugger

### Build Errors
```bash
cd FoxEndpoints.Benchmarks
dotnet clean
dotnet restore
dotnet build -c Release
```

## Example Output

```
BenchmarkDotNet v0.14.0, macOS Sonoma 14.5
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.100
  [Host]     : .NET 10.0.0

| Method                          | Mean     | Error    | StdDev   | Rank | Allocated |
|-------------------------------- |---------:|---------:|---------:|-----:|----------:|
| FoxEndpoints_GetProducts        | 42.15 μs | 0.823 μs | 0.770 μs |    1 |   5.12 KB |
| MVC_GetProducts                 | 51.28 μs | 1.015 μs | 0.950 μs |    2 |   6.89 KB |
```

**Analysis:** FoxEndpoints is ~18% faster with ~26% less memory allocation.

## CI/CD Integration

Add benchmarks to your CI pipeline:

```yaml
# GitHub Actions example
- name: Run Benchmarks
  run: |
    cd FoxEndpoints.Benchmarks
    dotnet run -c Release --filter *SimpleGetBenchmark* --job short
    
- name: Upload Results
  uses: actions/upload-artifact@v4
  with:
    name: benchmark-results
    path: FoxEndpoints.Benchmarks/BenchmarkDotNet.Artifacts/results/
```

## Further Reading

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [ASP.NET Core Performance Best Practices](https://learn.microsoft.com/en-us/aspnet/core/performance/performance-best-practices)
- [Measuring .NET Performance](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/measure-performance)