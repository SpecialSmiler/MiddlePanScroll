namespace MiddlePanScroll
{
    using System.ComponentModel.Composition;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Utilities;

    [Export(typeof(IMouseProcessorProvider))]
    [Name("MiddlePanScroll")]
    [Order(Before = "UrlClickMouseProcessor")]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal class MiddlePanScrollProvider : IMouseProcessorProvider
    {
        public IMouseProcessor GetAssociatedProcessor(IWpfTextView textView)
        {
            //if (_telemetrySession == null)
            //{
            //    _telemetrySession = TelemetrySessionForPPT.Create(this.GetType().Assembly);
            //}

            return MiddlePanScroll.Create(textView);
        }
    }
}
