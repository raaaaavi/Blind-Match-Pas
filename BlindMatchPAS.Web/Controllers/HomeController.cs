using BlindMatchPAS.Web.Data;
using BlindMatchPAS.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BlindMatchPAS.Web.Controllers;

public class HomeController(ApplicationDbContext context) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new LandingPageViewModel
        {
            StudentCount = await context.Users.CountAsync(x => x.StudentProfile != null),
            SupervisorCount = await context.Users.CountAsync(x => x.SupervisorProfile != null),
            ProposalCount = await context.Proposals.CountAsync(),
            MatchCount = await context.Matches.CountAsync(),
            ResearchAreas = await context.ResearchAreas
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Take(6)
                .Select(x => new ResearchAreaOptionViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    AccentColor = x.AccentColor
                })
                .ToListAsync()
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult About() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
