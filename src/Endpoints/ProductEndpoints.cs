using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyAppWeb.Endpoints.Dtos;
using MyAppWeb.Services;

namespace MyAppWeb.Endpoints;

/// <summary>POST /addproduct and GET /getproducts.</summary>
public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/addproduct", async (AppDbContext appDbContext, IMessageLog messageLog) =>
        {
            Product product = new() { Name = "TestProduct", Price = 10, Stock = 20 };
            appDbContext.Products?.Add(product);
            int result = await appDbContext.SaveChangesAsync();

            if (result == 1)
            {
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

        app.MapGet("/getproducts", async (AppDbContext appDbContext, IMessageLog messageLog) =>
        {
            var products = await appDbContext.Products!
                .AsNoTracking()
                .OrderBy(product => product.Name)
                .ToListAsync();

            if (products.Count == 0)
            {
                var response = new ApiResponse("success", "no products yet", DateTime.UtcNow);
                messageLog.Add(JsonSerializer.Serialize(response, JsonDefaults.CamelCase));
                return Results.Ok(response);
            }

            var payload = products
                .Select(p => new ProductResponse(p.Id, p.Name, p.Price, p.Stock))
                .ToList();

            messageLog.Add(JsonSerializer.Serialize(payload, JsonDefaults.CamelCase));
            return Results.Ok(payload);
        })
        .WithName("GetProducts")
        .WithTags("Products")
        .Produces<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResponse>(StatusCodes.Status200OK);

        return app;
    }
}
