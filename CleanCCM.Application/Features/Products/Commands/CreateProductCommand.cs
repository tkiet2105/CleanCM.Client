using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Application.Common.Models;
using CleanCCM.Domain.Common.Errors;
using CleanCCM.Domain.Entities;
using MediatR;

namespace CleanCCM.Application.Features.Products.Commands;

public record CreateProductCommand : IRequest<Result<Guid>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int Stock { get; init; }
    public List<Guid> CategoryIds { get; init; } = new();
    public List<Guid> TagIds { get; init; } = new();
}
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<Guid>>
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<Tag> _tagRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CreateProductCommandHandler(
        IRepository<Product> productRepository,
        IRepository<Category> categoryRepository,
        IRepository<Tag> tagRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            request.Name,
            request.Description,
            request.Price,
            request.Stock);

        await _productRepository.AddAsync(product, cancellationToken);

        foreach (var categoryId in request.CategoryIds)
        {
            var categoryExists = await _categoryRepository.AnyAsync(c => c.Id == categoryId, cancellationToken);
            if (!categoryExists)
                return Result<Guid>.Failure(Error.NotFound(BaseErrors.NotFoundById, $"Category {categoryId} not found"));

            var productCategory = ProductCategory.Create(product.Id, categoryId, _currentUserService.UserId);
            product.ProductCategories.Add(productCategory);
        }

        foreach (var tagId in request.TagIds)
        {
            var tagExists = await _tagRepository.AnyAsync(t => t.Id == tagId, cancellationToken);
            if (!tagExists)
                return Result<Guid>.Failure(Error.NotFound(BaseErrors.NotFoundById, $"Tag {tagId} not found"));

            var productTag = ProductTag.Create(product.Id, tagId, _currentUserService.UserId);
            product.ProductTags.Add(productTag);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(product.Id);
    }
}