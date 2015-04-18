using System;
using System.Collections.Generic;

namespace MvcSample.Web
{
    public class EventStore
    {
        //public IActionDescriptor Action { get; set; }

        public string ActionName { get; set; }

        public List<FilterResult> Filters { get; } = new List<FilterResult>();

        public IDictionary<string, object> RouteValues { get; set; }
    }

    public class FilterResult
    {
        public string Type { get; set; }

        public string FilterType { get; set; }

        public bool ShortCircuited { get; set; }
    }
}