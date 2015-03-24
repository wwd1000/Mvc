
using System.IO;
using Microsoft.AspNet.HtmlContent;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class StringCollectionHtmlContent : IHtmlContent
    {
        internal BufferEntryCollection Buffer { get; } = new BufferEntryCollection();

        public void WriteTo(TextWriter writer, IHtmlEncoder encoder)
        {
            var stringCollectionWriter = writer as StringCollectionTextWriter;
            if (stringCollectionWriter != null)
            {
                stringCollectionWriter.Buffer.Add(Buffer);
                return;
            }

            foreach (var entry in Buffer)
            {
                writer.Write(entry);
            }
        }
    }
}