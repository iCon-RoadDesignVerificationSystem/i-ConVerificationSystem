using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static i_ConVerificationSystem.Structs.OGExtensions;

namespace i_ConVerificationSystem.Forms.Plot
{
    partial class WCPicture : UserControl
    {
        public WCPicture()
        {
            InitializeComponent();
        }

        public void DrawWCImage(CrossSect_OGExtension cs)
        {
            var g = this.pictureBox1.CreateGraphics();
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));

            var odcss = cs.dcssList.OrderBy(T => T);
            int startPointX = 20;
            foreach (var dcss in odcss)
            {
                startPointX += DrawRectangleAndText(dcss, startPointX);
            }
        }

        int DrawRectangleAndText(DesignCrossSectSurf_OGExtension dcss, int startPointX)
        {
            var objPen = new Pen(System.Drawing.Color.Black, 2);
            var objPenWhite = new Pen(System.Drawing.Color.White, 2);
            var objFont = new Font("ＭＳ　Ｐゴシック", 15);
            var sf = new StringFormat();
            sf.FormatFlags = StringFormatFlags.DirectionVertical | StringFormatFlags.NoWrap | StringFormatFlags.NoClip;
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            sf.Trimming = StringTrimming.None;
            var objGrp = this.pictureBox1.CreateGraphics();

            decimal cspWidth = this.GetWidth(dcss);
            int rectWidth = (int)cspWidth * 15;

            Rectangle rect = new Rectangle(startPointX, 20, rectWidth, 100);
            Rectangle widthRect = new Rectangle(startPointX, 130, rectWidth, 100);

            objGrp.DrawRectangle(objPen, startPointX, 20, rectWidth, 100);
            objGrp.DrawString(dcss.name, objFont, Brushes.Black, rect, sf);

            objGrp.DrawString(cspWidth.ToString(), objFont, Brushes.Black, widthRect, sf);

            return rectWidth;
        }

        /// <summary>
        /// 幅員を取得
        /// </summary>
        /// <param name="dcss"></param>
        /// <returns></returns>
        private decimal GetWidth(DesignCrossSectSurf_OGExtension dcss)
        {
            return ((from T in dcss.cspList orderby T.roadWidth descending select T.roadWidth).First() -
                    (from T in dcss.cspList orderby T.roadWidth ascending select T.roadWidth).First());
        }

    }
}
