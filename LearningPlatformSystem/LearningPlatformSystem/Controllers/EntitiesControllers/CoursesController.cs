using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearningPlatformSystem.Context;
using LearningPlatformSystem.Models.TableModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LearningPlatformSystem.Models.CoreModels;

namespace LearningPlatformSystem.Controllers.EntitiesControllers
{
    public class CoursesController : Controller
    {
        private readonly MyContext _context;
        private readonly UserManager<AuthUsers> _userManager;

        public CoursesController(MyContext context,UserManager<AuthUsers> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Index()
        {
            var myContext = _context.Courses.Include(c => c.Tutor);
            return View(await myContext.ToListAsync());
        }

        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Learners)
                .FirstOrDefaultAsync(c => c.CourseCode == id);

            if (course == null) return NotFound();

            if (User.IsInRole("Learner"))
            {
                var user = await _userManager.GetUserAsync(User);
                var learner = await _context.Learners.FirstOrDefaultAsync(l => l.Email == user.Email);

                if (learner != null)
                {
                    var existingRequest = await _context.EnrollmentRequests
                        .FirstOrDefaultAsync(r => r.CourseId == id && r.LearnerId == learner.LearnerId);

                    ViewBag.ExistingRequest = existingRequest;
                }
            }

            return View(course);
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Create()
        {
            ViewData["TutorId"] = new SelectList(_context.Set<Tutor>(), "TutorId", "FirstName");
            return View();
        }


        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TutorId"] = new SelectList(_context.Set<Tutor>(), "TutorId", "FirstName", course.TutorId);
            return View(course);
        }
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            ViewData["TutorId"] = new SelectList(_context.Set<Tutor>(), "TutorId", "FirstName", course.TutorId);
            return View(course);
        }

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Course course)
        {
            if (id != course.CourseCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseCode))
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
            ViewData["TutorId"] = new SelectList(_context.Set<Tutor>(), "TutorId", "FirstName", course.TutorId);
            return View(course);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Tutor)
                .FirstOrDefaultAsync(m => m.CourseCode == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseCode == id);
        }
    }
}
