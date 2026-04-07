namespace FreightManagement.Helpers
{
    public class SessionHelper
    {
        public static bool IsLoggedIn(IHttpContextAccessor accessor)
            => accessor.HttpContext?.Session.GetInt32("UserId") != null;

        public static int? GetUserId(IHttpContextAccessor accessor)
            => accessor.HttpContext?.Session.GetInt32("UserId");

        public static string? GetRole(IHttpContextAccessor accessor)
            => accessor.HttpContext?.Session.GetString("RoleName");

        public static bool IsRole(IHttpContextAccessor accessor, string role)
            => GetRole(accessor) == role;
    }
}
//