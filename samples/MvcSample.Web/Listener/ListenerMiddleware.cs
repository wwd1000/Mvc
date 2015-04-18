
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Extensions;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Notify;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MvcSample.Web
{
    public class ListenerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly INotifier _notifier;
        private readonly object _listener;

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
        {
            ContractResolver = new DeclaredOnlyContractResolver(),
        };

        private readonly ConcurrentDictionary<string, EventStore> _requestStore = new ConcurrentDictionary<string, EventStore>(StringComparer.Ordinal);

        public ListenerMiddleware(RequestDelegate next, INotifier notifier)
        {
            _next = next;
            _notifier = notifier;

            _listener = new Listener();
            _notifier.EnlistTarget(_listener);
        }

        public async Task Invoke(HttpContext context)
        {
            PathString remaining;
            if (context.Request.Path.StartsWithSegments(new PathString("/GetData"), out remaining))
            {
                var trackingId = remaining.Value.Substring(1);

                EventStore eventStore;
                if (_requestStore.TryRemove(trackingId, out eventStore))
                {
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(eventStore, _settings));
                    return;
                }
                else
                {
                    context.Response.StatusCode = 404;
                    return;
                }
            }
            else
            {
                var trackingId = context.Request.Query.GetValues("trackingId")?.FirstOrDefault();
                if (trackingId != null)
                {
                    var eventStore = new EventStore();
                    context.SetFeature(eventStore);

                    _requestStore.TryAdd(trackingId, eventStore);
                }

                await _next(context);
            }
        }
    }

    public class DeclaredOnlyContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization);
        }
    }
}
