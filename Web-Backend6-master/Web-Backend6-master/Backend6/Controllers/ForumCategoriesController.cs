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
    public class ForumCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserPermissionsServiceForForum userPermissions;


        public ForumCategoriesController(ApplicationDbContext context, IUserPermissionsServiceForForum userPermissions)
        {
            this.userPermissions = userPermissions;
            _context = context;
        }

        // Index and details are not needed


        // GET: ForumCategories/Create
        public IActionResult Create()
        {
            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.RedirectToAction("Index", "Forums");
            }
            return View();
        }

        // POST: ForumCategories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumCategoryViewModel model)
        {
            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.RedirectToAction("Index", "Forums");
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var category = new ForumCategory() { Name = model.Name };
            
            try
            {
                this._context.Add(category);
                await this._context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }

            return RedirectToAction("Index", "Forums");

        }

        // GET: ForumCategories/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.RedirectToAction("Index", "Forums");
            }

            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories.SingleOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return NotFound();
            }
            return View(new ForumCategoryViewModel() { Name = forumCategory.Name });
        }

        // POST: ForumCategories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ForumCategoryViewModel model)
        {
            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.RedirectToAction("Index", "Forums");
            }

            if (id == null)
            {
                return NotFound();
            }

            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var forumCategory = await this._context.ForumCategories.SingleOrDefaultAsync(fc => fc.Id == id);
            if (forumCategory == null)
            {
                return this.NotFound();
            }
            try
            {
                forumCategory.Name = model.Name;
                _context.Update(forumCategory);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                if (!ForumCategoryExists(forumCategory.Id))
                {
                    return NotFound();
                }
                else
                {
                    this.ModelState.AddModelError("", $"Unexpected error: { e.Message}");
                }
            }
            return RedirectToAction("Index", "Forums");

        }

        // GET: ForumCategories/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (!this.userPermissions.UserIsAdmin())
            {
                this.ModelState.AddModelError("", "You have no permissions to CRUD categories");
                return this.RedirectToAction("Index", "Forums");
            }
            if (id == null)
            {
                return NotFound();
            }

            var forumCategory = await _context.ForumCategories
                .SingleOrDefaultAsync(m => m.Id == id);
            if (forumCategory == null)
            {
                return NotFound();
            }

            return View(forumCategory);
        }

        // POST: ForumCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var forumCategory = await _context.ForumCategories.SingleOrDefaultAsync(m => m.Id == id);

            try
            {
                _context.ForumCategories.Remove(forumCategory);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                this.ModelState.AddModelError("", $"Unexpected error: {e.Message}");
            }
            return RedirectToAction("Index", "Forums");
        }

        private bool ForumCategoryExists(Guid id)
        {
            return _context.ForumCategories.Any(e => e.Id == id);
        }
    }
}
