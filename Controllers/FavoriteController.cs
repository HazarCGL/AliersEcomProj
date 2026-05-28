using AliersEcom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AliersEcom.Controllers;

namespace AliersEcom.Controllers
{
    [Authorize]
    public class FavoriteController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoriteController(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var favorites = await _db.Favorites
                .Include(f => f.Product)
                .Where(f => f.UserId == user.Id)
                .OrderByDescending(f => f.AddedAt)
                .ToListAsync();
            return View(favorites);
        }

        public async Task<IActionResult> Add(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var product = await _db.Products.FindAsync(productId);
            if (product == null) return RedirectToAction("Index", "Home");

            var existing = await _db.Favorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ProductId == productId);

            if (existing == null)
            {
                _db.Favorites.Add(new Favorite
                {
                    UserId = user.Id,
                    ProductId = productId,
                    AddedAt = DateTime.Now
                });
                await _db.SaveChangesAsync();

                await NotificationController.Send(_db, user.Id,
                    "Favorilere Eklendi ❤️",
                    "\"" + product.Name + "\" ürünü favorilerinize eklendi.",
                    "❤️");
            }
            else
            {
                _db.Favorites.Remove(existing);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var fav = await _db.Favorites
                .FirstOrDefaultAsync(f => f.Id == id && f.UserId == user.Id);

            if (fav != null)
            {
                _db.Favorites.Remove(fav);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
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