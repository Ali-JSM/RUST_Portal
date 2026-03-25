using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningPlatformSystem.Context;
using LearningPlatformSystem.Models.TableModels;
using IWebHostEnvironment = Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
using Microsoft.AspNetCore.Authorization;

namespace LearningPlatformSystem.Controllers.EntitiesControllers
{
    public class TutorsController : Controller
    {
        private readonly MyContext _context;
        private readonly IWebHostEnvironment _host;

        public TutorsController(MyContext context, IWebHostEnvironment host)
        {
            _context = context;
            _host = host;

        }
        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Tutors.ToListAsync());
        }
        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tutor = await _context.Tutors
                .FirstOrDefaultAsync(m => m.TutorId == id);
            if (tutor == null)
            {
                return NotFound();
            }

            return View(tutor);
        }
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Tutor tutor)
        {
            if (ModelState.IsValid)
            {
                string fileName = string.Empty;
                if (tutor.clientFile != null)
                {
                    string myUpload = Path.Combine(_host.WebRootPath, "images");
                    fileName = tutor.clientFile.FileName;
                    string fullPath = Path.Combine(myUpload, fileName);
                    tutor.clientFile.CopyTo(new FileStream(fullPath, FileMode.Create));
                    tutor.imagePath = fileName;
                }
                _context.Add(tutor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tutor);
        }
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tutor = await _context.Tutors.FindAsync(id);
            if (tutor == null)
            {
                return NotFound();
            }
            return View(tutor);
        }

        [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Editor")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Tutor tutor)
        {
            if (id != tutor.TutorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tutor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TutorExists(tutor.TutorId))
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
            return View(tutor);
        }

            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> Delete(int? id)
            {
                if (id == null)
                {
                    return NotFound();
                }

                var tutor = await _context.Tutors
                    .FirstOrDefaultAsync(m => m.TutorId == id);
                if (tutor == null)
                {
                    return NotFound();
                }

                return View(tutor);
            }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tutor = await _context.Tutors.FindAsync(id);
            if (tutor != null)
            {
                _context.Tutors.Remove(tutor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TutorExists(int id)
        {
            return _context.Tutors.Any(e => e.TutorId == id);
        }
    }
}
