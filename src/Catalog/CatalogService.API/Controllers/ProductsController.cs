using CatalogService.Application.DTOs;
using CatalogService.Application.DTOs.Search;
using CatalogService.Application.DTOs.Stock;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController: ControllerBase
{
     private readonly IProductRepository _productRepo;
    private readonly IStockRepository _stockRepo;
    private readonly IProductSearchService _searchService;

    public ProductsController(
        IProductRepository productRepo,
        IStockRepository stockRepo,
        IProductSearchService searchService)
    {
        _productRepo = productRepo;
        _stockRepo = stockRepo;
        _searchService = searchService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken ct)
    {
        var product = await _productRepo.GetByIdAsync(id, ct);
        if (product is null) return NotFound();

        var stock = await _stockRepo.GetByProductIdAsync(id, ct);

        return Ok(MapToResponse(product, stock));
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var products = await _productRepo.GetAllAsync(page, pageSize, ct);
        var totalCount = await _productRepo.GetTotalCountAsync(ct: ct);

        var items = new List<ProductResponse>();
        foreach (var product in products)
        {
            var stock = await _stockRepo.GetByProductIdAsync(product.Id, ct);
            items.Add(MapToResponse(product, stock));
        }

        return Ok(new PagedResponse<ProductResponse>(items, totalCount, page, pageSize));
    }

    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetByCategory(
        Guid categoryId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var products = await _productRepo.GetByCategoryAsync(categoryId, page, pageSize, ct);
        var totalCount = await _productRepo.GetTotalCountAsync(categoryId, ct);

        var items = new List<ProductResponse>();
        foreach (var product in products)
        {
            var stock = await _stockRepo.GetByProductIdAsync(product.Id, ct);
            items.Add(MapToResponse(product, stock));
        }

        return Ok(new PagedResponse<ProductResponse>(items, totalCount, page, pageSize));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(
        [FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryId = request.CategoryId,
            ImageUrl = request.ImageUrl,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _productRepo.CreateAsync(product, ct);

        // Создаём запись стока
        await _stockRepo.SetStockAsync(product.Id, request.InitialStock, ct);

        // Индексируем в ElasticSearch
        await _searchService.IndexProductAsync(product, ct);

        var stock = await _stockRepo.GetByProductIdAsync(product.Id, ct);

        return CreatedAtAction(nameof(GetById), new { id = product.Id },
            MapToResponse(product, stock));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var existing = await _productRepo.GetByIdAsync(id, ct);
        if (existing is null) return NotFound();

        existing.Name = request.Name;
        existing.Description = request.Description;
        existing.Price = request.Price;
        existing.CategoryId = request.CategoryId;
        existing.ImageUrl = request.ImageUrl;
        existing.UpdatedAt = DateTime.UtcNow;

        await _productRepo.UpdateAsync(existing, ct);
        await _searchService.IndexProductAsync(existing, ct);

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        await _productRepo.DeactivateAsync(id, ct);
        await _searchService.DeleteProductAsync(id, ct);
        return NoContent();
    }

    [Authorize]
    [HttpGet("{id:guid}/stock")]
    public async Task<ActionResult<StockResponse>> GetStock(Guid id, CancellationToken ct)
    {
        var stock = await _stockRepo.GetByProductIdAsync(id, ct);
        if (stock is null) return NotFound();

        return Ok(new StockResponse(stock.ProductId, stock.Quantity,
            stock.Reserved, stock.Available));
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:guid}/stock")]
    public async Task<IActionResult> SetStock(
        Guid id, [FromBody] SetStockRequest request, CancellationToken ct)
    {
        await _stockRepo.SetStockAsync(id, request.Quantity, ct);
        return NoContent();
    }

    private static ProductResponse MapToResponse(Product product, StockItem? stock)
    {
        return new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.CategoryId,
            product.CategoryName,
            product.ImageUrl,
            product.IsActive,
            stock?.Available ?? 0);
    }
}