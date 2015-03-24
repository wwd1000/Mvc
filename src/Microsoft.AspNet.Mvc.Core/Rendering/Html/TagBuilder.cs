// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.AspNet.HtmlContent;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class TagBuilder : IHtmlContent
    {
        public TagBuilder(string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(tagName));
            }

            TagName = tagName;
            Attributes = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public IDictionary<string, string> Attributes { get; }

        public IHtmlContent InnerHtml { get; set; }

        public string TagName { get; }

        public bool IsSelfClosing { get; set; }

        public void AddCssClass(string value)
        {
            string currentValue;

            if (Attributes.TryGetValue("class", out currentValue))
            {
                Attributes["class"] = value + " " + currentValue;
            }
            else
            {
                Attributes["class"] = value;
            }
        }

        /// <summary>
        /// Return valid HTML 4.01 "id" attribute for an element with the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The original element name.</param>
        /// <param name="invalidCharReplacement">
        /// The <see cref="string"/> (normally a single <see cref="char"/>) to substitute for invalid characters in
        /// <paramref name="name"/>.
        /// </param>
        /// <returns>
        /// Valid HTML 4.01 "id" attribute for an element with the given <paramref name="name"/>.
        /// </returns>
        /// <remarks>Valid "id" attributes are defined in http://www.w3.org/TR/html401/types.html#type-id</remarks>
        public static string CreateSanitizedId(string name, [NotNull] string invalidCharReplacement)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var firstChar = name[0];
            if (!Html401IdUtil.IsAsciiLetter(firstChar))
            {
                // The first character must be a letter according to the HTML 4.01 specification.
                firstChar = 'z';
            }

            var stringBuffer = new StringBuilder(name.Length);
            stringBuffer.Append(firstChar);
            for (var index = 1; index < name.Length; index++)
            {
                var thisChar = name[index];
                if (Html401IdUtil.IsValidIdCharacter(thisChar))
                {
                    stringBuffer.Append(thisChar);
                }
                else
                {
                    stringBuffer.Append(invalidCharReplacement);
                }
            }

            return stringBuffer.ToString();
        }

        public void GenerateId(string name, [NotNull] string idAttributeDotReplacement)
        {
            if (!Attributes.ContainsKey("id"))
            {
                var sanitizedId = CreateSanitizedId(name, idAttributeDotReplacement);
                if (!string.IsNullOrEmpty(sanitizedId))
                {
                    Attributes["id"] = sanitizedId;
                }
            }
        }

        private void AppendAttributes(TextWriter writer, IHtmlEncoder encoder)
        {
            foreach (var attribute in Attributes)
            {
                var key = attribute.Key;
                if (string.Equals(key, "id", StringComparison.OrdinalIgnoreCase) &&
                    string.IsNullOrEmpty(attribute.Value))
                {
                    continue;
                }

                writer.Write(' ');
                writer.Write(key);
                writer.Write("=\"");
                encoder.HtmlEncode(attribute.Value, writer);
                writer.Write('"');
            }
        }

        public void MergeAttribute(string key, string value)
        {
            MergeAttribute(key, value, replaceExisting: false);
        }

        public void MergeAttribute(string key, string value, bool replaceExisting)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(key));
            }

            if (replaceExisting || !Attributes.ContainsKey(key))
            {
                Attributes[key] = value;
            }
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes)
        {
            MergeAttributes(attributes, replaceExisting: false);
        }

        public void MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting)
        {
            if (attributes != null)
            {
                foreach (var entry in attributes)
                {
                    var key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);
                    var value = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);
                    MergeAttribute(key, value, replaceExisting);
                }
            }
        }

        public void SetInnerText(string innerText)
        {
            InnerHtml = StringHtmlContent.FromEncodedText(innerText);
        }

        public HtmlString ToHtmlString(TagRenderMode renderMode)
        {
            return new HtmlString(ToString(renderMode));
        }

        public override string ToString()
        {
            return ToString(TagRenderMode.Normal);
        }

        public string ToString(TagRenderMode renderMode)
        {
            using (var stringWriter = new StringWriter())
            {
                WriteTo(stringWriter, new HtmlEncoder(), renderMode);
                return stringWriter.ToString();
            }
        }

        public void WriteTo(TextWriter writer, IHtmlEncoder encoder)
        {
            var renderMode = IsSelfClosing ? TagRenderMode.SelfClosing : TagRenderMode.Normal;
            WriteTo(writer, encoder, renderMode);
        }

        public void WriteTo(TextWriter writer, IHtmlEncoder encoder, TagRenderMode renderMode)
        {
            switch (renderMode)
            {
                case TagRenderMode.StartTag:
                    writer.Write('<');
                    writer.Write(TagName);
                    AppendAttributes(writer, encoder);
                    writer.Write('>');
                    break;
                case TagRenderMode.EndTag:
                    writer.Write("</");
                    writer.Write(TagName);
                    writer.Write('>');
                    break;
                case TagRenderMode.SelfClosing:
                    writer.Write('<');
                    writer.Write(TagName);
                    AppendAttributes(writer, encoder);
                    writer.Write(" />");
                    break;
                default:
                    writer.Write('<');
                    writer.Write(TagName);
                    AppendAttributes(writer, encoder);
                    writer.Write('>');
                    if (InnerHtml != null)
                    {
                        InnerHtml.WriteTo(writer, encoder);
                    }
                    writer.Write("</");
                    writer.Write(TagName);
                    writer.Write('>');
                    break;
            }
        }

        private static class Html401IdUtil
        {
            public static bool IsAsciiLetter(char testChar)
            {
                return (('A' <= testChar && testChar <= 'Z') || ('a' <= testChar && testChar <= 'z'));
            }

            public static bool IsValidIdCharacter(char testChar)
            {
                return (IsAsciiLetter(testChar) || IsAsciiDigit(testChar) || IsAllowableSpecialCharacter(testChar));
            }

            private static bool IsAsciiDigit(char testChar)
            {
                return ('0' <= testChar && testChar <= '9');
            }

            private static bool IsAllowableSpecialCharacter(char testChar)
            {
                switch (testChar)
                {
                    case '-':
                    case '_':
                    case ':':
                        // Note '.' is valid according to the HTML 4.01 specification. Disallowed here to avoid
                        // confusion with CSS class selectors or when using jQuery.
                        return true;

                    default:
                        return false;
                }
            }
        }
    }
}
