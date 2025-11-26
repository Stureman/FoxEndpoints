#!/bin/zsh

# FoxEndpoints Benchmark Runner
# Usage: ./run-benchmarks.sh [filter]
# Examples:
#   ./run-benchmarks.sh                    # Run all benchmarks
#   ./run-benchmarks.sh SimpleGetBenchmark # Run only SimpleGetBenchmark

cd "$(dirname "$0")"

echo "Building benchmarks in Release mode..."
dotnet build -c Release

if [ $? -ne 0 ]; then
    echo "Build failed. Aborting."
    exit 1
fi

echo ""
echo "Running benchmarks..."
echo ""

if [ -z "$1" ]; then
    # Run all benchmarks
    dotnet run -c Release --no-build
else
    # Run filtered benchmarks
    dotnet run -c Release --no-build -- --filter "*$1*"
fi

echo ""
echo "Benchmarks complete! Results saved to BenchmarkDotNet.Artifacts/results/"