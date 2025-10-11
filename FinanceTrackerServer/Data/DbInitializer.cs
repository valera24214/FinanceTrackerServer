using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            if (!context.Categories.Any())
            {
                var categories = new[]
                {
                // Расходы
                new Category { Name = "Еда", Type = CategoryType.Expense },
                new Category { Name = "Транспорт", Type = CategoryType.Expense },
                new Category { Name = "Развлечения", Type = CategoryType.Expense },
                new Category { Name = "Жилье", Type = CategoryType.Expense },
                new Category { Name = "Здоровье", Type = CategoryType.Expense },
                new Category { Name = "Одежда", Type = CategoryType.Expense },
                new Category { Name = "Образование", Type = CategoryType.Expense },
                new Category { Name = "Подарки", Type = CategoryType.Expense },
                new Category { Name = "Другое", Type = CategoryType.Expense },
                
                // Доходы
                new Category { Name = "Зарплата", Type = CategoryType.Income },
                new Category { Name = "Фриланс", Type = CategoryType.Income },
                new Category { Name = "Инвестиции", Type = CategoryType.Income },
                new Category { Name = "Подарки", Type = CategoryType.Income },
                new Category { Name = "Другое", Type = CategoryType.Income }
            };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }
        }
    }
}
