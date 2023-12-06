using Horizon.Database.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public string[] requiredRoles;

    public AuthorizeAttribute(string role = null)
    {
        requiredRoles = role?.Split(',') ?? new string[0];
    }
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
        {
            var user = (UserDTO)context.HttpContext.Items["User"];
            if (user == null)
            {
                // not logged in
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
            else if (requiredRoles != null && !user.Roles.Any(r => requiredRoles.Contains(r)))
            {
                //User does not have the required role for this request
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}
