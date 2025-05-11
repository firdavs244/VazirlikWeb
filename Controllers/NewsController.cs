using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VazirlikWeb.Data;
using VazirlikWeb.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VazirlikWeb.Controllers
{
    public class NewsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NewsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: News - Barcha foydalanuvchilar uchun ochiq
        public async Task<IActionResult> Index()
        {
            var newsList = await _context.News
                .OrderByDescending(n => n.Date)  // Yangiliklarni eng oxirgisidan birinchi bo'lib ko'rsatish
                .ToListAsync();
            return View(newsList);
        }

        // GET: News/Details/5 - Barcha foydalanuvchilar uchun ochiq
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FirstOrDefaultAsync(m => m.Id == id);
            if (news == null) return NotFound();

            return View(news);
        }

        // GET: News/Create - Faqat Administrator va Editor rollari uchun
        [Authorize(Roles = "Administrator,Editor")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: News/Create - Faqat Administrator va Editor rollari uchun
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Create([Bind("Title,Content,ImageUrl")] News news)
        {
            if (ModelState.IsValid)
            {
                // Null yoki bo'sh qiymatlarni tekshirish
                if (string.IsNullOrEmpty(news.Content) || string.IsNullOrEmpty(news.ImageUrl))
                {
                    ModelState.AddModelError("", "Mazmun yoki rasm havolasi bo'sh bo'lmasligi kerak.");
                    return View(news);
                }

                news.Date = DateTime.UtcNow;
                _context.Add(news);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yangilik muvaffaqiyatli saqlandi.";
                return RedirectToAction(nameof(Index));
            }
            return View(news);
        }

        // GET: News/Edit/5 - Faqat Administrator va Editor rollari uchun
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            return View(news);
        }

        // POST: News/Edit/5 - Faqat Administrator va Editor rollari uchun
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ImageUrl")] News news)
        {
            if (id != news.Id) return NotFound();

            if (ModelState.IsValid)
            {
                // Null yoki bo'sh qiymatlarni tekshirish
                if (string.IsNullOrEmpty(news.Content) || string.IsNullOrEmpty(news.ImageUrl))
                {
                    ModelState.AddModelError("", "Mazmun yoki rasm havolasi bo'sh bo'lmasligi kerak.");
                    return View(news);
                }

                try
                {
                    news.Date = DateTime.UtcNow;
                    _context.Update(news);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Yangilik muvaffaqiyatli yangilandi.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsExists(news.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(news);
        }

        // GET: News/Delete/5 - Faqat Administrator rollari uchun
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FirstOrDefaultAsync(m => m.Id == id);
            if (news == null) return NotFound();

            return View(news);
        }

        // POST: News/Delete/5 - Faqat Administrator rollari uchun
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yangilik muvaffaqiyatli o'chirildi.";  // O'chirish muvaffaqiyatli
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: News/Management - Faqat Administrator va Editor rollari uchun
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Management()
        {
            var newsList = await _context.News
                .OrderByDescending(n => n.Date)
                .ToListAsync();
            return View(newsList);
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Id == id);
        }
    }
}