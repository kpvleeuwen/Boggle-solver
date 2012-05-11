using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using PatienceSolverConsole;

namespace BrowserPatience
{
    public static class CardPainter
    {
        public static Typeface Cardfont { get; private set; }
        public static Pen Edge { get; private set; }
        static CardPainter()
        {
            Cardfont = SystemFonts.SmallCaptionFontFamily.GetTypefaces().First();
            Edge = new Pen(Brushes.Black, 2);
        }

        public static void DrawCard(this DrawingContext drawingContext, Card card, Rect r)
        {
             if (card.Visible)
                {
                    DrawCardOutline(drawingContext, r, Brushes.Beige);
                    drawingContext.DrawText(GetText(card), new Point(3, r.Top + 3)); 
                }
                else
                {
                    DrawCardOutline(drawingContext, r, Brushes.Blue); 
                }
        }

        public static Point Center(this Size size)
        {
            return new Point(size.Width / 2, size.Height / 2);
        }
        static void DrawCardOutline(DrawingContext drawingContext, Rect r, SolidColorBrush brush)
        {
            drawingContext.DrawRoundedRectangle(brush, Edge,
                r,
                5, 5);
        }

        static FormattedText GetText(Card card)
        {
            var text = new FormattedText(
                card.ToString(),
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                Cardfont,
                17,
                GetBrush(card));
            return text;
        }

        static Brush GetBrush(Card card)
        {
            switch (card.Color)
            {
                case CardColor.Black:
                    return Brushes.Black;
                case CardColor.Red:
                    return Brushes.Red;
            }
            // fallthrough (future joker?)
            throw new NotImplementedException();
        }

    }
}
