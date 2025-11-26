# FoxEndpoints Benchmarks

Performance benchmarks comparing FoxEndpoints to traditional ASP.NET Core MVC controllers.

## Overview

This project uses BenchmarkDotNet to measure and compare the performance characteristics of:
- **FoxEndpoints** - Lightweight endpoint framework
- **MVC Controllers** - Traditional ASP.NET Core controllers

## Benchmark Scenarios

### 1. SimpleGetBenchmark
Tests basic GET request routing and response serialization.
- Endpoint: `/api/products`
- Returns a list of 5 products
- Measures routing overhead and JSON serialization

### 2. RouteParameterBenchmark
Tests route parameter binding and extraction.
- Endpoint: `/api/products/{id}`
- Tests parameter binding from route values
- Returns a single product

### 3. JsonPostBenchmark
Tests JSON request deserialization and POST handling.
- Endpoint: `/api/products`
- Posts a CreateProductRequest with JSON body
- Measures model binding and request processing

## Running the Benchmarks

### Run All Benchmarks
```bash
cd FoxEndpoints.Benchmarks
dotnet run -c Release
```

### Run Specific Benchmark
```bash
dotnet run -c Release -- --filter *SimpleGetBenchmark*
dotnet run -c Release -- --filter *RouteParameterBenchmark*
dotnet run -c Release -- --filter *JsonPostBenchmark*
```

### List Available Benchmarks
```bash
dotnet run -c Release -- --list flat
```

## Results

Results are saved to `BenchmarkDotNet.Artifacts/results/` after each run.

Key metrics measured:
- **Mean execution time** - Average time per operation
- **Memory allocation** - Bytes allocated per operation
- **Rank** - Performance ranking (1 = fastest)

## Configuration

- **Target Framework**: .NET 10.0
- **Mode**: Release (optimizations enabled)
- **Server GC**: Enabled for realistic server workload simulation
- **Memory Diagnostics**: Enabled to track allocations

## Project Structure

```
FoxEndpoints.Benchmarks/
├── Benchmarks/
│   ├── SimpleGetBenchmark.cs       # GET request benchmark
│   ├── RouteParameterBenchmark.cs  # Route parameter benchmark
│   └── JsonPostBenchmark.cs        # POST/JSON benchmark
├── Controllers/
│   └── ProductsMvcController.cs    # MVC controller implementations
├── Endpoints/
│   ├── GetProductsFoxEndpoint.cs   # FoxEndpoints implementations
│   ├── GetProductByIdFoxEndpoint.cs
│   └── CreateProductFoxEndpoint.cs
├── Models/
│   └── ProductDto.cs               # Shared DTOs
└── Program.cs                      # Benchmark runner entry point
```

## Notes

- Benchmarks use in-memory `TestServer` to eliminate network overhead
- Each benchmark creates isolated host instances to prevent cross-contamination
- Baseline is always FoxEndpoints for comparison
- Results may vary based on hardware and system load