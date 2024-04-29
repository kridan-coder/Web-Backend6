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
    public class ForumMessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserPermissionsServiceForForum userPermissions;


        public ForumMessagesController(ApplicationDbContext context, IUserPermissionsServiceForForum userPermissions)
        {
            this.userPermissions = userPermissions;
            _context = context;
        }

        // no need for details and index

        // GET: ForumMessages/Create
        public IActionResult Create(Guid topicId)
        {
            this.ViewBag.topicId = topicId;
            return View();
        }

        // POST: ForumMessages/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid topicId, ForumMessageViewModel model)
        {
            this.ViewBag.topicId = topicId;
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var message = new ForumMessage() { CreatorId = await userPermissions.GetCurrentUserId(), Created = DateTime.UtcNow, Text = model.Text, ForumTopicId = topicId };

            try
            {
                this._context.Add(message);
                await this._context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }
            return this.RedirectToAction("Details", "ForumTopics", new { id = topicId });
        }

        // GET: ForumMessages/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var forumMessage = await _context.ForumMessages.SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null)
            {
                return NotFound();
            }
            this.ViewBag.topicId = forumMessage.ForumTopicId;
            return View(new ForumMessageViewModel()
            {
                Text = forumMessage.Text,
            });
        }

        // POST: ForumMessages/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid? id, ForumMessageViewModel model)
        {
            if (id == null)
            {
                return NotFound();
            }


            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var message = await this._context.ForumMessages.SingleOrDefaultAsync(fm => fm.Id == id);
            if (message == null)
            {
                return this.NotFound();
            }
            if (!this.userPermissions.CanCRUDForumMessage(message))
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD this topic");
                return this.RedirectToAction("Details", "ForumTopics", new { id = message.ForumTopicId });
            }
            try
            {
                message.Text = model.Text;
                message.Modified = DateTime.UtcNow;
                _context.Update(message);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (!ForumMessageExists(message.Id))
                {
                    return NotFound();
                }
                else
                {
                    this.ModelState.AddModelError("", $"Unexpected error: { e.Message}");
                }
            }
            return this.RedirectToAction("Details", "ForumTopics", new { id = message.ForumTopicId });
        }

        // GET: ForumMessages/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessage = await _context.ForumMessages
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessage == null)
            {
                return NotFound();
            }
            if (!this.userPermissions.CanCRUDForumMessage(forumMessage))
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD this topic");
                return this.RedirectToAction("Details", "ForumTopics", new { id = forumMessage.ForumTopicId });
            }
            this.ViewBag.topicId = forumMessage.ForumTopicId;
            return View(forumMessage);
        }

        // POST: ForumMessages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var forumMessage = await _context.ForumMessages.SingleOrDefaultAsync(m => m.Id == id);
            try
            {
                _context.ForumMessages.Remove(forumMessage);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }
            return this.RedirectToAction("Details", "ForumTopics",
                new { id = forumMessage.ForumTopicId });
        }

        private bool ForumMessageExists(Guid id)
        {
            return _context.ForumMessages.Any(e => e.Id == id);
        }
    }
}
