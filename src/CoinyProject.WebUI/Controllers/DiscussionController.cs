using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Application.DTO.Discussion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoinyProject.WebUI.Controllers
{
    [Authorize]
    public class DiscussionController : Controller
    {
        private readonly IDiscussionService _discussionService;
        public DiscussionController(IDiscussionService discussionService) 
        {
            _discussionService = discussionService;
        }

        public async Task<IActionResult> Index()
        {
            var discussions = await _discussionService.GetAllDiscussionsForView();
            return View(discussions);
        }

        public async Task<IActionResult> Create()
        {
            var discussion = new DiscussionCreateDTO
            {
                AvailableTopics = await _discussionService.GetAvailableTopics()
            };

            return View(discussion);
        }

        [HttpPost]
        public async Task<IActionResult> Create(DiscussionCreateDTO discussion)
        {
            await _discussionService.AddDiscussion(discussion, User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            TempData["success"] = "Discussion successfully created";
            return RedirectToAction("Index", "Discussion");
            
        }
    }
}
