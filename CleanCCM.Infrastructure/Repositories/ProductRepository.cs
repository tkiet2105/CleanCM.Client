using CleanCCM.Application.Common.Interfaces;
using CleanCCM.Domain.Entities;
using CleanCCM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CleanCCM.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

   
}