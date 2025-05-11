using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using VazirlikWeb.Data;
using VazirlikWeb.Models;

namespace VazirlikWeb.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AssignmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Helper method to set up common ViewBag properties
        private void SetupCommonViewBagProperties()
        {
            // Status color dictionary for badges
            ViewBag.StatusColors = new Dictionary<AssignmentStatus, string>
            {
                { AssignmentStatus.New, "bg-primary" },
                { AssignmentStatus.InProgress, "bg-info" },
                { AssignmentStatus.UnderReview, "bg-warning" },
                { AssignmentStatus.Completed, "bg-success" },
                { AssignmentStatus.Cancelled, "bg-danger" }
            };

            // Priority icons dictionary
            ViewBag.PriorityIcons = new Dictionary<AssignmentPriority, string>
            {
                { AssignmentPriority.Low, "fa-arrow-down" },
                { AssignmentPriority.Medium, "fa-equals" },
                { AssignmentPriority.High, "fa-arrow-up" }
            };

            // Priority names dictionary
            ViewBag.PriorityNames = new Dictionary<AssignmentPriority, string>
            {
                { AssignmentPriority.Low, "Past" },
                { AssignmentPriority.Medium, "O'rta" },
                { AssignmentPriority.High, "Yuqori" }
            };

            // Status names dictionary
            ViewBag.StatusNames = new Dictionary<AssignmentStatus, string>
            {
                { AssignmentStatus.New, "Yangi" },
                { AssignmentStatus.InProgress, "Bajarilmoqda" },
                { AssignmentStatus.UnderReview, "Kutilmoqda" },
                { AssignmentStatus.Completed, "Bajarilgan" },
                { AssignmentStatus.Cancelled, "Bekor qilingan" }
            };
        }

        // GET: Assignment
        [Authorize]
        public async Task<IActionResult> Index()
        {
            SetupCommonViewBagProperties();

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Administrator");
            var isEditor = await _userManager.IsInRoleAsync(currentUser, "Editor");

            if (isAdmin || isEditor)
            {
                // Admin va Editorlar barcha topshiriqlarni ko'ra oladi
                var allAssignments = await _context.Assignments
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                return View(allAssignments);
            }
            else
            {
                // Oddiy foydalanuvchilar faqat o'zlariga tayinlangan topshiriqlarni ko'radi
                var myAssignments = await _context.Assignments
                    .Include(t => t.CreatedBy)
                    .Include(t => t.AssignedTo)  // Add this line to include AssignedTo for all users
                    .Where(t => t.AssignedToId == currentUser.Id)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync();
                return View(myAssignments);
            }
        }

        // GET: Assignment/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            SetupCommonViewBagProperties();

            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            // Tekshirish: foydalanuvchi ushbu topshiriqni ko'ra oladimi?
            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Administrator");
            var isEditor = await _userManager.IsInRoleAsync(currentUser, "Editor");

            if (!isAdmin && !isEditor && assignment.AssignedToId != currentUser.Id)
            {
                return Forbid();
            }

            return View(assignment);
        }

        // GET: Assignment/Create
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Create()
        {
            SetupCommonViewBagProperties();

            var users = await _userManager.Users.ToListAsync();

            var userList = users.Select(u => new
            {
                u.Id,
                FullName = GetFullName(u)
            }).ToList();

            ViewBag.Users = new SelectList(userList, "Id", "FullName");

            // Standart qiymatlar bilan yangi Assignment obyekti yaratish
            // Barcha DateTime qiymatlarini aniq UTC sifatida belgilash
            var utcToday = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Utc);
            var utcDueDate = DateTime.SpecifyKind(DateTime.Today.AddDays(7), DateTimeKind.Utc);

            var assignment = new Assignment
            {
                StartDate = utcToday,
                DueDate = utcDueDate,
                Status = AssignmentStatus.New,
                Priority = AssignmentPriority.Medium,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return View(assignment);
        }

        // POST: Assignment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,StartDate,DueDate,Status,Priority,AssignedToId")] Assignment assignment)
        {
            try
            {
                // Hozirgi foydalanuvchini olish
                var currentUser = await _userManager.GetUserAsync(User);

                // Ensure all DateTime values are properly set as UTC
                // Barcha DateTime qiymatlarini aniq UTC sifatida belgilash
                assignment.StartDate = ConvertToUtc(assignment.StartDate);
                assignment.DueDate = ConvertToUtc(assignment.DueDate);

                // Yaratuvchi uchun qiymatni o'rnatish
                assignment.CreatedById = currentUser.Id;
                assignment.CreatedAt = DateTime.UtcNow;
                assignment.UpdatedAt = DateTime.UtcNow;

                // Sana validatsiyasi - control sifatida
                if (assignment.DueDate < assignment.StartDate)
                {
                    ModelState.AddModelError("DueDate", "Tugash sanasi boshlanish sanasidan keyin bo'lishi kerak");

                    // Xatolik holatida foydalanuvchilar ro'yxatini olish va viewga qaytarish
                    var usersForDropdown = await GetUserListForDropdown();
                    ViewBag.Users = new SelectList(usersForDropdown, "Id", "FullName", assignment.AssignedToId);
                    SetupCommonViewBagProperties();
                    return View(assignment);
                }

                // ModelState.IsValid ga tekshirish qilishdan oldin
                ModelState.Remove("CreatedById");
                ModelState.Remove("CreatedBy");
                ModelState.Remove("AssignedTo");

                // Agar model to'g'ri bo'lsa
                if (ModelState.IsValid)
                {
                    // Yangi topshiriqni qo'shish
                    _context.Add(assignment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Saqlashda xatolik yuz berdi: " + ex.Message);
            }

            // Xatolik bo'lsa, foydalanuvchilar ro'yxatini olish
            var users = await _userManager.Users.ToListAsync();

            // Foydalanuvchilarni "FullName" ni hisoblab va SelectList yaratish
            var userList = users.Select(u => new
            {
                u.Id,
                FullName = GetFullName(u) ?? "Ism yo'q"
            }).ToList();

            // ViewBag orqali foydalanuvchilarni uzatish
            ViewBag.Users = new SelectList(userList, "Id", "FullName", assignment.AssignedToId);
            SetupCommonViewBagProperties();

            return View(assignment);
        }

        private DateTime ConvertToUtc(DateTime date)
        {
            if (date.Kind == DateTimeKind.Utc)
                return date;

            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        // Nullable DateTime uchun yordamchi metod
        private DateTime? ConvertToUtcNullable(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return null;

            return ConvertToUtc(dateTime.Value);
        }

        // Foydalanuvchining to'liq ismini olish uchun yordamchi metod
        private string GetFullName(ApplicationUser user)
        {
            return $"{user.LastName} {user.FirstName}".Trim();
        }

        // Dropdown uchun foydalanuvchilar ro'yxatini olish
        private async Task<List<dynamic>> GetUserListForDropdown()
        {
            var users = await _userManager.Users.ToListAsync();

            var userList = users.Select(u => new
            {
                u.Id,
                FullName = GetFullName(u) ?? "Ism yo'q"
            }).ToList<dynamic>();

            return userList;
        }

        // GET: Assignment/Edit/5
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Edit(int? id)
        {
            SetupCommonViewBagProperties();

            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            var users = await _userManager.Users.ToListAsync();

            var userList = users.Select(u => new
            {
                u.Id,
                FullName = GetFullName(u)
            }).ToList();

            ViewBag.Users = new SelectList(userList, "Id", "FullName", assignment.AssignedToId);

            return View(assignment);
        }

        // POST: Assignment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,StartDate,DueDate,Status,Priority,AssignedToId")] Assignment assignment)
        {
            if (id != assignment.Id)
            {
                return NotFound();
            }

            // ModelState.IsValid ga tekshirish qilishdan oldin
            ModelState.Remove("CreatedById");
            ModelState.Remove("CreatedBy");
            ModelState.Remove("AssignedTo");

            if (ModelState.IsValid)
            {
                try
                {
                    // Mavjud topshiriqni olish
                    var existingAssignment = await _context.Assignments.FindAsync(id);
                    if (existingAssignment == null)
                    {
                        return NotFound();
                    }

                    // Ensure input dates are UTC
                    assignment.StartDate = ConvertToUtc(assignment.StartDate);
                    assignment.DueDate = ConvertToUtc(assignment.DueDate);

                    // Yangi ma'lumotlar bilan yangilash
                    existingAssignment.Title = assignment.Title;
                    existingAssignment.Description = assignment.Description;
                    existingAssignment.StartDate = assignment.StartDate;
                    existingAssignment.DueDate = assignment.DueDate;
                    existingAssignment.Status = assignment.Status;
                    existingAssignment.Priority = assignment.Priority;
                    existingAssignment.AssignedToId = assignment.AssignedToId;
                    existingAssignment.UpdatedAt = DateTime.UtcNow;

                    // Agar status "Completed" bo'lsa, bajarilgan sanani belgilash
                    if (assignment.Status == AssignmentStatus.Completed && !existingAssignment.CompletedDate.HasValue)
                    {
                        existingAssignment.CompletedDate = DateTime.UtcNow;
                    }
                    else if (assignment.Status != AssignmentStatus.Completed)
                    {
                        existingAssignment.CompletedDate = null;
                    }

                    _context.Update(existingAssignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AssignmentExists(assignment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "FullName", assignment.AssignedToId);
            SetupCommonViewBagProperties();
            return View(assignment);
        }

        // GET: Assignment/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            SetupCommonViewBagProperties();

            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _context.Assignments
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // POST: Assignment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment != null)
            {
                _context.Assignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Assignment/UpdateStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, AssignmentStatus status)
        {
            var assignment = await _context.Assignments.FindAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Administrator");
            var isEditor = await _userManager.IsInRoleAsync(currentUser, "Editor");

            // Faqat administratorlar, editorlar yoki topshiriq bajaruvchisi statusni o'zgartira oladi
            if (!isAdmin && !isEditor && assignment.AssignedToId != currentUser.Id)
            {
                return Forbid();
            }

            assignment.Status = status;
            assignment.UpdatedAt = DateTime.UtcNow;

            // Agar status "Completed" bo'lsa, bajarilgan sanani saqlash
            if (status == AssignmentStatus.Completed && !assignment.CompletedDate.HasValue)
            {
                assignment.CompletedDate = DateTime.UtcNow;
            }
            else if (status != AssignmentStatus.Completed)
            {
                assignment.CompletedDate = null;
            }

            _context.Update(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = assignment.Id });
        }

        // GET: Assignment/MyAssignments
        [Authorize]
        public async Task<IActionResult> MyAssignments()
        {
            SetupCommonViewBagProperties();

            var currentUser = await _userManager.GetUserAsync(User);

            var myAssignments = await _context.Assignments
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)  // Add this line to include AssignedTo
                .Where(t => t.AssignedToId == currentUser.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View("Index", myAssignments);
        }

        // GET: Assignment/Management
        [Authorize(Roles = "Administrator,Editor")]
        public async Task<IActionResult> Management()
        {
            SetupCommonViewBagProperties();

            var assignments = await _context.Assignments
                .Include(t => t.CreatedBy)
                .Include(t => t.AssignedTo)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(assignments);
        }

        private bool AssignmentExists(int id)
        {
            return _context.Assignments.Any(e => e.Id == id);
        }
    }
}