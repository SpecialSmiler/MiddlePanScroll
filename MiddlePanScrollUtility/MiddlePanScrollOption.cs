using MiddlePanScroll.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Documents;

namespace MiddlePanScroll.Options
{
    internal class MiddlePanScrollOption : BaseOptionModel<MiddlePanScrollOption>
    {
        [Category("Options about MiddlePanScroll")]
        [DisplayName("Scroll by pixels")]
        [Description("Set (true) to scroll by pixel, (false) to scroll by lines.")]
        [DefaultValue(false)]
        public bool IsScrollByPixel { get; set; } = false;

        [Category("Options about MiddlePanScroll")]
        [DisplayName("Disable horizontal")]
        [Description("Whether to disable horizontal scrolling.")]
        [DefaultValue(false)]
        public bool IsHorizontalDisable { get; set; } = false;

        [Category("Options about MiddlePanScroll")]
        [DisplayName("Sensitivity")]
        [Description("Sensitivity of scrolling.")]
        [DefaultValue(1)]
        public int Sensitivity { get; set; } = 1;

    }
}
