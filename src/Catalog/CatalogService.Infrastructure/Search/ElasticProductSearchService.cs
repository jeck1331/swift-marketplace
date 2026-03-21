using CatalogService.Application.DTOs;
using CatalogService.Application.Interfaces;
using CatalogService.Domain.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Configuration;

namespace CatalogService.Infrastructure.Search;

public class ElasticProductSearchService : IProductSearchService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "products";

    public ElasticProductSearchService(IConfiguration configuration)
    {
        var uri = configuration.GetConnectionString("ElasticSearch")
            ?? "http://localhost:9200";

        var settings = new ElasticsearchClientSettings(new Uri(uri))
            .DefaultIndex(IndexName);

        _client = new ElasticsearchClient(settings);
    }

    public async Task IndexProductAsync(Product product, CancellationToken ct = default)
    {
        var document = new ProductDocument
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            CategoryName = product.CategoryName,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive
        };

        await _client.IndexAsync(document, i => i
            .Index(IndexName)
            .Id(product.Id.ToString()),
            ct);
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken ct = default)
    {
        await _client.DeleteAsync(IndexName, productId.ToString(), ct);
    }

    public async Task<SearchResult> SearchAsync(
        string? query,
        Guid? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var response = await _client.SearchAsync<ProductDocument>(s => s
            .Index(IndexName)
            .From((page - 1) * pageSize)
            .Size(pageSize)
            .Query(q => q
                .Bool(b =>
                {
                    // полнотекстовый поиск
                    if (!string.IsNullOrWhiteSpace(query))
                    {
                        b.Must(must => must
                            .MultiMatch(mm => mm
                                .Query(query)
                                .Fields(new[] { "name", "description" })
                                .Fuzziness(new Fuzziness("AUTO"))
                            )
                        );
                    }

                    // FILTER: точные условия без scoring
                    var filters = new List<Action<QueryDescriptor<ProductDocument>>>();

                    // Только активные
                    filters.Add(f => f.Term(t => t
                        .Field(p => p.IsActive)
                        .Value(true)));

                    // По категории
                    if (categoryId.HasValue)
                    {
                        filters.Add(f => f.Term(t => t
                            .Field(p => p.CategoryId)
                            .Value(categoryId.Value.ToString())));
                    }

                    // По цене
                    if (minPrice.HasValue || maxPrice.HasValue)
                    {
                        filters.Add(f => f.Range(r => r
                            .NumberRange(nr =>
                            {
                                nr.Field(p => p.Price);
                                if (minPrice.HasValue)
                                    nr.Gte((double)minPrice.Value);
                                if (maxPrice.HasValue)
                                    nr.Lte((double)maxPrice.Value);
                            })));
                    }

                    if (filters.Count > 0)
                    {
                        b.Filter(filters.ToArray());
                    }
                })
            ),
            ct);

        var products = response.Documents.Select(d => new Product
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            Price = d.Price,
            CategoryId = d.CategoryId,
            CategoryName = d.CategoryName,
            ImageUrl = d.ImageUrl,
            IsActive = d.IsActive
        }).ToList();

        return new SearchResult
        {
            Products = products,
            TotalCount = response.Total
        };
    }
}