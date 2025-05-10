using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VazirlikWeb.Data;
using VazirlikWeb.Models;
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

        // GET: News
        public async Task<IActionResult> Index()
        {
            var newsList = await _context.News
                .OrderByDescending(n => n.Date)  // Yangiliklarni eng oxirgisidan birinchi bo'lib ko'rsatish
                .ToListAsync();
            return View(newsList);
        }

        // GET: News/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FirstOrDefaultAsync(m => m.Id == id);
            if (news == null) return NotFound();

            return View(news);
        }

        // GET: News/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: News/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FindAsync(id);
            if (news == null) return NotFound();

            return View(news);
        }

        // POST: News/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // GET: News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var news = await _context.News.FirstOrDefaultAsync(m => m.Id == id);
            if (news == null) return NotFound();

            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news != null)
            {
                _context.News.Remove(news);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yangilik muvaffaqiyatli o‘chirildi.";  // O‘chirish muvaffaqiyatli
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.Id == id);
        }
    }
}
