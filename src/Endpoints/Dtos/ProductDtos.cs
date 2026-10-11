namespace MyAppWeb.Endpoints.Dtos;

/// <summary>
/// Wire format of a product. Previously the endpoints serialized the EF entity directly,
/// which coupled the HTTP contract to the database model.
/// </summary>
public sealed record ProductResponse(long Id, string Name, decimal Price, int Stock);
