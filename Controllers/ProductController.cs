using AliersEcom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AliersEcom.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [Authorize(Roles = "Researcher,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Researcher,Admin")]
        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var savePath = Path.Combine(_env.WebRootPath, "uploads", fileName);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                product.ImageUrl = "/uploads/" + fileName;
            }

            product.IsApproved = false;
            product.CreatedAt = DateTime.Now;
            product.AddedByUserId = User.Identity.Name;
            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Ürün eklendi, admin onayı bekleniyor.";
            return RedirectToAction("MyProducts");
        }

        [Authorize(Roles = "Researcher,Admin")]
        public async Task<IActionResult> MyProducts()
        {
            var userId = User.Identity.Name;
            var products = await _db.Products
                .Where(p => p.AddedByUserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(products);
        }

        [Authorize(Roles = "Researcher,Admin")]
        public async Task<IActionResult> MyDelete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null && product.AddedByUserId == User.Identity.Name)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("MyProducts");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminList()
        {
            var products = await _db.Products.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                product.IsApproved = true;
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("AdminList");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product != null)
            {
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction("AdminList");
        }

        // Ürün detay sayfası
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null || !product.IsApproved)
                return RedirectToAction("Index", "Home");
            return View(product);
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