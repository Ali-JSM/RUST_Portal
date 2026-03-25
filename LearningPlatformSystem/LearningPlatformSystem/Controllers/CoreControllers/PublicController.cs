using LearningPlatformSystem.Context;
using LearningPlatformSystem.Models.CoreModels;
using LearningPlatformSystem.Models.TableModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LearningPlatformSystem.Controllers.CoreControllers
{
    [AllowAnonymous]
    public class PublicController : Controller
    {
        private readonly UserManager<AuthUsers> _userManager;

        public PublicController(UserManager<AuthUsers> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Courses()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }
        [HttpGet]
        public IActionResult CreateUser()
        {
            ViewBag.Roles = new List<string> { "Learner" };
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(Register model)
        {
            if (ModelState.IsValid)
            {
                var user = new AuthUsers
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    SignUpDate = DateTime.Now,
                    Role = model.Role
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);

                    if (model.Role == "Learner")
                    {
                        using var scope = HttpContext.RequestServices.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<MyContext>();

                        context.Learners.Add(new Learner
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            Email = model.Email,
                            SignUpDate = DateTime.Now
                        });
                        await context.SaveChangesAsync();
                    }

                    return RedirectToAction("Index", "Public");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = new List<string> { "Learner" };
            return View(model);
        }
    }
}
