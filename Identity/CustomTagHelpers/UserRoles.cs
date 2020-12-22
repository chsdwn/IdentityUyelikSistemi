using System.Linq;
using System.Threading.Tasks;
using Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Identity.CustomTagHelpers
{
    [HtmlTargetElement("p", Attributes = "user-roles")]
    public class UserRoles : TagHelper
    {
        [HtmlAttributeName("user-roles")]
        public string UserId { get; set; }

        private readonly UserManager<AppUser> _userManager;

        public UserRoles(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            var roles = await _userManager.GetRolesAsync(user);

            var html = string.Empty;
            roles.ToList().ForEach(r => html += $"<span>{r}</span>");

            output.Content.SetHtmlContent(html);
        }
    }
}