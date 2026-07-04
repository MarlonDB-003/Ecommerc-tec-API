using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechWorld.Application.Products.Commands.CreateProduct;
using TechWorld.Application.Products.Commands.DeleteProduct;
using TechWorld.Application.Products.Commands.UpdateProduct;
using TechWorld.Application.Products.Queries.GetFeaturedProducts;
using TechWorld.Application.Products.Queries.GetProductById;
using TechWorld.Application.Products.Queries.GetProducts;

namespace TechWorld.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? category,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        [FromQuery] string sortBy = "createdat",
        [FromQuery] bool ascending = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(
            new GetProductsQuery(category, search, isActive, sortBy, ascending, page, pageSize), ct);
        return Ok(result);
    }

    [HttpGet("featured")]
    public async Task<IActionResult> GetFeatured(
        [FromQuery] string type = "recent",
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetFeaturedProductsQuery(type, limit), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductByIdQuery(id), ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return Created($"api/products/{result.Id}", result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var command = new UpdateProductCommand(
            id, request.Name, request.Price, request.Category,
            request.Description, request.ImageUrl, request.Stock,
            request.DiscountPercentage, request.Specifications);
        var result = await mediator.Send(command, ct);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await mediator.Send(new DeleteProductCommand(id), ct);
        return NoContent();
    }
}

public record UpdateProductRequest(
    string Name,
    decimal Price,
    string Category,
    string? Description,
    string? ImageUrl,
    int Stock,
    int DiscountPercentage,
    IEnumerable<SpecificationInput> Specifications);
