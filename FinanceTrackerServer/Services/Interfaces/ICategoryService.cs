using FinanceTrackerServer.Models.DTO;

namespace FinanceTrackerServer.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task<List<CategoryDto>> GetCategories();
    }
}
