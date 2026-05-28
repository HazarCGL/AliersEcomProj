using AliersEcom.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AliersEcom.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, string? category)
        {
            if (!string.IsNullOrEmpty(search) || !string.IsNullOrEmpty(category))
            {
                if (!User.Identity.IsAuthenticated)
                {
                    TempData["LoginRequired"] = "Urunleri filtrelemek icin giris yapman gerekiyor.";
                    return RedirectToAction("Login", "Account");
                }
            }

            var products = _db.Products.Where(p => p.IsApproved);

            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search) || p.Description.Contains(search) || p.Tags.Contains(search));

            if (!string.IsNullOrEmpty(category))
                products = products.Where(p => p.Category == category);

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Categories = await _db.Products
                .Where(p => p.IsApproved)
                .Select(p => p.Category)
                .Distinct()
                .ToListAsync();

            // Kullanicinin favorilerini al
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var favoriteIds = await _db.Favorites
                        .Where(f => f.UserId == user.Id)
                        .Select(f => f.ProductId)
                        .ToListAsync();
                    ViewBag.FavoriteIds = favoriteIds;
                }
            }
            else
            {
                ViewBag.FavoriteIds = new List<int>();
            }

            // Okunmamis bildirim sayisi
            if (User.Identity.IsAuthenticated)
            {
                var notifUser = await _userManager.GetUserAsync(User);
                if (notifUser != null)
                {
                    ViewBag.UnreadCount = await _db.Notifications
                        .CountAsync(n => n.UserId == notifUser.Id && !n.IsRead);
                    ViewBag.Notifications = await _db.Notifications
                        .Where(n => n.UserId == notifUser.Id)
                        .OrderByDescending(n => n.CreatedAt)
                        .Take(5)
                        .ToListAsync();
                }
            }

            return View(await products.ToListAsync());
        }
    }
}
/*
███╗░░░███╗░█████╗░██████╗░███████╗  ██████╗░██╗░░░██╗  ██╗░░██╗░█████╗░███████╗░█████╗░██████╗░
████╗░████║██╔══██╗██╔══██╗██╔════╝  ██╔══██╗╚██╗░██╔╝  ██║░░██║██╔══██╗╚════██║██╔══██╗██╔══██╗
██╔████╔██║███████║██║░░██║█████╗░░  ██████╦╝░╚████╔╝░  ███████║███████║░░███╔═╝███████║██████╔╝
██║╚██╔╝██║██╔══██║██║░░██║██╔══╝░░  ██╔══██╗░░╚██╔╝░░  ██╔══██║██╔══██║██╔══╝░░██╔══██║██╔══██╗
██║░╚═╝░██║██║░░██║██████╔╝███████╗  ██████╦╝░░░██║░░░  ██║░░██║██║░░██║███████╗██║░░██║██║░░██║
╚═╝░░░░░╚═╝╚═╝░░╚═╝╚═════╝░╚══════╝  ╚═════╝░░░░╚═╝░░░  ╚═╝░░╚═╝╚═╝░░╚═╝╚══════╝╚═╝░░╚═╝╚═╝░░╚═╝github/HazarCGL if youre interested in more projects like this go check my Github
 */