using LearningPlatformSystem.Context;
using LearningPlatformSystem.Models.CoreModels;
using LearningPlatformSystem.Models.TableModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class PanelController : Controller
{
    private readonly UserManager<AuthUsers> _userManager;
    private readonly MyContext _context;

    public PanelController(UserManager<AuthUsers> userManager, MyContext context)
    {
        _userManager = userManager;
        _context = context;
    }
    private async Task<Learner> GetOrCreateLearnerAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        var learner = await _context.Learners
            .FirstOrDefaultAsync(l => l.Email == user.Email);

        if (learner == null)
        {
            learner = new Learner
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                SignUpDate = DateTime.Now
            };
            _context.Learners.Add(learner);
            await _context.SaveChangesAsync();
        }

        return learner;
    }
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> MyEnrollmentStatus()
    {
        var learner = await GetOrCreateLearnerAsync();

        var requests = await _context.EnrollmentRequests
            .Include(r => r.Course)
            .Where(r => r.LearnerId == learner.LearnerId)
            .Select(r => new EnrollmentStatusViewModel
            {
                CourseId = r.CourseId,
                CourseTitle = r.Course.Title,
                Status = r.IsApproved ? "Approved" :
                         r.IsRejected ? "Rejected" : "Pending",
                RejectionReason = r.RejectionReason,
                RequestDate = r.RequestDate,
                ProcessedDate = r.ProcessedDate
            })
            .ToListAsync();

        return View(requests);
    }
    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> MyCourses()
    {
        var learner = await GetOrCreateLearnerAsync();

//هون بظهر بس الكورسات المقبولة!!
            var courses = await _context.Learners
            .Where(l => l.LearnerId == learner.LearnerId)
            .SelectMany(l => l.Courses)
            .Where(c => c.Learners.Any(l => l.LearnerId == learner.LearnerId))
            .ToListAsync();

        return View(courses);
    }

    [Authorize(Roles = "Learner")]
    public async Task<IActionResult> MyCertificates()
    {
        var learner = await GetOrCreateLearnerAsync();
        var certificates = await _context.AwardedCertificates
            .Include(ac => ac.Course)
            .Where(ac => ac.LearnerId == learner.LearnerId)
            .ToListAsync();

        return View(certificates);
    }
    [Authorize]
    public async Task<IActionResult> Index()
    {
        if (User.IsInRole("Admin"))
        {
            ViewBag.Stats = new
            {
                TotalUsers = await _userManager.Users.CountAsync(),
                TotalCourses = await _context.Courses.CountAsync(),
                TotalLearners = await _context.Learners.CountAsync(),
                PendingRequests = await _context.EnrollmentRequests
                    .CountAsync(r => !r.IsApproved && !r.IsRejected),
                RecentSignups = await _userManager.Users
                    .OrderByDescending(u => u.SignUpDate)
                    .Take(5)
                    .ToListAsync()
            };
        }
        return View(await _userManager.GetUserAsync(User));
    }

    [Authorize(Roles = "Editor,Admin")]
    public async Task<IActionResult> UsersDetails()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.LastName)
            .ToListAsync();

        var userViewModels = new List<AllUsers>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userViewModels.Add(new AllUsers
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SignUpDate = user.SignUpDate,
                Roles = roles.ToList(),
                AllRoles = new List<string> { "Admin", "Editor", "Learner" }
            });
        }

        return View(userViewModels);
    }

    //فقط للادمن - عمليات

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var userRoles = await _userManager.GetRolesAsync(user);

        var model = new AllUsers
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            SignUpDate = user.SignUpDate,
            Roles = userRoles.ToList(),
            AllRoles = new List<string> { "Admin", "Editor", "Learner" }
        };

        return View(model);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(AllUsers model)
    {
        if (!ModelState.IsValid)
        {
            model.AllRoles = new List<string> { "Admin", "Editor", "Learner" };
            return View(model);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound();

            var oldEmail = user.Email;
            var oldRoles = await _userManager.GetRolesAsync(user);
            var wasLearner = oldRoles.Contains("Learner");
            var isLearner = model.Roles.Contains("Learner");

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded) throw new Exception(updateResult.Errors.First().Description);

            await _userManager.RemoveFromRolesAsync(user, oldRoles);
            await _userManager.AddToRolesAsync(user, model.Roles);

            if (isLearner)
            {
                var learner = await _context.Learners.FirstOrDefaultAsync(l => l.Email == oldEmail)
                              ?? await _context.Learners.FirstOrDefaultAsync(l => l.Email == model.Email);

                if (learner != null)
                {
                    learner.Email = model.Email;
                    learner.FirstName = model.FirstName;
                    learner.LastName = model.LastName;
                    _context.Update(learner);
                }
                else
                {
                    _context.Learners.Add(new Learner
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        SignUpDate = DateTime.Now
                    });
                }
            }
            else if (wasLearner)
            {
                var learner = await _context.Learners.FirstOrDefaultAsync(l => l.Email == model.Email);
                if (learner != null) _context.Learners.Remove(learner);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return RedirectToAction(nameof(UsersDetails));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError("", $"Update failed: {ex.Message}");
            model.AllRoles = new List<string> { "Admin", "Editor", "Learner" };
            return View(model);
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var learner = await _context.Learners.FirstOrDefaultAsync(l => l.Email == user.Email);
            if (learner != null)
            {
                _context.Learners.Remove(learner);
                await _context.SaveChangesAsync(); 
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                await transaction.RollbackAsync();
                foreach (var error in result.Errors)
                {
                    TempData["ErrorMessage"] = error.Description;
                }
                return RedirectToAction(nameof(UsersDetails));
            }

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["ErrorMessage"] = $"Deletion failed: {ex.Message}";
            return RedirectToAction(nameof(UsersDetails));
        }

        TempData["SuccessMessage"] = "User and associated learner deleted successfully!";
        return RedirectToAction(nameof(UsersDetails));
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult CreateUser()
    {
        ViewBag.Roles = new List<string> { "Admin", "Editor", "Learner" };
        return View(new Register
        {
            Password = "123123", 
            ConfirmPassword = "123123"
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(Register model)
    {
        if (ModelState.IsValid)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
            {
                ModelState.AddModelError("Email", "Email is already registered");
                ViewBag.Roles = new List<string> { "Admin", "Editor", "Learner" };
                return View(model);
            }

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
                    var learner = new Learner
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        SignUpDate = DateTime.Now
                    };
                    _context.Learners.Add(learner);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"User {model.Email} created successfully!";
                return RedirectToAction(nameof(UsersDetails));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        ViewBag.Roles = new List<string> { "Admin", "Editor", "Learner" };
        return View(model);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ManageEnrollments()
    {
        var model = new EnrollmentView
        {
            AvailableCourses = await _context.Courses.ToListAsync(),
            AvailableLearners = await _context.Learners.ToListAsync(),
            CurrentEnrollments = await GetCurrentEnrollments()
        };

        return View(model);
    }
    [Authorize(Roles = "Learner")]
    [HttpPost]
    public async Task<IActionResult> RequestEnrollment(int courseId)
    {
        var user = await _userManager.GetUserAsync(User);
        var learner = await _context.Learners
            .FirstOrDefaultAsync(l => l.Email == user.Email);

        if (learner == null) return NotFound("Learner profile not found");

        var isEnrolled = await _context.Courses
            .Where(c => c.CourseCode == courseId)
            .SelectMany(c => c.Learners)
            .AnyAsync(l => l.LearnerId == learner.LearnerId);

        if (isEnrolled)
        {
            TempData["ErrorMessage"] = "You are already enrolled in this course";
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        var existingRequest = await _context.EnrollmentRequests
            .FirstOrDefaultAsync(r => r.CourseId == courseId && r.LearnerId == learner.LearnerId);

        if (existingRequest != null)
        {
            if (existingRequest.IsApproved)
            {
                TempData["ErrorMessage"] = "You are already enrolled in this course";
            }
            else if (existingRequest.IsRejected)
            {
                TempData["ErrorMessage"] = "Your previous request for this course was rejected";
            }
            else
            {
                TempData["ErrorMessage"] = "Request already submitted and pending approval";
            }
            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        var request = new EnrollmentRequest
        {
            CourseId = courseId,
            LearnerId = learner.LearnerId,
            RequestDate = DateTime.Now
        };

        _context.EnrollmentRequests.Add(request);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Enrollment request submitted successfully!";
        return RedirectToAction("Details", "Courses", new { id = courseId });
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> EnrollLearner(EnrollmentView model)
    {
        if (model.SelectedCourseId == null || model.SelectedLearnerId == null)
        {
            TempData["ErrorMessage"] = "Please select both a course and a learner";
            return RedirectToAction(nameof(ManageEnrollments));
        }

        try
        {
            var course = await _context.Courses
                .Include(c => c.Learners)
                .FirstOrDefaultAsync(c => c.CourseCode == model.SelectedCourseId);

            var learner = await _context.Learners
                .FirstOrDefaultAsync(l => l.LearnerId == model.SelectedLearnerId);

            if (course == null || learner == null)
            {
                TempData["ErrorMessage"] = "Course or learner not found";
                return RedirectToAction(nameof(ManageEnrollments));
            }

            course.Learners ??= new List<Learner>();

            if (course.Learners.Any(l => l.LearnerId == learner.LearnerId))
            {
                TempData["WarningMessage"] = "Learner is already enrolled in this course";
                return RedirectToAction(nameof(ManageEnrollments));
            }

            course.Learners.Add(learner);

            var request = new EnrollmentRequest
            {
                CourseId = course.CourseCode,
                LearnerId = learner.LearnerId,
                RequestDate = DateTime.Now,
                IsApproved = true,
                ProcessedDate = DateTime.Now
            };
            _context.EnrollmentRequests.Add(request);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Enrollment successful!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error enrolling learner: {ex.Message}";
        }

        return RedirectToAction(nameof(ManageEnrollments));
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RemoveEnrollment(int courseId, int learnerId)
    {
        try
        {
            var course = await _context.Courses
                .Include(c => c.Learners)
                .FirstOrDefaultAsync(c => c.CourseCode == courseId);

            if (course == null || course.Learners == null)
            {
                TempData["ErrorMessage"] = "Course not found or has no enrollments";
                return RedirectToAction(nameof(ManageEnrollments));
            }

            var learner = course.Learners.FirstOrDefault(l => l.LearnerId == learnerId);

            if (learner == null)
            {
                TempData["WarningMessage"] = "Learner not enrolled in this course";
                return RedirectToAction(nameof(ManageEnrollments));
            }

            course.Learners.Remove(learner);

            var enrollmentRequest = await _context.EnrollmentRequests
                .FirstOrDefaultAsync(r => r.CourseId == courseId && r.LearnerId == learnerId);

            if (enrollmentRequest != null)
            {
                enrollmentRequest.IsApproved = false;
                enrollmentRequest.ProcessedDate = DateTime.Now;
                _context.Update(enrollmentRequest);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Enrollment removed successfully!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error removing enrollment: {ex.Message}";
        }

        return RedirectToAction(nameof(ManageEnrollments));
    }
    [Authorize(Roles = "Learner")]
    [HttpPost]
    public async Task<IActionResult> Unenroll(int courseId)
    {
        var learner = await GetOrCreateLearnerAsync();

        try
        {
            var course = await _context.Courses
                .Include(c => c.Learners)
                .FirstOrDefaultAsync(c => c.CourseCode == courseId);

            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found";
                return RedirectToAction(nameof(MyCourses));
            }

            var learnerInCourse = course.Learners.FirstOrDefault(l => l.LearnerId == learner.LearnerId);
            if (learnerInCourse == null)
            {
                TempData["ErrorMessage"] = "You are not enrolled in this course";
                return RedirectToAction(nameof(MyCourses));
            }

            course.Learners.Remove(learnerInCourse);

            var enrollmentRequest = await _context.EnrollmentRequests
                .FirstOrDefaultAsync(r => r.CourseId == courseId && r.LearnerId == learner.LearnerId);

            if (enrollmentRequest != null)
            {
                enrollmentRequest.IsApproved = false;
                enrollmentRequest.ProcessedDate = null;
                _context.Update(enrollmentRequest);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Successfully unenrolled from the course";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error unenrolling: {ex.Message}";
        }

        return RedirectToAction(nameof(MyCourses));
    }
    private async Task<List<EnrollmentRecord>> GetCurrentEnrollments()
    {
        return await _context.Courses
            .Include(c => c.Learners)
            .SelectMany(c => c.Learners.Select(l => new EnrollmentRecord
            {
                CourseId = c.CourseCode,
                CourseTitle = c.Title,
                LearnerId = l.LearnerId,
                LearnerName = $"{l.FirstName} {l.LastName}"
            }))
            .ToListAsync();
    }

}