using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Bison_Highlighter
{

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Bison Token")]
    [Name("Bison Token")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(After = Priority.Default, Before = Priority.High)] // Set the priority to be after the default classifiers
    internal sealed class BisonToken : ClassificationFormatDefinition
    {
        public BisonToken()
        {
            this.DisplayName = "Bison Token"; // Human readable version of the name
            this.ForegroundColor = Colors.HotPink;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "Block Name")]
    [Name("Block Name")]
    [UserVisible(true)] // This should be visible to the end user
    [Order(After = Priority.Default, Before = Priority.High)] // Set the priority to be after the default classifiers
    internal sealed class BlockName : ClassificationFormatDefinition
    {
        public BlockName()
        {
            this.DisplayName = "Block Name"; // Human readable version of the name
            this.ForegroundColor = Colors.LightSeaGreen;
        }
    }

}
