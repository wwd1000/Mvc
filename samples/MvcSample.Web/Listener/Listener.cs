using System;
using System.Collections.Generic;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Notify;

namespace MvcSample.Web
{
    public class Listener
    {
        private HttpContextAccessor _accessor = new HttpContextAccessor();

        [NotificationName("Microsoft.AspNet.Mvc.ActionStarting")]
        public void OnActionStarted(dynamic actionDescriptor, IDictionary<string, object> routeValues)
        {
            var store = GetEventStore();
            if (store == null)
            {
                return;
            }

            store.ActionName = actionDescriptor.Name;
            store.RouteValues = routeValues;
        }

        [NotificationName("Microsoft.AspNet.Mvc.BeforeAsyncAuthorizationFilter")]
        public void BeforeAsyncAuthorizationFilter(object filter)
        {
        }

        [NotificationName("Microsoft.AspNet.Mvc.AfterAsyncAuthorizationFilter")]
        public void AfterAsyncAuthorizationFilter(object filter, IActionResult result)
        {
            var store = GetEventStore();
            if (store == null)
            {
                return;
            }

            store.Filters.Add(new FilterResult()
            {
                FilterType = "Authorization",
                ShortCircuited = result == null,
                Type = filter.GetType().FullName,
            });
        }

        [NotificationName("Microsoft.AspNet.Mvc.BeforeAuthorizationFilter")]
        public void BeforeAuthorizationFilter(object filter)
        {
        }

        [NotificationName("Microsoft.AspNet.Mvc.AfterAuthorizationFilter")]
        public void AfterAuthorizationFilter(object filter, IActionResult result)
        {
            var store = GetEventStore();
            if (store == null)
            {
                return;
            }

            store.Filters.Add(new FilterResult()
            {
                FilterType = "Authorization",
                ShortCircuited = result == null,
                Type = filter.GetType().FullName,
            });
        }

        private EventStore GetEventStore()
        {
            return _accessor.HttpContext?.GetFeature<EventStore>();
        }
    }
}
