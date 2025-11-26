using System.Net.Http.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using FoxEndpoints.Benchmarks.Infrastructure;
using FoxEndpoints.Benchmarks.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace FoxEndpoints.Benchmarks.Benchmarks;

/// <summary>
/// Comprehensive benchmark testing multiple operations in sequence
/// to simulate realistic API usage patterns
/// </summary>
[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class MixedOperationsBenchmark
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

    /// <summary>
    /// Simulates a realistic workflow:
    /// 1. GET all products
    /// 2. GET specific product by ID
    /// 3. POST create new product
    /// </summary>
    [Benchmark(Baseline = true)]
    public async Task FoxEndpoints_MixedOperations()
    {
        // GET all products
        var allProducts = await _foxClient.GetFromJsonAsync<ProductsResponse>("/api/products");
        
        // GET specific product
        var singleProduct = await _foxClient.GetFromJsonAsync<ProductDto>("/api/products/1");
        
        // POST create product
        var createResponse = await _foxClient.PostAsJsonAsync("/api/products", _testRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateProductResponse>();
    }

    /// <summary>
    /// Same workflow using MVC controllers
    /// </summary>
    [Benchmark]
    public async Task MVC_MixedOperations()
    {
        // GET all products
        var allProducts = await _mvcClient.GetFromJsonAsync<ProductsResponse>("/api/mvc/products");
        
        // GET specific product
        var singleProduct = await _mvcClient.GetFromJsonAsync<ProductDto>("/api/mvc/products/1");
        
        // POST create product
        var createResponse = await _mvcClient.PostAsJsonAsync("/api/mvc/products", _testRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<CreateProductResponse>();
    }
}