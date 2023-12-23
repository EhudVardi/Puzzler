using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentation.WPF
{
    public static class ConvertWinfromsObjects
    {
        public static System.Windows.Media.Color ConvertColor(System.Drawing.Color c)
        {
            System.Windows.Media.Color c2 = new System.Windows.Media.Color();
            c2 = System.Windows.Media.Color.FromArgb(c.A, c.R, c.G, c.B);
            return c2;
        }
        public static System.Windows.Media.Pen ConvertPen(System.Drawing.Pen c)
        {
            return new System.Windows.Media.Pen(ConvertBrush(c.Brush as System.Drawing.SolidBrush), 1);
        }
        public static System.Windows.Media.Brush ConvertBrush(System.Drawing.SolidBrush c)
        {
            return new System.Windows.Media.SolidColorBrush(ConvertColor(c.Color));
        }
        public static System.Windows.Point ConvertPoint(System.Drawing.PointF p)
        {
            return new System.Windows.Point(p.X, p.Y);
        }

        public static System.Windows.Forms.Keys ConvertKeys(System.Windows.Input.Key key)
        {
            System.Windows.Forms.Keys formsKey = System.Windows.Forms.Keys.None;
            switch (key)
            {
                case System.Windows.Input.Key.D0: formsKey = System.Windows.Forms.Keys.D0; break;
                case System.Windows.Input.Key.D1: formsKey = System.Windows.Forms.Keys.D1; break;
                case System.Windows.Input.Key.D2: formsKey = System.Windows.Forms.Keys.D2; break;
                case System.Windows.Input.Key.D3: formsKey = System.Windows.Forms.Keys.D3; break;
                case System.Windows.Input.Key.D4: formsKey = System.Windows.Forms.Keys.D4; break;
                case System.Windows.Input.Key.D5: formsKey = System.Windows.Forms.Keys.D5; break;
                case System.Windows.Input.Key.D6: formsKey = System.Windows.Forms.Keys.D6; break;
                case System.Windows.Input.Key.D7: formsKey = System.Windows.Forms.Keys.D7; break;
                case System.Windows.Input.Key.D8: formsKey = System.Windows.Forms.Keys.D8; break;
                case System.Windows.Input.Key.D9: formsKey = System.Windows.Forms.Keys.D9; break;
                default: break;
            }
            return formsKey;
        }
    }
}
