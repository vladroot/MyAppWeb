using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MyAppWeb.Endpoints.Dtos;
using MyAppWeb.Services;

namespace MyAppWeb.Endpoints;

/// <summary>POST /addproduct and GET /getproducts.</summary>
public static class ProductEndpoints
{
    /// <summary>
    /// Key of the cached product list. The cache instance name configured in DI
    /// (<see cref="Configuration.DatabaseOptions.RedisInstanceName"/>) is prepended by the
    /// Redis provider, so the entry is stored as <c>MyAppWeb:products:all</c>.
    /// </summary>
    private const string ProductsCacheKey = "products:all";

    private const string LogCategory = "MyAppWeb.Endpoints.ProductEndpoints";

    /// <summary>How long a product list stays cached before it is rebuilt from the database.</summary>
    private static readonly TimeSpan ProductsCacheTtl = TimeSpan.FromMinutes(5);

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/addproduct", async (AppDbContext appDbContext, IDistributedCache cache, IMessageLog messageLog, ILoggerFactory loggerFactory) =>
        {
            var logger = CreateLogger(loggerFactory);

            Product product = new() { Name = "TestProduct", Price = 10, Stock = 20 };
            appDbContext.Products?.Add(product);
            int result = await appDbContext.SaveChangesAsync();

            if (result == 1)
            {
                // The cached list is stale now — drop it so the next read rebuilds it.
                await TryInvalidateProductsAsync(cache, logger);

                var response = new ApiResponse("success", "added new product", DateTime.UtcNow);
                messageLog.Add(JsonSerializer.Serialize(response, JsonDefaults.CamelCase));
                return Results.Created($"/products/{product.Id}", response);
            }
            else
            {
                var response = new ApiErrorResponse("failed", "Could not add new product!", DateTime.UtcNow);
                messageLog.Add(JsonSerializer.Serialize(response, JsonDefaults.CamelCase));
                return Results.Json(response, statusCode: 500);
            }
        })
        .WithName("AddProduct")
        .WithTags("Products")
        .Produces<ApiResponse>(StatusCodes.Status201Created)
        .Produces<ApiErrorResponse>(StatusCodes.Status500InternalServerError);

        app.MapGet("/getproducts", async (AppDbContext appDbContext, IDistributedCache cache, IMessageLog messageLog, ILoggerFactory loggerFactory) =>
        {
            var logger = CreateLogger(loggerFactory);

            // Cache-aside: serve the stored list when Redis has it, otherwise read EF and
            // refresh the entry. A broken cache must never fail the endpoint.
            var cachedProducts = await TryGetProductsAsync(cache, logger);
            if (cachedProducts is not null)
            {
                if (cachedProducts.Count == 0)
                {
                    var response = new ApiResponse("success", "no products yet", DateTime.UtcNow);
                    messageLog.Add(JsonSerializer.Serialize(response, JsonDefaults.CamelCase));
                    return Results.Ok(response);
                }

                messageLog.Add(JsonSerializer.Serialize(cachedProducts, JsonDefaults.CamelCase));
                return Results.Ok(cachedProducts);
            }

            var products = await appDbContext.Products!
                .AsNoTracking()
                .OrderBy(product => product.Name)
                .ToListAsync();

            var payload = products
                .Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.Stock))
                .ToList();

            await TryCacheProductsAsync(cache, payload, logger);

            if (payload.Count == 0)
            {
                var response = new ApiResponse("success", "no products yet", DateTime.UtcNow);
                messageLog.Add(JsonSerializer.Serialize(response, JsonDefaults.CamelCase));
                return Results.Ok(response);
            }

            messageLog.Add(JsonSerializer.Serialize(payload, JsonDefaults.CamelCase));
            return Results.Ok(payload);
        })
        .WithName("GetProducts")
        .WithTags("Products")
        .Produces<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse>(StatusCodes.Status200OK);

        return app;
    }

    private static ILogger CreateLogger(ILoggerFactory loggerFactory)
        => loggerFactory.CreateLogger(LogCategory);

    /// <summary>
    /// Reads the cached product list. Returns <c>null</c> on a miss (or on any cache failure,
    /// which is logged) so the caller falls through to the database.
    /// </summary>
    private static async Task<List<ProductResponse>?> TryGetProductsAsync(IDistributedCache cache, ILogger logger)
    {
        try
        {
            var cached = await cache.GetAsync(ProductsCacheKey);
            if (cached is null)
                return null;

            return JsonSerializer.Deserialize<List<ProductResponse>>(cached, JsonDefaults.CamelCase);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Reading the product cache failed; falling back to the database.");
            return null;
        }
    }

    /// <summary>Stores the product list, or logs a warning when Redis is unreachable.</summary>
    private static async Task TryCacheProductsAsync(
        IDistributedCache cache,
        IReadOnlyList<ProductResponse> products,
        ILogger logger)
    {
        try
        {
            var payload = JsonSerializer.SerializeToUtf8Bytes(products, JsonDefaults.CamelCase);
            await cache.SetAsync(ProductsCacheKey, payload, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ProductsCacheTtl
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Writing the product cache failed; the entry will expire on its own.");
        }
    }

    /// <summary>Drops the cached list after a write, or logs a warning when Redis is unreachable.</summary>
    private static async Task TryInvalidateProductsAsync(IDistributedCache cache, ILogger logger)
    {
        try
        {
            await cache.RemoveAsync(ProductsCacheKey);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalidating the product cache failed; the entry will expire on its own.");
        }
    }
}
