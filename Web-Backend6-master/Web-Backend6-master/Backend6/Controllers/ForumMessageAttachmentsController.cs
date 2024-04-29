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
using Microsoft.AspNetCore.Hosting;
using Backend6.Models.ViewModels;
using System.IO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Net.Http.Headers;

namespace Backend6.Controllers
{
    [Authorize]
    public class ForumMessageAttachmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserPermissionsServiceForForum userPermissions;

        public static readonly HashSet<String>
    AllowedExtensions = new HashSet<String> { ".jpg", ".jpeg", ".png", ".gif" };

        private readonly IHostingEnvironment hostingEnvironment;

        public ForumMessageAttachmentsController(ApplicationDbContext context, IUserPermissionsServiceForForum userPermissions, IHostingEnvironment hostingEnvironment)
        {
            this.hostingEnvironment = hostingEnvironment;
            this.userPermissions = userPermissions;
            _context = context;
        }

        // GET: ForumMessageAttachments/Create
        public async Task<IActionResult> Create(Guid messageId)
        {
            this.ViewBag.messageId = messageId;

            var message = await this._context.ForumMessages
                .SingleOrDefaultAsync(m => m.Id == messageId);
            if (!this.userPermissions.CanCRUDForumMessage(message))
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD this topic");
                return this.RedirectToAction("Details", "ForumsTopics", new { id = message.ForumTopicId });
            }

            this.ViewBag.topicId = message.ForumTopicId;

            return View();
        }

        // POST: ForumMessageAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid messageId, PostAttachmentEditModel model)
        {
            this.ViewBag.messageId = messageId;

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var message = await this._context.ForumMessages
    .SingleOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
            {
                return NotFound();
            }
            this.ViewBag.Post = message;
            if (!this.userPermissions.CanCRUDForumMessage(message))
            {
                this.ModelState.AddModelError("", "You have no permissions to attach files");
                return this.RedirectToAction("Details", "ForumsTopics", new { id = message.ForumTopicId });
            }
            this.ViewBag.topicId = message.ForumTopicId;
            var fileName = Path.GetFileName(
                ContentDispositionHeaderValue.Parse(model.File.ContentDisposition).FileName.Value.Trim('"'));
            var fileExt = Path.GetExtension(fileName);

            if (!AllowedExtensions.Contains(fileExt))
            {
                this.ModelState.AddModelError(nameof(model.File), "This file type is prohibited");
                return this.View(model);
            }

            var postAttachment = new PostAttachment
            {
                Created = DateTime.UtcNow,
            };
            ForumMessageAttachment attachment = new ForumMessageAttachment();
            attachment.Created = DateTime.UtcNow;
            Guid Id = attachment.Id;
            var attachmentPath = Path.Combine(this.hostingEnvironment.WebRootPath, "attachments",
                Id.ToString("N") + fileExt);
            postAttachment.Path = $"/attachments/{Id:N}{fileExt}";
            using (var fileStream = new FileStream(attachmentPath, FileMode.CreateNew, FileAccess.ReadWrite,
                FileShare.Read))
            {
                await model.File.CopyToAsync(fileStream);
            }


            attachment.FileName = fileName;
            attachment.FilePath = postAttachment.Path;
            attachment.ForumMessageId = messageId;

            try
            {
                this._context.Add(attachment);
                await this._context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }

            return this.RedirectToAction("Details", "ForumTopics", new { id = message.ForumTopicId });
        }


        // GET: ForumMessageAttachments/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forumMessageAttachment = await _context.ForumMessageAttachments.Include(f => f.ForumMessage).SingleOrDefaultAsync(m => m.Id == id);
            if (forumMessageAttachment == null)
            {
                return NotFound();
            }
            if (!this.userPermissions.CanCRUDForumMessage(forumMessageAttachment.ForumMessage))
            {
                this.ModelState.AddModelError("", "You have no permissions to delete files");
                return this.RedirectToAction("Details", "ForumsTopics", new { id = forumMessageAttachment.ForumMessage.ForumTopicId });
            }
            this.ViewBag.topicId = forumMessageAttachment.ForumMessage.ForumTopicId;
            return this.View(forumMessageAttachment);

        }

        // POST: ForumMessageAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var forumMessageAttachment = await _context.ForumMessageAttachments.Include(f => f.ForumMessage).SingleOrDefaultAsync(m => m.Id == id);

            if (forumMessageAttachment == null)
            {
                return NotFound();
            }
            if (!this.userPermissions.CanCRUDForumMessage(forumMessageAttachment.ForumMessage))
            {
                this.ModelState.AddModelError("", "You have no permissions to delete files");
                return this.RedirectToAction("Details", "ForumsTopics", new { id = forumMessageAttachment.ForumMessage.ForumTopicId });
            }

            try
            {
                this._context.ForumMessageAttachments.Remove(forumMessageAttachment);
                await this._context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }

            return this.RedirectToAction("Details", "ForumTopics", new { id = forumMessageAttachment.ForumMessage.ForumTopicId });
        }

        private bool ForumMessageAttachmentExists(Guid id)
        {
            return _context.ForumMessageAttachments.Any(e => e.Id == id);
        }
    }
}
