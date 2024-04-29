using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend6.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Backend6.Services
{
    public class UserPermissionsServiceForForum : IUserPermissionsServiceForForum
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;

        public UserPermissionsServiceForForum(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
            this.httpContextAccessor = httpContextAccessor;
        }

        private HttpContext HttpContext => this.httpContextAccessor.HttpContext;

        public Boolean CanCRUDForumTopic(ForumTopic topic)
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return true;
            }

            return this.userManager.GetUserId(this.httpContextAccessor.HttpContext.User) == topic.CreatorId;
        }

        public Boolean CanCRUDForumMessage(ForumMessage message)
        {
            if (!this.HttpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            if (this.HttpContext.User.IsInRole(ApplicationRoles.Administrators))
            {
                return true;
            }

            return this.userManager.GetUserId(this.httpContextAccessor.HttpContext.User) == message.CreatorId;
        }

        public async Task<String> GetCurrentUserId()
        {
           var user = await this.userManager.GetUserAsync(this.HttpContext.User);
           return user.Id;
        }

        public Boolean UserIsAdmin()
        {
            return this.HttpContext.User.IsInRole(ApplicationRoles.Administrators);
        }
    }
}
