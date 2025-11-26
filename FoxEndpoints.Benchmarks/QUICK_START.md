# Quick Reference: FoxEndpoints Benchmarks

## Run All Benchmarks
```bash
cd FoxEndpoints.Benchmarks
./run-benchmarks.sh
```

## Run Individual Benchmarks
```bash
./run-benchmarks.sh SimpleGetBenchmark        # Simple GET
./run-benchmarks.sh RouteParameterBenchmark   # Route params
./run-benchmarks.sh JsonPostBenchmark         # JSON POST
./run-benchmarks.sh MixedOperationsBenchmark  # Mixed workflow
```

## Alternative (without script)
```bash
dotnet run -c Release
dotnet run -c Release -- --filter *SimpleGetBenchmark*
```

## Results Location
```
BenchmarkDotNet.Artifacts/results/
├── *-report.html      # Interactive report
├── *-report.csv       # Spreadsheet data
├── *-report-github.md # Markdown table
└── *.log              # Detailed logs
```

## What's Measured
- ⏱️  **Mean execution time** (lower = better)
- 💾 **Memory allocation** (lower = better)
- 🏆 **Rank** (1 = fastest)

## Project Structure
```
FoxEndpoints.Benchmarks/
├── Benchmarks/         # Benchmark test classes
├── Controllers/        # MVC implementations
├── Endpoints/          # FoxEndpoints implementations
├── Models/             # Shared DTOs
├── README.md           # Full documentation
├── RESULTS_GUIDE.md    # Analysis guide
└── run-benchmarks.sh   # Convenience script
```

## More Info
- Full README: `FoxEndpoints.Benchmarks/README.md`
- Analysis guide: `FoxEndpoints.Benchmarks/RESULTS_GUIDE.md`
- BenchmarkDotNet docs: https://benchmarkdotnet.org/