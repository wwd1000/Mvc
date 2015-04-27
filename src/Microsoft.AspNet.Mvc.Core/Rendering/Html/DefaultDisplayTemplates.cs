// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AspNet.HtmlContent;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.Rendering.Internal;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public static class DefaultDisplayTemplates
    {
        public static IHtmlContent BooleanTemplate(IHtmlHelper htmlHelper)
        {
            bool? value = null;
            if (htmlHelper.ViewData.Model != null)
            {
                value = Convert.ToBoolean(htmlHelper.ViewData.Model, CultureInfo.InvariantCulture);
            }

            return htmlHelper.ViewData.ModelMetadata.IsNullableValueType ?
                BooleanTemplateDropDownList(htmlHelper, value) :
                BooleanTemplateCheckbox(value ?? false, htmlHelper);
        }

        private static IHtmlContent BooleanTemplateCheckbox(bool value, IHtmlHelper htmlHelper)
        {
            var inputTag = new TagBuilder("input");
            inputTag.AddCssClass("check-box");
            inputTag.Attributes["disabled"] = "disabled";
            inputTag.Attributes["type"] = "checkbox";
            if (value)
            {
                inputTag.Attributes["checked"] = "checked";
            }

            inputTag.IsSelfClosing = true;

            return inputTag;
        }

        private static IHtmlContent BooleanTemplateDropDownList(IHtmlHelper htmlHelper, bool? value)
        {
            var selectTag = new TagBuilder("select");
            selectTag.AddCssClass("list-box");
            selectTag.AddCssClass("tri-state");
            selectTag.Attributes["disabled"] = "disabled";

            var innerContent = new BufferedHtmlContent();
            selectTag.InnerHtml = innerContent;

            foreach (var item in TriStateValues(value))
            {
                var encodedText = htmlHelper.Encode(item.Text);
                var option = DefaultHtmlGenerator.GenerateOption(item, encodedText);
                innerContent.Append(option);
            }

            return selectTag;
        }

        // Will soon need to be shared with the default editor templates implementations.
        internal static List<SelectListItem> TriStateValues(bool? value)
        {
            return new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = Resources.Common_TriState_NotSet,
                    Value = string.Empty,
                    Selected = !value.HasValue
                },
                new SelectListItem
                {
                    Text = Resources.Common_TriState_True,
                    Value = "true",
                    Selected = (value == true),
                },
                new SelectListItem
                {
                    Text = Resources.Common_TriState_False,
                    Value = "false",
                    Selected = (value == false),
                },
            };
        }

        public static IHtmlContent CollectionTemplate(IHtmlHelper htmlHelper)
        {
            var model = htmlHelper.ViewData.Model;
            if (model == null)
            {
                return StringHtmlContent.Empty;
            }

            var collection = model as IEnumerable;
            if (collection == null)
            {
                // Only way we could reach here is if user passed templateName: "Collection" to a Display() overload.
                throw new InvalidOperationException(Resources.FormatTemplates_TypeMustImplementIEnumerable(
                    "Collection", model.GetType().FullName, typeof(IEnumerable).FullName));
            }

            var typeInCollection = typeof(string);
            var genericEnumerableType = collection.GetType().ExtractGenericInterface(typeof(IEnumerable<>));
            if (genericEnumerableType != null)
            {
                typeInCollection = genericEnumerableType.GetGenericArguments()[0];
            }

            var typeInCollectionIsNullableValueType = typeInCollection.IsNullableValueType();

            var oldPrefix = htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix;

            try
            {
                htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix = string.Empty;

                var fieldNameBase = oldPrefix;
                var result = new BufferedHtmlContent();

                var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
                var metadataProvider = serviceProvider.GetRequiredService<IModelMetadataProvider>();
                var viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();

                var index = 0;
                foreach (var item in collection)
                {
                    var itemType = typeInCollection;
                    if (item != null && !typeInCollectionIsNullableValueType)
                    {
                        itemType = item.GetType();
                    }

                    var modelExplorer = metadataProvider.GetModelExplorerForType(itemType, item);
                    var fieldName = string.Format(CultureInfo.InvariantCulture, "{0}[{1}]", fieldNameBase, index++);

                    var templateBuilder = new TemplateBuilder(
                        viewEngine,
                        htmlHelper.ViewContext,
                        htmlHelper.ViewData,
                        modelExplorer,
                        htmlFieldName: fieldName,
                        templateName: null,
                        readOnly: true,
                        additionalViewData: null);

                    var output = templateBuilder.Build();
                    result.Append(output);
                }

                return result;
            }
            finally
            {
                htmlHelper.ViewData.TemplateInfo.HtmlFieldPrefix = oldPrefix;
            }
        }

        public static IHtmlContent DecimalTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == htmlHelper.ViewData.Model)
            {
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue =
                    string.Format(CultureInfo.CurrentCulture, "{0:0.00}", htmlHelper.ViewData.Model);
            }

            return StringTemplate(htmlHelper);
        }

        public static IHtmlContent EmailAddressTemplate(IHtmlHelper htmlHelper)
        {
            var uriString = "mailto:" + ((htmlHelper.ViewData.Model == null) ?
                string.Empty :
                htmlHelper.ViewData.Model.ToString());
            var linkedText = (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == null) ?
                string.Empty :
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString();

            return HyperlinkTemplate(uriString, linkedText, htmlHelper);
        }

        public static IHtmlContent HiddenInputTemplate(IHtmlHelper htmlHelper)
        {
            if (htmlHelper.ViewData.ModelMetadata.HideSurroundingHtml)
            {
                return StringHtmlContent.Empty;
            }

            return StringTemplate(htmlHelper);
        }

        public static IHtmlContent HtmlTemplate(IHtmlHelper htmlHelper)
        {
            return new StringHtmlContent(htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString());
        }

        public static IHtmlContent ObjectTemplate(IHtmlHelper htmlHelper)
        {
            var viewData = htmlHelper.ViewData;
            var templateInfo = viewData.TemplateInfo;
            var modelExplorer = viewData.ModelExplorer;

            if (modelExplorer.Model == null)
            {
                return new StringHtmlContent(modelExplorer.Metadata.NullDisplayText);
            }

            if (templateInfo.TemplateDepth > 1)
            {
                var text = modelExplorer.GetSimpleDisplayText();
                if (modelExplorer.Metadata.HtmlEncode)
                {
                    text = htmlHelper.Encode(text);
                }

                return new StringHtmlContent(text);
            }

            var serviceProvider = htmlHelper.ViewContext.HttpContext.RequestServices;
            var viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();

            var content = new BufferedHtmlContent();
            foreach (var propertyExplorer in modelExplorer.Properties)
            {
                var propertyMetadata = propertyExplorer.Metadata;
                if (!ShouldShow(propertyExplorer, templateInfo))
                {
                    continue;
                }

                TagBuilder containerDivTag = null;
                if (!propertyMetadata.HideSurroundingHtml)
                {
                    var label = propertyMetadata.GetDisplayName();
                    if (!string.IsNullOrEmpty(label))
                    {
                        var labelDivTag = new TagBuilder("div");
                        labelDivTag.SetInnerText(label);
                        labelDivTag.AddCssClass("display-label");
                        content.Append(labelDivTag);
                        content.Append(Environment.NewLine);
                    }

                    containerDivTag = new TagBuilder("div");
                    containerDivTag.AddCssClass("display-field");
                }

                var templateBuilder = new TemplateBuilder(
                    viewEngine,
                    htmlHelper.ViewContext,
                    htmlHelper.ViewData,
                    propertyExplorer,
                    htmlFieldName: propertyMetadata.PropertyName,
                    templateName: null,
                    readOnly: true,
                    additionalViewData: null);

                if (!propertyMetadata.HideSurroundingHtml)
                {
                    containerDivTag.InnerHtml = templateBuilder.Build();
                    content.Append(containerDivTag);
                    content.Append(Environment.NewLine);
                }
                else
                {
                    content.Append(templateBuilder.Build());
                }
            }

            return content;
        }

        private static bool ShouldShow(ModelExplorer modelExplorer, TemplateInfo templateInfo)
        {
            return
                modelExplorer.Metadata.ShowForDisplay &&
                !modelExplorer.Metadata.IsComplexType &&
                !templateInfo.Visited(modelExplorer);
        }

        public static IHtmlContent StringTemplate(IHtmlHelper htmlHelper)
        {
            return new StringHtmlContent(htmlHelper.Encode(htmlHelper.ViewData.TemplateInfo.FormattedModelValue));
        }

        public static IHtmlContent UrlTemplate(IHtmlHelper htmlHelper)
        {
            var uriString = (htmlHelper.ViewData.Model == null) ? string.Empty : htmlHelper.ViewData.Model.ToString();
            var linkedText = (htmlHelper.ViewData.TemplateInfo.FormattedModelValue == null) ?
                string.Empty :
                htmlHelper.ViewData.TemplateInfo.FormattedModelValue.ToString();

            return HyperlinkTemplate(uriString, linkedText, htmlHelper);
        }

        // Neither uriString nor linkedText need be encoded prior to calling this method.
        private static IHtmlContent HyperlinkTemplate(string uriString, string linkedText, IHtmlHelper htmlHelper)
        {
            var hyperlinkTag = new TagBuilder("a");
            hyperlinkTag.MergeAttribute("href", uriString);
            hyperlinkTag.SetInnerText(linkedText);

            return hyperlinkTag;
        }
    }
}
