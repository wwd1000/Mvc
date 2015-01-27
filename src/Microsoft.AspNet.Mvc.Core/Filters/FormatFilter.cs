// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc.Description;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.OptionsModel;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Mvc
{
    /// <summary>
    /// A filter which will use the format value in the route data or query string to set the content type on an 
    /// <see cref="ObjectResult" /> returned from an action.
    /// </summary>
    public class FormatFilter : IFormatFilter, IResourceFilter, IResultFilter
    {

        private MvcOptions _mvcOptions;

        /// <summary>
        /// Initializes an instance of <see cref="FormatFilter"/>.
        /// </summary>
        /// <param name="options"><see cref="MvcOptions"/>.</param>
        public FormatFilter(MvcOptions options)
        {
            _mvcOptions = options;
            Format = null;
            ContentType = null;
            IsActive = false;
        }

        /// <summary>
        /// format value in the current request. <c>null</c> if format not present in the current request.
        /// </summary>
        public string Format { get; private set; }

        /// <summary>
        /// <see cref="MediaTypeHeaderValue"/> for the format value in the current request.
        /// </summary>
        public MediaTypeHeaderValue ContentType { get; private set; }

        /// <summary>
        /// <c>true</c> if the current <see cref="FormatFilter"/> is active and will execute. 
        /// </summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// As a <see cref="IResourceFilter"/>, this filter looks at the request and rejects it before going ahead if
        /// 1. The format in the request doesnt match any format in the map.
        /// 2. If there is a conflicting producesFilter.
        /// </summary>
        /// <param name="context">The <see cref="ResourceExecutingContext"/>.</param>
        public void OnResourceExecuting([NotNull] ResourceExecutingContext context)
        {
            Format = GetFormat(context);

            if (string.IsNullOrEmpty(Format))
            {
                return; // no format specified by user, so the filter is muted
            }

            ContentType = GetContentType(Format, context);

            if (ContentType == null)
            {
                // no contentType exists for the format, return 404
                context.Result = new HttpNotFoundResult();
            }
            else
            {
                var responseTypeFilters = context.Filters.OfType<IApiResponseMetadataProvider>();
                var contentTypes = new List<MediaTypeHeaderValue>();

                foreach (var filter in responseTypeFilters)
                {
                    filter.SetContentTypes(contentTypes);
                }

                if (contentTypes.Count != 0)
                {
                    // There is no IApiResponseMetadataProvider to generate the content type user asked for. We have to
                    // exit here with not found result. 
                    if (!contentTypes.Any(c => ContentType.IsSubsetOf(c)))
                    {
                        context.Result = new HttpNotFoundResult();
                    }
                    else
                    {
                        IsActive = true;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void OnResourceExecuted([NotNull] ResourceExecutedContext context)
        {

        }

        /// <summary>
        /// Sets a Content Type on an  <see cref="ObjectResult" />  using a format value from the request.
        /// </summary>
        /// <param name="context">The <see cref="ResultExecutingContext"/>.</param>
        public void OnResultExecuting([NotNull] ResultExecutingContext context)
        {
            if (!IsActive)
            {
                return; // no format specified by user, so the filter is muted
            }

            var objectResult = context.Result as ObjectResult;
            if (objectResult != null)
            {
                objectResult.ContentTypes.Clear();
                objectResult.ContentTypes.Add(ContentType);
            }
        }

        /// <inheritdoc />
        public void OnResultExecuted([NotNull] ResultExecutedContext context)
        {

        }

        ///// <summary>
        ///// Returns <c>true</c> if the filter is active and will execute; otherwise, <c>false</c>. The filter is active
        /////  if the current request contains format value. 
        ///// </summary>
        ///// <param name="context">The <see cref="FilterContext"/></param>
        ///// <returns><c>true</c> if the current request contains format value; otherwise, <c>false</c>.</returns>
        //public bool IsActive(FilterContext context)
        //{
        //    var format = GetFormat(context);

        //    return !string.IsNullOrEmpty(format);
        //}

        private string GetFormat(FilterContext context)
        {
            object format = null;

            if (!context.RouteData.Values.TryGetValue("format", out format))
            {
                format = context.HttpContext.Request.Query["format"];
            }

            if (format != null)
            {
                return format.ToString();
            }

            return null;
        }

        private MediaTypeHeaderValue GetContentType(string format, FilterContext context)
        {
            var options = context.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>();
            var contentType = options.Options.FormatterMappings.GetMediaTypeMappingForFormat(format);

            return contentType;
        }
    }
}