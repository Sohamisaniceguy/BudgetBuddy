using Microsoft.AspNetCore.Http;

public static class UserUtility
{
    private const string SessionKeyUserId = "UserId";

    public static void SetUserId(int userId, ISession session)
    {
        session.SetInt32(SessionKeyUserId, userId);
    }

    public static int? GetUserId(ISession session)
    {
        return session.GetInt32(SessionKeyUserId);
    }
}
