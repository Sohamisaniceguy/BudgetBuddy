namespace BudgetBuddy.Service
{

    public interface IUserService
    {
        int GetLoggedInUserId();
    }

    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetLoggedInUserId()
        {
            if (_httpContextAccessor.HttpContext?.Session.GetInt32("UserId") != null)
            {
                return (int)_httpContextAccessor.HttpContext.Session.GetInt32("UserId");
            }

            return -1;
        }
    }

}
