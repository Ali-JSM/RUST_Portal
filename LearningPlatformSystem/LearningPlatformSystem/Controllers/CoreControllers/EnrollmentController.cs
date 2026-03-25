using LearningPlatformSystem.Context;
using LearningPlatformSystem.Models.CoreModels;
using LearningPlatformSystem.Models.TableModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class EnrollmentController : Controller
{
    private readonly MyContext _context;
    private readonly UserManager<AuthUsers> _userManager;

    public EnrollmentController(MyContext context, UserManager<AuthUsers> userManager)
    {
        _context = context;
        _userManager = userManager;
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
                existingRequest.IsRejected = false;
                existingRequest.RejectionReason = null;
                existingRequest.ProcessedDate = null;
                existingRequest.RequestDate = DateTime.Now;
                _context.Update(existingRequest);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Enrollment request resubmitted successfully!";
                return RedirectToAction("Details", "Courses", new { id = courseId });
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
    public async Task<IActionResult> ApproveRequest(int requestId)
    {
        var request = await _context.EnrollmentRequests
            .Include(r => r.Course)
            .Include(r => r.Learner)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) return NotFound();

        request.Course.Learners ??= new List<Learner>();
        request.Course.Learners.Add(request.Learner);

        request.IsApproved = true;
        request.ProcessedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Enrollment request approved successfully!";
        return RedirectToAction("PendingRequests");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RejectRequest(int requestId, string? rejectionReason)
    {
        var request = await _context.EnrollmentRequests
            .Include(r => r.Course)
            .Include(r => r.Learner)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request == null) return NotFound();

        request.IsRejected = true;
        request.RejectionReason = rejectionReason;
        request.ProcessedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Enrollment request rejected successfully!";
        return RedirectToAction("PendingRequests");
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> PendingRequests()
    {
        var requests = await _context.EnrollmentRequests
            .Include(r => r.Course)
            .Include(r => r.Learner)
            .Where(r => !r.IsApproved && !r.IsRejected)
            .ToListAsync();

        return View(requests);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AllRequests()
    {
        var requests = await _context.EnrollmentRequests
            .Include(r => r.Course)
            .Include(r => r.Learner)
            .OrderByDescending(r => r.RequestDate)
            .ToListAsync();

        return View(requests);
    }
}