using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LearningPlatformSystem.Context;
using LearningPlatformSystem.Models.TableModels;
using Microsoft.AspNetCore.Authorization;

namespace LearningPlatformSystem.Controllers.EntitiesControllers
{
    public class LearningMaterialsController : Controller
    {
        private readonly MyContext _context;


        public LearningMaterialsController(MyContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Index()
        {
            var myContext = _context.LearningMaterials.Include(l => l.Course);
            return View(await myContext.ToListAsync());
        }
        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var learningMaterial = await _context.LearningMaterials
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.MaterialId == id);
            if (learningMaterial == null)
            {
                return NotFound();
            }

            return View(learningMaterial);
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Create()
        {
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title");
            return View();
        }

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LearningMaterial learningMaterial)
        {
            if (ModelState.IsValid)
            {
                _context.Add(learningMaterial);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title", learningMaterial.CourseCode);
            return View(learningMaterial);
        }

        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var learningMaterial = await _context.LearningMaterials.FindAsync(id);
            if (learningMaterial == null)
            {
                return NotFound();
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title", learningMaterial.CourseCode);
            return View(learningMaterial);
        }

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LearningMaterial learningMaterial)
        {
            if (id != learningMaterial.MaterialId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(learningMaterial);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LearningMaterialExists(learningMaterial.MaterialId))
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
            ViewData["CourseCode"] = new SelectList(_context.Courses, "CourseCode", "Title", learningMaterial.CourseCode);
            return View(learningMaterial);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var learningMaterial = await _context.LearningMaterials
                .Include(l => l.Course)
                .FirstOrDefaultAsync(m => m.MaterialId == id);
            if (learningMaterial == null)
            {
                return NotFound();
            }

            return View(learningMaterial);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var learningMaterial = await _context.LearningMaterials.FindAsync(id);
            if (learningMaterial != null)
            {
                _context.LearningMaterials.Remove(learningMaterial);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LearningMaterialExists(int id)
        {
            return _context.LearningMaterials.Any(e => e.MaterialId == id);
        }
    }
}
