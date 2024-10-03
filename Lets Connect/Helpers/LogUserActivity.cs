﻿using Lets_Connect.Extensions;
using Lets_Connect.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lets_Connect.Helpers
{
    public class LogUserActivity : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var resultContext = await next();
            if(context.HttpContext.User.Identity?.IsAuthenticated != true) return;

            var userId = resultContext.HttpContext.User.GetUserId();
            var repo = resultContext.HttpContext.RequestServices.GetRequiredService<IUserRepository>();
            var user = await repo.GetUSerByIdAsync(userId);
            if(user == null) return;
            user.LastActive = DateTime.UtcNow;
            await repo.SaveAllAsync();
        }
    }
}
