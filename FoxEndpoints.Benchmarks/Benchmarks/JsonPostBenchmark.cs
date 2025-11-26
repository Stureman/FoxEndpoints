using System.Net.Http.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FoxEndpoints.Benchmarks.Infrastructure;
using FoxEndpoints.Benchmarks.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace FoxEndpoints.Benchmarks.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class JsonPostBenchmark
{
    private HttpClient _foxClient = null!;
    private HttpClient _mvcClient = null!;
    private IHost _foxHost = null!;
    private IHost _mvcHost = null!;
    private CreateProductRequest _testRequest = null!;

    [GlobalSetup]
    public async Task Setup()
    {
        _testRequest = new CreateProductRequest
        {
            Name = "Test Product",
            Price = 99.99m,
            Description = "A benchmark test product",
            Category = "Electronics"
        };

        // Setup FoxEndpoints server
        _foxHost = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapFoxEndpoints();
                        });
                    });
            })
            .StartAsync();

        _foxClient = _foxHost.GetTestClient();

        // Setup MVC server
        _mvcHost = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddControllers();
                        services.AddRouting();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapControllers();
                        });
                    });
            })
            .StartAsync();

        _mvcClient = _mvcHost.GetTestClient();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        _foxClient?.Dispose();
        _mvcClient?.Dispose();
        if (_foxHost != null) await _foxHost.StopAsync();
        if (_mvcHost != null) await _mvcHost.StopAsync();
        _foxHost?.Dispose();
        _mvcHost?.Dispose();
    }

    [Benchmark(Baseline = true)]
    public async Task<CreateProductResponse?> FoxEndpoints_CreateProduct()
    {
        var response = await _foxClient.PostAsJsonAsync("/api/products", _testRequest);
        return await response.Content.ReadFromJsonAsync<CreateProductResponse>();
    }

    [Benchmark]
    public async Task<CreateProductResponse?> MVC_CreateProduct()
    {
        var response = await _mvcClient.PostAsJsonAsync("/api/mvc/products", _testRequest);
        return await response.Content.ReadFromJsonAsync<CreateProductResponse>();
    }
}