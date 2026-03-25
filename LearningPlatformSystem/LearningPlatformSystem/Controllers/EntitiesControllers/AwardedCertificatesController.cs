using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearningPlatformSystem.Context;
using LearningPlatformSystem.Models.TableModels;
using Microsoft.AspNetCore.Authorization;

namespace LearningPlatformSystem.Controllers.EntitiesControllers
{
    public class AwardedCertificatesController : Controller
    {
        private readonly MyContext _context;

        public AwardedCertificatesController(MyContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Index()
        {
            var myContext = _context.AwardedCertificates.Include(a => a.Course).Include(a => a.Learner);
            return View(await myContext.ToListAsync());
        }
        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var awardedCertificate = await _context.AwardedCertificates
                .Include(a => a.Course)
                .Include(a => a.Learner)
                .FirstOrDefaultAsync(m => m.CertificateSerialCode == id);
            if (awardedCertificate == null)
            {
                return NotFound();
            }

            return View(awardedCertificate);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title");
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "FirstName");
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AwardedCertificate awardedCertificate)
        {
            if (ModelState.IsValid)
            {
                var isEnrolled = await _context.Courses
                    .Where(c => c.CourseCode == awardedCertificate.CourseCode)
                    .SelectMany(c => c.Learners)
                    .AnyAsync(l => l.LearnerId == awardedCertificate.LearnerId);

                if (!isEnrolled)
                {
                    ModelState.AddModelError(string.Empty, "Learner is not enrolled in this course");
                    ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title", awardedCertificate.CourseCode);
                    ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "FirstName", awardedCertificate.LearnerId);
                    return View(awardedCertificate);
                }

                _context.Add(awardedCertificate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title", awardedCertificate.CourseCode);
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "FirstName", awardedCertificate.LearnerId);
            return View(awardedCertificate);
        }

        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var awardedCertificate = await _context.AwardedCertificates.FindAsync(id);
            if (awardedCertificate == null)
            {
                return NotFound();
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title", awardedCertificate.CourseCode);
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "FirstName", awardedCertificate.LearnerId);
            return View(awardedCertificate);
        }


        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AwardedCertificate awardedCertificate)
        {
            if (id != awardedCertificate.CertificateSerialCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var isEnrolled = await _context.Courses
                    .Where(c => c.CourseCode == awardedCertificate.CourseCode)
                    .SelectMany(c => c.Learners)
                    .AnyAsync(l => l.LearnerId == awardedCertificate.LearnerId);

                if (!isEnrolled)
                {
                    ModelState.AddModelError(string.Empty, "Learner is not enrolled in this course");
                    ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title", awardedCertificate.CourseCode);
                    ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "FirstName", awardedCertificate.LearnerId);
                    return View(awardedCertificate);
                }

                try
                {
                    _context.Update(awardedCertificate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AwardedCertificateExists(awardedCertificate.CertificateSerialCode))
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

            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title", awardedCertificate.CourseCode);
            ViewData["LearnerId"] = new SelectList(_context.Learners, "LearnerId", "FirstName", awardedCertificate.LearnerId);
            return View(awardedCertificate);
        }


        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var awardedCertificate = await _context.AwardedCertificates
                .Include(a => a.Course)
                .Include(a => a.Learner)
                .FirstOrDefaultAsync(m => m.CertificateSerialCode == id);
            if (awardedCertificate == null)
            {
                return NotFound();
            }

            return View(awardedCertificate);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var awardedCertificate = await _context.AwardedCertificates.FindAsync(id);
            if (awardedCertificate != null)
            {
                _context.AwardedCertificates.Remove(awardedCertificate);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AwardedCertificateExists(int id)
        {
            return _context.AwardedCertificates.Any(e => e.CertificateSerialCode == id);
        }
    }
}
