using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using System.Security.Claims;

namespace SignalRMVC
{
    public class BasicChatHub : Hub
    {
        private readonly UserManager<IdentityUser> _userManager;
        HttpContext? _httpContext;
        string _userId;

        public BasicChatHub(UserManager<IdentityUser> userManager)
        {
            if (_httpContext == null)
            {
                _httpContext = Context.GetHttpContext();
            }

            _userId = _httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _userManager = userManager;
        }
        public override async Task OnConnectedAsync()
        {

            var roles = await GetUserRoles(_userId);
            // Now you can use the userId as needed
            await base.OnConnectedAsync();
        }

        public string GetUserId()
        {
            return _userId;
        }
        public async Task<IdentityUser> GetUserAsync(string userId)
        {
            IdentityUser? user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                return user;
            }
            return null;
        }

        public async Task<IList<string>> GetUserRoles(string userId)
        {
            IdentityUser? user = await _userManager.FindByIdAsync(userId);
            IList<string> roles = await _userManager.GetRolesAsync(user);
            return roles;
        }

        public static List<string> GroupsJoined { get; set; } = new List<string>();

        [Authorize]
        public async Task JoinGroup(string senderId)
        {
         
            var role = (await GetUserRoles(_userId)).FirstOrDefault();
            if (!GroupsJoined.Contains(Context.ConnectionId + ":" + role))
            {
                GroupsJoined.Add(Context.ConnectionId + ":" + role);
                //do something else

                var sender = GetUserAsync(senderId);

                await Groups.AddToGroupAsync(Context.ConnectionId, role);
            }
        }

    }
}
