using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FreightManagement.Filters
{
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        private readonly string? _role;

        public RequireLoginAttribute(string? role = null)
        {
            _role = role;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session;
            var userId = session.GetInt32("UserId");
            var roleName = session.GetString("RoleName");

            if (userId == null)
            {
                // Chưa đăng nhập
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_role != null && roleName != _role)
            {
                // Sai role → về trang chủ
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }
        }
    }
}
