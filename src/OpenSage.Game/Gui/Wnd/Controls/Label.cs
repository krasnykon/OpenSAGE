using OpenSage.Mathematics;
using OpenSage.Data.Wnd;
using System;

namespace OpenSage.Gui.Wnd.Controls
{
    public class Label : Control
    {
        public TextAlignment TextAlignment { get; set; } = TextAlignment.Center;

        public Label(WndWindowDefinition wndWindow)
        {
            TextAlignment = wndWindow.StaticTextData.Centered
                ? TextAlignment.Center
                : TextAlignment.Leading;
        }

        public int ImplicitHeight()
        {
            SizeF descSize = DrawingContext2D.MeasureText(Text, Font, TextAlignment, Width);
            return (int)MathF.Ceiling(descSize.Height);
        }

        protected override void DrawOverride(DrawingContext2D drawingContext)
        {
            DrawText(drawingContext, TextAlignment);
        }
    }
}
