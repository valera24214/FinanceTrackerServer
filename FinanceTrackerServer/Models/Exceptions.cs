namespace FinanceTrackerServer.Models
{
    public interface IBaseException
    {
        int _stausCode { get; }
        string _title { get; }
    }

    public abstract class BaseException: Exception, IBaseException
    {
        public int _stausCode { get; }
        public string _title { get; }

        protected BaseException(string Details, int StatusCode, string Title) : base(Details)
        {
            _stausCode = StatusCode;
            _title = Title;
        }
    }

    public class NotFoundException : BaseException
    {
        public NotFoundException(string Details) : base(Details, 404, "Resource not found") { }
    }

    public class ValidationException : BaseException
    {
        public ValidationException(string Details) : base(Details, 400, "Validation Error") { }
    }
}
