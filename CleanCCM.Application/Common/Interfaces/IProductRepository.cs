using CleanCCM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCCM.Application.Common.Interfaces;


/// <summary>
/// Sử dụng: Repository cho Product với các query phức tạp về sản phẩm
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetByTagAsync(Guid tagId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetFeaturedProductsAsync(int take = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<Product?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetMostViewedAsync(int take = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetMostLikedAsync(int take = 10, CancellationToken cancellationToken = default);
    Task<IEnumerable<Product>> GetTopRatedAsync(int take = 10, CancellationToken cancellationToken = default);
    Task<Product?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> IsSlugExistsAsync(string slug, Guid? excludeId = null, CancellationToken cancellationToken = default);
}




