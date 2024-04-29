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
using Microsoft.AspNetCore.Identity;
using Backend6.Services;
using Backend6.Models.ViewModels;

namespace Backend6.Controllers
{
    [Authorize]
    public class ForumsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IUserPermissionsServiceForForum userPermissions;

        public ForumsController(ApplicationDbContext context, IUserPermissionsServiceForForum userPermissions)
        {
            _context = context;
            this.userPermissions = userPermissions;
        }

        // GET: Forums
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                return this.View(
    await this._context.ForumCategories.Include(fg => fg.Forums).ThenInclude(f => f.ForumTopics).ToListAsync()
);
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
                return this.View();
            }

        }

        [AllowAnonymous]
        // GET: Forums/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var forum = await _context.Forums
                .Include(f => f.ForumTopics).ThenInclude(ft => ft.Creator)
                .Include(f => f.ForumTopics).ThenInclude(ft => ft.ForumMessages).ThenInclude(fm => fm.Creator)
                .SingleOrDefaultAsync(m => m.Id == id);

            if (forum == null)
            {
                return NotFound();
            }

            return View(forum);
        }

        // GET: Forums/Create
        public IActionResult Create(Guid categoryId)
        {
            if (categoryId == null)
            {
                return NotFound();
            }

            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.View("Index");
            }

            this.ViewBag.categoryId = categoryId;

            return View();
        }

        // POST: Forums/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Guid categoryId, ForumViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            if (categoryId == null)
            {
                return NotFound();
            }

            var forum = new Forum() { Name = model.Description, Description = model.Description, ForumCategoryId = categoryId };

            try
            {
                this._context.Add(forum);
                await this._context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Forums/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.View("Index");
            }

            var forum = await _context.Forums.SingleOrDefaultAsync(m => m.Id == id);
            if (forum == null)
            {
                return NotFound();
            }

            ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name", forum.ForumCategoryId);

            return View( new ForumViewModel() { Name = forum.Name, Description = forum.Description, ForumCategoryId = forum.ForumCategoryId });
        }

        // POST: Forums/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ForumViewModel model)
        {
            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.View("Index");
            }

            if (id == null)
            {
                return NotFound();
            }

            ViewData["ForumCategoryId"] = new SelectList(_context.ForumCategories, "Id", "Name", model.ForumCategoryId);

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var forum = await this._context.Forums.SingleOrDefaultAsync(f => f.Id == id);
            if (forum == null)
            {
                return this.NotFound();
            }
            try
            {
                forum.Name = model.Name;
                forum.ForumCategoryId = model.ForumCategoryId;
                forum.Description = model.Description;
                _context.Update(forum);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (!ForumExists(forum.Id))
                {
                    return NotFound();
                }
                else
                {
                    this.ModelState.AddModelError("", $"Unexpected error: { e.Message}");
                }
            }
            return this.RedirectToAction(nameof(Index));

        }

        // GET: Forums/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.View("Index");
            }

            var forum = await _context.Forums
                .Include(f => f.ForumCategory)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forum == null)
            {
                return NotFound();
            }

            return View(forum);
        }

        // POST: Forums/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.View("Index");
            }

            var forum = await _context.Forums.SingleOrDefaultAsync(m => m.Id == id);
            try
            {
                _context.Forums.Remove(forum);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ForumExists(Guid id)
        {
            return _context.Forums.Any(e => e.Id == id);
        }
    }
}
