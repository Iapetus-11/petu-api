﻿using api.iapetus11.me.Models;
using api.iapetus11.me.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.iapetus11.me.Controllers;

[ApiController]
[Route("github/stats")]
public class GitHubStatsController : ControllerBase
{
    private readonly IGitHubService _gitHub;

    public GitHubStatsController(IGitHubService gitHub)
    {
        _gitHub = gitHub;
    }

    [HttpGet("{userName}")]
    public async Task<IActionResult> GetGitHubUserStats(string userName)
    {
        if (!_gitHub.IsValidUserName(userName))
        {
            return BadRequest("Invalid GitHub username provided.");
        }

        return Ok(new
        {
            EarnedStars = await _gitHub.GetUserEarnedStars(userName),
            MergedPullRequests = await _gitHub.GetUserMergedPullRequests(userName),
            OpenedIssues = await _gitHub.GetUserOpenedIssues(userName),
            Dependants = await _gitHub.GetUserDependantRepositoriesCount(userName),
        });
    }

    [HttpGet("{userName}/shield/stars"), ResponseCache(Duration = 1800)]
    public async Task<IActionResult> GetGitHubUserStarCard(string userName, [FromQuery] ShieldQueryParams shieldParams)
    {
        if (!_gitHub.IsValidUserName(userName))
        {
            return BadRequest("Invalid GitHub username provided.");
        }

        return Content(await _gitHub.GetUserEarnedStarsShieldSvg(userName, shieldParams), "image/svg+xml");
    }

    [HttpGet("{userName}/shield/prs"), ResponseCache(Duration = 1800)]
    public async Task<IActionResult> GetGitHubUserPRsCard(string userName, [FromQuery] ShieldQueryParams shieldParams)
    {
        if (!_gitHub.IsValidUserName(userName))
        {
            return BadRequest("Invalid GitHub username provided.");
        }

        return Content(await _gitHub.GetUserMergedPullRequestsShieldSvg(userName, shieldParams), "image/svg+xml");
    }

    [HttpGet("{userName}/shield/issues"), ResponseCache(Duration = 1800)]
    public async Task<IActionResult> GetGitHubUserIssuesCard(string userName,
        [FromQuery] ShieldQueryParams shieldParams)
    {
        if (!_gitHub.IsValidUserName(userName))
        {
            return BadRequest("Invalid GitHub username provided.");
        }

        return Content(await _gitHub.GetUserOpenedIssuesShieldSvg(userName, shieldParams), "image/svg+xml");
    }

    [HttpGet("{userName}/shield/dependants"), ResponseCache(Duration = 1800)]
    public async Task<IActionResult> GetGitHubUserDependantRepositoriesCard(string userName,
        [FromQuery] ShieldQueryParams shieldParams)
    {
        if (!_gitHub.IsValidUserName(userName))
        {
            return BadRequest("Invalid GitHub username provided.");
        }

        return Content(await _gitHub.GetUserDependantRepositoriesShieldSvg(userName, shieldParams), "image/svg+xml");
    }
}
