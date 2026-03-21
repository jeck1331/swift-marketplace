using CatalogService.Application.DTOs;
using CatalogService.Application.DTOs.Search;
using CatalogService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController: ControllerBase
{
    private readonly IProductSearchService _searchService;

    public SearchController(IProductSearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> Search(
        [FromQuery] SearchRequest request, CancellationToken ct)
    {
        var result = await _searchService.SearchAsync(
            request.Query,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.Page,
            request.PageSize,
            ct);

        var items = result.Products.Select(p => new ProductResponse(
            p.Id, p.Name, p.Description, p.Price,
            p.CategoryId, p.CategoryName, p.ImageUrl,
            p.IsActive, 0)).ToList();

        return Ok(new PagedResponse<ProductResponse>(
            items, result.TotalCount, request.Page, request.PageSize));
    }
}