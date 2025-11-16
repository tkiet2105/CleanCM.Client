using CleanCCM.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanCCM.Application.Common.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {
        //Task<IEnumerable<Product>> GetProductsWithCategoriesAsync(CancellationToken cancellationToken = default);
        //Task<IEnumerable<Product>> GetProductsWithTagsAsync(CancellationToken cancellationToken = default);
        //Task<Product?> GetProductDetailAsync(Guid id, CancellationToken cancellationToken = default);
        //Task<IEnumerable<Product>> GetProductsByCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
        //Task<IEnumerable<Product>> GetProductsByTagAsync(Guid tagId, CancellationToken cancellationToken = default);
    }
  
    public interface ICategoryRepository : IRepository<Category>
    {
    }
    public interface ITagRepository : IRepository<Tag>
    {
    }
    public interface IProductCategoryRepository : IRepository<ProductCategory>
    {
    }
    public interface IProductTagRepository : IRepository<ProductTag>
    {
    }
    public interface IReactionRepository : IRepository<Reaction>
    {
    }
    public interface ICommentRepository : IRepository<Comment>
    {
    }
    public interface IRatingRepository : IRepository<Rating>
    {
    }

}
