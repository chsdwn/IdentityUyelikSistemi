using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Identity
{
    public class Requirements
    {
        public class ExchangeExpireDateRequirement : IAuthorizationRequirement
        {

        }

        public class ExchangeExpireDateHandler : AuthorizationHandler<ExchangeExpireDateRequirement>
        {
            protected override Task HandleRequirementAsync(
                AuthorizationHandlerContext context,
                ExchangeExpireDateRequirement requirement)
            {
                if (context.User != null && context.User.Identity != null)
                {
                    var claim = context.User.Claims
                        .Where(c => c.Type == "ExchangeExpireDate" && c.Value != null)
                        .FirstOrDefault();

                    if (claim != null)
                    {
                        if (DateTime.UtcNow < Convert.ToDateTime(claim.Value))
                            context.Succeed(requirement);
                        else
                            context.Fail();
                    }
                }

                return Task.CompletedTask;
            }
        }
    }
}