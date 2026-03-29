using System.Diagnostics;
using FreightManagement.Models;
using Microsoft.AspNetCore.Mvc;

namespace FreightManagement.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // Nếu đã đăng nhập, redirect về dashboard tương ứng
            var role = HttpContext.Session.GetString("RoleName");
            if (role != null)
            {
                return role switch
                {
                    "Admin" => RedirectToAction("Index", "Admin"),
                    "KhachHang" => RedirectToAction("Index", "Orders"),
                    "QuanLyKho" => RedirectToAction("Index", "Warehouse"),
                    "TaiXe" => RedirectToAction("Index", "Driver"),
                    _ => View()
                };
            }
            return View();
        }

    }
}
