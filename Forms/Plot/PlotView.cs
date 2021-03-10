using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using static i_ConVerificationSystem.Structs.VerificationResult;

namespace i_ConVerificationSystem.Forms.Plot
{
    class PlotView : Control
    {
        //総幅員
        public string TotalWidth;
        //始点、終点、路線名、幅員(文字)
        public List<Tuple<Point, Point, string, string, VerifyResultType>> Points;
        //幅員と路線名用の描画高さ
        private double namePHeight 
        {
            get
            {
                if (Points == null || Points.Count() == 0) return 0;
                var p1Max = Points.Select(row => row.Item1).Max(mRow => mRow.Y);
                var p2Max = Points.Select(row => row.Item2).Max(mRow => mRow.Y);
                var retPHeight = Math.Max(p1Max, p2Max) + 20;
                return retPHeight;
            }
        }
        //原点からの左端
        private double EndOfLeftX
        {
            get
            {
                if (Points == null || Points.Count() == 0) return 0;
                var p1Min = Points.Select(row => row.Item1).Min(mRow => mRow.X);
                var p2Min = Points.Select(row => row.Item2).Min(mRow => mRow.X);
                var retEOLX = Math.Min(p1Min, p2Min);
                return retEOLX;
            }
        }
        //原点からの右端
        private double EndOfRightX
        {
            get
            {
                if (Points == null || Points.Count() == 0) return 0;
                var p1Max = Points.Select(row => row.Item1).Max(mRow => mRow.X);
                var p2Max = Points.Select(row => row.Item2).Max(mRow => mRow.X);
                var retEORX = Math.Max(p1Max, p2Max);
                return retEORX;
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (Points == null || Points.Count() == 0)
            {
                return;
            }

            var pen = new Pen(Brushes.Black, 2);
            var brush = Brushes.Black;

            bool poppingSwitch = false;
            foreach (var point in Points)
            {
                if (point.Item3 != null)
                {
                    DrawWidthLines(point, drawingContext, poppingSwitch);
                    poppingSwitch = !poppingSwitch;
                }
                drawingContext.DrawLine(pen, point.Item1, point.Item2);
                drawingContext.DrawEllipse(brush, null, point.Item1, 5, 5);
                drawingContext.DrawEllipse(brush, null, point.Item2, 5, 5);
            }

            //総幅員の描画
            var dispTotal = new FormattedText(TotalWidth,
                                              CultureInfo.CurrentCulture,
                                              FlowDirection.LeftToRight,
                                              new Typeface("Verdana"),
                                              10,
                                              Brushes.Black,
                                              new NumberSubstitution(),
                                              1);
            var rt = new ScaleTransform();
            rt.ScaleY = -1;
            rt.CenterX = 0;
            rt.CenterY = -namePHeight + 20;
            drawingContext.PushTransform(rt);
            drawingContext.DrawText(dispTotal, new Point(0, -namePHeight + 20));
            drawingContext.Pop();

            var redPen = new Pen(Brushes.Red, 1);
            //赤線の描画座標
            var p1 = new Point(EndOfLeftX, -namePHeight);
            var p2 = new Point(EndOfRightX, -namePHeight);
            //線の描画
            drawingContext.DrawLine(redPen, new Point(EndOfLeftX, 0), p1);
            drawingContext.DrawLine(redPen, new Point(EndOfRightX, 0), p2);
            drawingContext.DrawLine(redPen, p1, p2);

            //p1始点に対してp2終点に向ける、p2終点に対してp1始点に向ける
            var ap1_1 = new Point(p1.X + (p2.X <= p1.X ? -3 : 3), p1.Y + 3);
            var ap1_2 = new Point(p1.X + (p2.X <= p1.X ? -3 : 3), p1.Y - 3);
            var ap2_1 = new Point(p2.X + (p1.X <= p2.X ? -3 : 3), p2.Y + 3);
            var ap2_2 = new Point(p2.X + (p1.X <= p2.X ? -3 : 3), p2.Y - 3);

            drawingContext.DrawLine(redPen, p1, ap1_1);
            drawingContext.DrawLine(redPen, p1, ap1_2);
            drawingContext.DrawLine(redPen, p2, ap2_1);
            drawingContext.DrawLine(redPen, p2, ap2_2);

        }

        private void DrawWidthLines(Tuple<Point, Point, string, string, VerifyResultType> point, DrawingContext drawingContext, bool poppingSwitch)
        {
            var pen = new Pen(Brushes.Red, 1);
            var arrowPen = new Pen(Brushes.Red, 1);
            arrowPen.StartLineCap = PenLineCap.Triangle;
            arrowPen.EndLineCap = PenLineCap.Triangle;
            var dispName = new FormattedText(point.Item3, 
                                             CultureInfo.CurrentCulture, 
                                             FlowDirection.LeftToRight, 
                                             new Typeface("Verdana"), 
                                             10, 
                                             Brushes.Black, 
                                             new NumberSubstitution(),
                                             1);
            var dispWidth = new FormattedText(point.Item4,
                                              CultureInfo.CurrentCulture,
                                              FlowDirection.LeftToRight,
                                              new Typeface("Verdana"),
                                              10,
                                              Brushes.Black,
                                              new NumberSubstitution(),
                                              1);

            if (point.Item5 == VerifyResultType.NG)
            {
                dispName.SetForegroundBrush(Brushes.Red);
            }

            //赤線の描画座標
            var p1 = new Point(point.Item1.X, namePHeight);
            var p2 = new Point(point.Item2.X, namePHeight);
            //だいたいセンター
            var namePcenterX = (p1.X + p2.X) / 2 - 20;
            var widthPcenterX = (p1.X + p2.X) / 2 - 10;
            //描画座標
            var widthP = new Point(widthPcenterX, namePHeight - 5);
            //文字被りを避けるため高さを交互に変える
            var nameP = new Point(namePcenterX, namePHeight + (poppingSwitch ? 20 : 30));

            //線の描画
            drawingContext.DrawLine(pen, point.Item1, p1);
            drawingContext.DrawLine(pen, point.Item2, p2);
            drawingContext.DrawLine(arrowPen, p1, p2);

            //なぜか矢印にならないので直線で矢印線を再現
            //p1始点に対してp2終点に向ける、p2終点に対してp1始点に向ける
            var ap1_1 = new Point(p1.X + (p2.X <= p1.X ? -3 : 3), p1.Y + 3);
            var ap1_2 = new Point(p1.X + (p2.X <= p1.X ? -3 : 3), p1.Y - 3);
            var ap2_1 = new Point(p2.X + (p1.X <= p2.X ? -3 : 3), p2.Y + 3);
            var ap2_2 = new Point(p2.X + (p1.X <= p2.X ? -3 : 3), p2.Y - 3);

            drawingContext.DrawLine(arrowPen, p1, ap1_1);
            drawingContext.DrawLine(arrowPen, p1, ap1_2);
            drawingContext.DrawLine(arrowPen, p2, ap2_1);
            drawingContext.DrawLine(arrowPen, p2, ap2_2);

            //文字の描画（上下反転しているため文字だけ元に戻す）
            var rt = new ScaleTransform();
            rt.ScaleY = -1;
            rt.CenterX = widthP.X;
            rt.CenterY = widthP.Y;
            drawingContext.PushTransform(rt);
            drawingContext.DrawText(dispWidth, widthP);
            drawingContext.Pop();

            rt = new ScaleTransform();
            rt.ScaleY = -1;
            rt.CenterX = nameP.X;
            rt.CenterY = nameP.Y;
            drawingContext.PushTransform(rt);
            drawingContext.DrawText(dispName, nameP);
            drawingContext.Pop();
        }
    }
}
