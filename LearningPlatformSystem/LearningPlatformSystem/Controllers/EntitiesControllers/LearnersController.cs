using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LearningPlatformSystem.Context;
using LearningPlatformSystem.Models.TableModels;
using Microsoft.AspNetCore.Authorization;
using LearningPlatformSystem.Models.CoreModels;
using Microsoft.AspNetCore.Identity;

namespace LearningPlatformSystem.Controllers.EntitiesControllers
{
    public class LearnersController : Controller
    {
        private readonly UserManager<AuthUsers> _userManager;
        private readonly MyContext _context;
        private readonly IWebHostEnvironment _host;

        public LearnersController(MyContext context, IWebHostEnvironment host, UserManager<AuthUsers> userManager)
        {
            _context = context;
            _host = host;
            _userManager = userManager;
        }
        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Learners.ToListAsync());
        }

        [Authorize(Roles = "Learner,Editor,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var learners = await _context.Learners
                .FirstOrDefaultAsync(m => m.LearnerId == id);
            if (learners == null)
            {
                return NotFound();
            }

            return View(learners);
        }
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Learner learner)
        {
            if (ModelState.IsValid)
            {
                var user = new AuthUsers
                {
                    UserName = learner.Email,
                    Email = learner.Email,
                    FirstName = learner.FirstName,
                    LastName = learner.LastName,
                    SignUpDate = DateTime.Now,
                    Role = "Learner"
                };

                const string defaultPassword = "123123";
                var result = await _userManager.CreateAsync(user, defaultPassword);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(learner);
                }
                await _userManager.AddToRoleAsync(user, "Learner");

                if (learner.clientFile != null)
                {
                    string myUpload = Path.Combine(_host.WebRootPath, "images");
                    string fileName = learner.clientFile.FileName;
                    string fullPath = Path.Combine(myUpload, fileName);
                    await using var stream = new FileStream(fullPath, FileMode.Create);
                    await learner.clientFile.CopyToAsync(stream);
                    learner.imagePath = fileName;
                }

                learner.SignUpDate = DateTime.Now;
                _context.Add(learner);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(learner);
        }
        [Authorize(Roles = "Editor,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var learners = await _context.Learners.FindAsync(id);
            if (learners == null)
            {
                return NotFound();
            }
            return View(learners);
        }

        [Authorize(Roles = "Editor,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Learner learnerModel)
        {
            if (id != learnerModel.LearnerId) return NotFound();

            if (!ModelState.IsValid) return View(learnerModel);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingLearner = await _context.Learners.AsNoTracking()
                    .FirstOrDefaultAsync(l => l.LearnerId == id);

                if (existingLearner == null) return NotFound();

                if (existingLearner.Email != learnerModel.Email)
                {
                    var emailUser = await _userManager.FindByEmailAsync(learnerModel.Email);
                    if (emailUser != null)
                    {
                        ModelState.AddModelError("Email", "Email is already in use");
                        return View(learnerModel);
                    }
                }

                _context.Update(learnerModel);
                await _context.SaveChangesAsync();

                var user = await _userManager.FindByEmailAsync(existingLearner.Email);
                if (user != null)
                {
                    user.Email = learnerModel.Email;
                    user.UserName = learnerModel.Email;
                    user.FirstName = learnerModel.FirstName;
                    user.LastName = learnerModel.LastName;

                    await _userManager.UpdateAsync(user);
                }

                await transaction.CommitAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Update failed: {ex.Message}");
                return View(learnerModel);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var learners = await _context.Learners
                .FirstOrDefaultAsync(m => m.LearnerId == id);
            if (learners == null)
            {
                return NotFound();
            }

            return View(learners);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var learner = await _context.Learners.FindAsync(id);
            if (learner == null)
            {
                return NotFound();
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var user = await _userManager.FindByEmailAsync(learner.Email);
                if (user != null)
                {
                    var deleteUserResult = await _userManager.DeleteAsync(user);
                    if (!deleteUserResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        foreach (var error in deleteUserResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return View("Delete", learner);
                    }
                }

                _context.Learners.Remove(learner);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View("Delete", learner);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
