using CleanCCM.BlazorUI.Services.Interfaces;
using CleanCCM.Shared.Common;
using CleanCCM.Shared.Common.DTOs;
using CleanCCM.Shared.Products.Requests;

namespace CleanCCM.BlazorUI.Services.Implementations;

public class ProductClient : BaseApiClient, IProductClient
{
    public ProductClient(IHttpClientFactory httpClientFactory)
    : base(httpClientFactory, "Api")
    {
    }

    public Task<ApiResult<List<ProductDto>>> GetAll(GetAllProductQueryRequest getAllProductQueryRequest ,CancellationToken cancellationToken = default)
         => GetAsync<List<ProductDto>>("api/product/get-all", getAllProductQueryRequest, cancellationToken);

   
}
