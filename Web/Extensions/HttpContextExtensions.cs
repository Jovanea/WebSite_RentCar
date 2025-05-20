using System;
using System.Web;
using eUseControl.Domain.Entities.User;

namespace Web.Extensions
{
    public static class HttpContextExtensions
    {
        public static void SetMySessionObject(this HttpContext context, ULoginData userData)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            
            if (userData != null)
            {
                context.Session["UserData"] = userData;
            }
        }
    }
} 