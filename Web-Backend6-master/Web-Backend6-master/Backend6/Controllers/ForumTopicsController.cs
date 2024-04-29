using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Backend6.Data;
using Backend6.Models;
using Microsoft.AspNetCore.Authorization;
using Backend6.Services;
using Backend6.Models.ViewModels;

namespace Backend6.Controllers
{
    [Authorize]
    public class ForumTopicsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserPermissionsServiceForForum userPermissions;

        public ForumTopicsController(ApplicationDbContext context, IUserPermissionsServiceForForum userPermissions)
        {
            _context = context;
            this.userPermissions = userPermissions;
        }


        // GET: ForumTopics/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumTopic = await _context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .Include(f => f.ForumMessages).ThenInclude(fm => fm.Creator)
                .Include(f => f.ForumMessages).ThenInclude(fm => fm.ForumMessageAttachments)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumTopic == null)
            {
                return NotFound();
            }

            return View(forumTopic);
        }

        // GET: ForumTopics/Create
        public IActionResult Create(Guid forumId)
        {

            ViewBag.forumId = forumId;
            return View();
        }

        // POST: ForumTopics/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid forumId, ForumTopicViewModel model)
        {
            if (forumId == null)
            {
                return NotFound();
            }
            ViewBag.forumId = forumId;

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }


            var topic = new ForumTopic() { Name = model.Name, ForumId = forumId, CreatorId = await this.userPermissions.GetCurrentUserId(), Created = DateTime.UtcNow };

            try
            {
                this._context.Add(topic);
                await this._context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }
            return this.RedirectToAction("Details", "Forums", new { id = forumId });
        }

        // GET: ForumTopics/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.ForumTopics.SingleOrDefaultAsync(m => m.Id == id);
            if (topic == null)
            {
                return NotFound();
            }

            ViewBag.forumId = topic.ForumId;

            if (!this.userPermissions.CanCRUDForumTopic(topic))
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD this topic");
                return this.RedirectToAction("Details", "Forums", new { id = topic.ForumId });
            }

            return View(new ForumTopicViewModel() { Name = topic.Name });
        }

        // POST: ForumTopics/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ForumTopicViewModel model)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var topic = await this._context.ForumTopics.SingleOrDefaultAsync(ft => ft.Id == id);

            try
            {
                topic.Name = model.Name;
                _context.Update(topic);
                await _context.SaveChangesAsync();
            }
                catch (Exception e)
                {
                    if (!ForumTopicExists(topic.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
                    }
            }
            this.ViewBag.forumId = topic.ForumId;
            return RedirectToAction("Details", "Forums", new { id = topic.ForumId });

        }

        // GET: ForumTopics/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var topic = await _context.ForumTopics
                .Include(f => f.Creator)
                .Include(f => f.Forum)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (!this.userPermissions.CanCRUDForumTopic(topic))
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD this topic");
                return this.RedirectToAction("Details", "Forums", new { id = topic.ForumId });
            }
            if (topic == null)
            {
                return NotFound();
            }
            this.ViewBag.forumId = topic.ForumId;

            return View(topic);
        }

        // POST: ForumTopics/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var topic = await _context.ForumTopics.SingleOrDefaultAsync(m => m.Id == id);

            if (!this.userPermissions.CanCRUDForumTopic(topic))
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD this topic");
                return this.RedirectToAction("Details", "Forums", new { id = topic.ForumId });
            }
            if (topic == null)
            {
                return NotFound();
            }

            this.ViewBag.forumId = topic.ForumId;
            try
            {
                _context.ForumTopics.Remove(topic);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }
            return this.RedirectToAction("Details", "Forums", new { id = topic.ForumId });
        }

        private bool ForumTopicExists(Guid id)
        {
            return _context.ForumTopics.Any(e => e.Id == id);
        }
    }
}
