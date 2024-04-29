using System;
using System.Threading.Tasks;
using Backend6.Models;

namespace Backend6.Services
{
    public interface IUserPermissionsServiceForForum
    {
        Boolean CanCRUDForumTopic(ForumTopic topic);

        Boolean CanCRUDForumMessage(ForumMessage message);

        Task<String> GetCurrentUserId();
        Boolean UserIsAdmin();

    }
}