using FinanceTrackerServer.Data;
using FinanceTrackerServer.Models.DTO;
using FinanceTrackerServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceTrackerServer.Services
{
    public class CategoryService:ICategoryService
    {
        private readonly AppDbContext _context;

        public CategoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryDto>> GetCategories()
        {
            return await _context.Categories
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type
            })
            .ToListAsync();
        }
    }
}
