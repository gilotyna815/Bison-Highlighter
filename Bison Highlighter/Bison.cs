using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bison_Highlighter
{
    internal sealed class Bison
    {
        internal const string ContentType = nameof(Bison);
        internal const string FileExtension = ".y";

        [Export]
        [Name(ContentType)]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition ContentTypeDefinition = null;

        [Export]
        [Name(ContentType + nameof(FileExtensionToContentTypeDefinition))]
        [ContentType(ContentType)]
        [FileExtension(FileExtension)]
        internal static FileExtensionToContentTypeDefinition FileExtensionToContentTypeDefinition = null;


    }
}
