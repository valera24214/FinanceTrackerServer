using FinanceTrackerServer.Models.DTO;

namespace FinanceTrackerServer.Interfaces
{
    public interface ICategoryService
    {
        public Task<List<CategoryDto>> GetCategories();
    }
}
