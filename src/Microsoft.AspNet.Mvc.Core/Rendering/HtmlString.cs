// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.HtmlContent;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlString : StringHtmlContent
    {
        public static new readonly HtmlString Empty = new HtmlString(string.Empty);

        public HtmlString(string input)
            : base(input, encodeOnWrite: false)
        {
        }
    }
}
