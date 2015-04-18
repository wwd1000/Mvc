using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcSample.Web
{
    public interface IActionDescriptor
    {
        string DisplayName { get; }

        string Name { get; }

        string Id { get; }
    }
}
