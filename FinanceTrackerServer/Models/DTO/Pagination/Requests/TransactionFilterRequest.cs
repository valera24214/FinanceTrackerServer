using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Models.DTO.Pagination.Requests
{
    public class TransactionFilterRequest:PaginationRequest
    {
        public TransactionType? Type { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string? Description { get; set; }
    }
}
