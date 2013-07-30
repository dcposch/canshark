using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using SSCP.Telem;

namespace SSCP.Telem.CanShark {
    public class IntegralEventArgs : EventArgs
    {
        public Dictionary<string, double> Integrals { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
    public enum GraphType { Scatter, Timeseries };
    public partial class GraphPanel : UserControl
    {
        public GraphPanel()
        {
            InitializeComponent();
            ScatterX = ScatterY = new TimeSeries();
            base.AutoScroll = true;
            this.DoubleBuffered = true;

            Update();
        }
        public override bool AutoScroll
        {
            get
            {
                return base.AutoScroll;
            }
            set
            {
                if(value != base.AutoScroll)
                    throw new ReadOnlyException("GraphPanel.AutoScroll is read-only");
            }
        }

        GraphType type;
        public GraphType Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                Update();
            }
        }
        DateTime start;
        public DateTime Start
        {
            get
            {
                return start;
            }
            set
            {
                int scrollMax = (int)((End.Ticks - value.Ticks - interval.Ticks) / 10000L);
                if (scrollMax > 0)
                    HorizontalScroll.Maximum = scrollMax;
                int scrollVal = HorizontalScroll.Value + (int)((start.Ticks - value.Ticks) / 10000L);
                if (scrollVal > HorizontalScroll.Maximum)
                    scrollVal = HorizontalScroll.Maximum;
                if (scrollVal < 0)
                    scrollVal = 0;
                HorizontalScroll.Value = scrollVal;
                start = value;
            }
        }
        DateTime end;
        public DateTime End
        {
            get
            {
                return end;
            }
            set
            {
                int scrollMax = (int)((value.Ticks - Start.Ticks - interval.Ticks) / 10000L);
                if (scrollMax > 0)
                    HorizontalScroll.Maximum = scrollMax;
                end = value;
            }
        }
        TimeSpan interval;
        public TimeSpan Interval
        {
            get
            {
                return interval;
            }
            set
            {
                interval = value;
                int scrollMax = (int)((End.Ticks - Start.Ticks - interval.Ticks) / 10000L);
                if (scrollMax > 0)
                    HorizontalScroll.Maximum = scrollMax;
            }
        }
        private HashSet<TimeSeries> series = new HashSet<TimeSeries>();
        public HashSet<TimeSeries> Series
        {
            get { return series; }
        }
        public bool ScrollToLatest { get; set; }
        public TimeSeries ScatterX { get; set; }
        public TimeSeries ScatterY { get; set; }
        /* world-coord dimensions */
        float xmin, xmax, ymin, ymax;
        /* virtual width and height (possibly different from screen dims if behind scrollbars) */
        float vwidth, vheight;
        /* mouseover support */
        PointF mouse; bool hover;
        //Rectangle cursorDims = new Rectangle(-20, -20, 200, 200);
        /* drag support */
        Rectangle drag; Point dragStart;
        bool dragging;
        Dictionary<string, double> integrals = new Dictionary<string, double>();
        public event EventHandler<IntegralEventArgs> IntegralDragged;
    
        new void Update()
        {
            if (type == GraphType.Timeseries)
            {
                this.VScroll = false;
                int scrollMax = (int)((End.Ticks - Start.Ticks)*Width / Interval.Ticks);
                if (ScrollToLatest && AutoScrollMinSize != null && AutoScrollMinSize.Width != scrollMax)
                {
                    int x = scrollMax - Width;
                    if(x < 0) x = 0;
                    AutoScrollPosition = new Point(x, AutoScrollPosition.Y);
                }
                AutoScrollMinSize = new Size(scrollMax, 0);
                vwidth = scrollMax;
                //if (vwidth < Width)
                //    vwidth = Width;
                vheight = Height;
            }
            if (type == GraphType.Scatter)
            {
                AutoScrollMinSize = new Size(0, 0); //for now, no scrolling scatterplots
                vwidth = Width;
                vheight = Height;
            }
            //pnlGraph.HorizontalScroll.Enabled = true;
            //pnlGraph.HorizontalScroll.Visible = true;
            //firstTicks = series.StartTime.Ticks;
            //lastTicks = series.EndTime.Ticks;
            //pnlGraph.HorizontalScroll.Minimum = 0;
            //pnlGraph.HorizontalScroll.Maximum = (int)((lastTicks - firstTicks) / 10000L);
            //long uiInterval = getTsInterval();
            //if (interval != uiInterval)
            //{
            //    long windowEndTicks = lastTicks;
            //    if (lastTicks > firstTicks)
            //        windowEndTicks = (long)pnlGraph.HorizontalScroll.Value + firstTicks + interval;
            //    interval = uiInterval;
            //    if (lastTicks - firstTicks > 0 && (10000L * interval < (lastTicks - firstTicks)))
            //    {
            //        pbxGraph.Width = (int)(10000L * pnlGraph.Width * interval / (lastTicks - firstTicks));
            //        int scrollVal = (int)((windowEndTicks - firstTicks) / 10000L - interval);
            //        if (scrollVal < 0)
            //            scrollVal = 0;
            //        pnlGraph.HorizontalScroll.Value = scrollVal;
            //    }
            //}
        }

        private void normalize(ref float min, ref float max)
        {
            bool invalid =
                float.IsInfinity(min) ||
                float.IsNaN(min) ||
                float.IsInfinity(max) ||
                float.IsNaN(max);
            if (invalid || (min + float.Epsilon >= max))
            {
                min = -1.0f;
                max = 1.0f;
            }
            //leave some space at top and bottom
            float newmax = min + (max - min) * 1.1f;
            min = min - (max - min) * 0.1f;
            max = newmax;
        }
        private void drawAxes(bool yAxis, Graphics g)
        {
            //dims, in screen coords, of the axes and gridlines we're drawing
            int boxWidth = Width - AutoScrollPosition.X;
            int boxHeight = Height - AutoScrollPosition.Y;

            //axes
            if (yAxis)
            {
                float x = worldToScreen(new PointF(0.0f, 0.0f)).X;
                g.DrawLine(
                    new Pen(Color.Gray, 2.0f),
                    x, 0,
                    boxHeight, 0); //y axis
            }
            else
            {
                float y = worldToScreen(new PointF(0.0f, 0.0f)).Y;
                g.DrawLine(
                    new Pen(Color.Gray, 2.0f),
                    0, y,
                    boxWidth, y); //x axis
            }
            //draw up to 10 power-of-ten lines
            float min = yAxis ? ymin : xmin;
            float max = yAxis ? ymax : xmax;
            float scale = 1.0f;
            while ((max - min) / scale > 10.0)
                scale *= 10.0f;
            while ((max - min) / scale <= 2.0)
                scale /= 10.0f;
            //yeah, yeah, numerically instable, i know. it's only ~10 points.
            for (
                float line = (float)Math.Floor(min / scale) * scale;
                line < (Math.Ceiling(max / scale) + 1.0) * scale;
                line += scale)
            {
                if (yAxis)
                {
                    float y = worldToScreen(new PointF(0.0f, (float)line)).Y;
                    g.DrawLine(
                        new Pen(Color.Gray, 1.0f),
                        0, y,
                        boxWidth, y);
                    float fontSize = 10.0f;
                    g.DrawString(string.Format("{0:0.0}", line),
                        new Font("Lucida Console", fontSize), Brushes.Gray,
                        -AutoScrollPosition.X, y + 1);
                }
                else
                {
                    float x = worldToScreen(new PointF((float)line, 0.0f)).X;
                    g.DrawLine(
                        new Pen(Color.Gray, 1.0f),
                        x, 0,
                        x, boxHeight);
                    float fontSize = 10.0f;
                    g.DrawString(string.Format("{0:0.0}", line),
                        new Font("Lucida Console", fontSize), Brushes.Gray,
                        x + 1, boxHeight - 1.2f * fontSize);
                }
            }
            //draw time axis
            //for (long ticks = (plotTimes[0].Ticks / tickInterval.Ticks) * tickInterval.Ticks;
            //    ticks < (plotTimes[plotTimes.Count - 1].Ticks / tickInterval.Ticks + 1) * tickInterval.Ticks;
            //    ticks += tickInterval.Ticks*10)
            //{
            //    float x = (float)((plotTimes[plotTimes.Count - 1].Ticks - ticks) * pbxGraph.Width / (plotTimes[plotTimes.Count - 1].Ticks - plotTimes[0].Ticks));
            //    g.DrawLine(
            //        new Pen(Color.Gray, 1.0f),
            //        x, 0,
            //        x, pbxGraph.Height);
            //    g.DrawString((new DateTime(ticks)).ToShortTimeString(),
            //        new Font("Lucida Console", 10.0f), Brushes.Gray,
            //        x, pbxGraph.Height - 0.0f);
            //}
        }
        private void drawCursor(Graphics g, string label){
            //draw crosshair
            Pen pen = new Pen(Color.White, 2.0f);
            float cursorSize = 8;
            float x = mouse.X - AutoScrollPosition.X;
            float y = mouse.Y;
            g.DrawLine(pen, x, y - cursorSize, x, y + cursorSize);
            g.DrawLine(pen, x - cursorSize, y, x + cursorSize, y);
            pen = Pens.White;
            g.DrawLine(pen, x, 0, x, Height - AutoScrollPosition.Y);
            g.DrawLine(pen, 0, y, Width - AutoScrollPosition.X, y);

            //draw label
            x += 3;
            y += 3;
            Font font = new Font("Verdana", 10f);
            g.DrawString(label, font, Brushes.Tomato, x, y);
        }

        private PointF worldToScreen(PointF pWorld)
        {
            /* virtual dimensions (which may be larger than actual dimensions, with a scroll bar) */
            PointF pScreen;
            pScreen = new PointF(
                   vwidth * (pWorld.X - xmin) / (xmax - xmin),
                   vheight * (ymax - pWorld.Y) / (ymax - ymin));
            return pScreen;
        }
        protected PointF screenToWorld(PointF pScreen)
        {
            PointF pWorld = new PointF(
                xmin + pScreen.X * (xmax - xmin) / vwidth,
                ymax - pScreen.Y * (ymax - ymin) / vheight);
            return pWorld;
        }
        protected DateTime screenXToTime(float x)
        {
            long ticks = Start.Ticks + (long)(x - AutoScrollPosition.X) * Interval.Ticks / Width;
            return new DateTime(ticks);
        }

        void invalidateCursor(PointF pt)
        {
            //Rectangle invalid = new Rectangle(cursorDims.Location, cursorDims.Size);
            //invalid.Offset((int)pt.X, (int)pt.Y);
            //Invalidate(invalid);
            Invalidate();
        }

        void drawTimeseries(Graphics g, Rectangle clip)
        {
            //scale
            g.TranslateTransform(AutoScrollPosition.X, 0);
            xmin = 0.0f;
            xmax = (End.Ticks - Start.Ticks) / 10000f;
            DateTime clipStart = Start + new TimeSpan((clip.Left - AutoScrollPosition.X) * Interval.Ticks / Width);
            DateTime clipEnd = Start + new TimeSpan((clip.Right - AutoScrollPosition.X) * Interval.Ticks / Width);

            //y bounds
            ymin = 10;
            ymax = -10;
            Dictionary<string, double> varMults = new Dictionary<string, double>();
            foreach (TimeSeries ts in Series)
            {
                double mult = 1000000;
                foreach (DataPoint pt in ts.points)
                {
                    while (Math.Abs(pt.Value * mult) > 10.0)
                        mult /= 10;
                }
                varMults.Add(ts.name, mult);
            }
            Dictionary<string, List<PointF>> varPoints = new Dictionary<string, List<PointF>>();
            foreach (TimeSeries ts in Series)
            {
                double mult = varMults[ts.name];
                List<PointF> points = new List<PointF>();
                varPoints.Add(ts.name, points);
                int startIx = ts.BSearch(clipStart);
                if (startIx == -1)
                    startIx = 0;
                int endIx = ts.BSearch(clipEnd) + 1;
                /* get one point past the end so that when we scroll around, we don't leave
                 * single-segment gaps between the redraw regions */
                if (endIx < ts.points.Count - 1)
                    endIx++;
                for (int i = startIx; i < endIx; i++)
                {
                    PointF point = new PointF(
                        (float)((ts.points[i].Timestamp.Ticks - Start.Ticks) / 10000f) - xmin,
                        (float)(ts.points[i].Value * mult));
                    points.Add(point);
                }
                for (int i = 0; i < ts.points.Count; i++)
                {
                    float y = (float)(ts.points[i].Value * mult);
                    if (y < ymin) ymin = y;
                    if (y > ymax) ymax = y;
                }
                /*}
                else
                {
                    for (int i = 0; i < nPlotTimes; i++)
                    {
                        float point = series.ComputeTimeWeightedAverage(var,
                             new DateTime(wLastTicks - (nPlotTimes - i) * tickInterval.Ticks),
                             new DateTime(wLastTicks - (nPlotTimes - i - 1) * tickInterval.Ticks));
                        points.Add(new PointF((float)(i * tickInterval.Ticks), (float)point));
                        if (point < min) min = point;
                        if (point > max) max = point;
                    }
                }*/
            }

            normalize(ref ymin, ref ymax);
            drawAxes(true, g);
            if (hover)
            {
                DateTime cursorTime = screenXToTime(mouse.X);
                string label = cursorTime.ToString("yyyy.MM.dd HH:mm:ss") + "\n";
                foreach (TimeSeries ts in series)
                {
                    int ix = ts.BSearch(cursorTime);
                    if (dragging)
                        label += string.Format("{0} {1:0.000} ({2:0.000})\n", ts.name, ts.points[ix].Value, integrals[ts.name]);
                    else
                        label += string.Format("{0} {1:0.000}\n", ts.name, ts.points[ix].Value);
                }
                if (dragging)
                {
                    label += string.Format("integrating {0:0.00} secs", (float)drag.Width / 1000f);
                }
                drawCursor(g, label);
            }

            //TODO: separate integral calculation from display?
            integrals.Clear();
            foreach (string var in varPoints.Keys)
            {
                integrals.Add(var, 0);
                List<PointF> points = varPoints[var];
                Color col = Color.Yellow;
                Pen pen = new Pen(col);
                Brush brush = new SolidBrush(Color.FromArgb(100, col));
                for (int i = 0; i < points.Count - 1; i++)
                {
                    /* draw graph */
                    PointF screenA = worldToScreen(points[i]);
                    PointF screenB = worldToScreen(points[i + 1]);
                    float y0 = worldToScreen(new PointF()).Y;
                    g.DrawLine(pen, screenA, screenB);

                    /* draw integral trapezoid */
                    if (dragging && screenB.X > drag.Left && screenA.X < drag.Right)
                    {
                        PointF a = new PointF(), b = new PointF();
                        if (screenA.X < drag.Left)
                        {
                            a.X = drag.Left;
                            a.Y = screenA.Y;
                        }
                        else
                        {
                            a = screenA;
                        }
                        if (screenB.X > drag.Right)
                        {
                            b.X = drag.Right;
                            b.Y = screenA.Y;
                        }
                        else
                        {
                            b = screenB;
                        }
                        g.FillPolygon(brush, new PointF[]{
                                new PointF(a.X, y0),
                                a,
                                b,
                                new PointF(b.X, y0)});

                        /* calculate integral as we render it */
                        PointF worldA = screenToWorld(a), worldB = screenToWorld(b);
                        /* x coord is in millisecs, so we must convert back to secs */
                        integrals[var] += (double)(worldB.X - worldA.X) * (worldB.Y + worldA.Y) / varMults[var] / (2.0 * 1000.0);
                    }
                }
            }
        }
        void scaleScatter()
        {

            List<DataPoint> xs = ScatterX.points;
            List<DataPoint> ys = ScatterY.points;

            ymin = float.PositiveInfinity;
            ymax = float.NegativeInfinity;
            xmin = float.PositiveInfinity;
            xmax = float.NegativeInfinity;
            foreach (DataPoint dp in xs)
            {
                if (dp.Value < xmin)
                    xmin = (float)dp.Value;
                if (dp.Value > xmax)
                    xmax = (float)dp.Value;
            }
            foreach (DataPoint dp in ys)
            {
                if (dp.Value < ymin)
                    ymin = (float)dp.Value;
                if (dp.Value > ymax)
                    ymax = (float)dp.Value;
            }
            if (xs.Count == 0)
                xmin = xmax = 0f;
            if (ys.Count == 0)
                ymin = ymax = 0f;
            normalize(ref xmin, ref xmax);
            normalize(ref ymin, ref ymax);
        }
        void drawScatter(Graphics g, Rectangle clip)
        {
            //scale
            scaleScatter();
            drawAxes(false, g);
            drawAxes(true, g);

            //draw
            List<DataPoint> xs = ScatterX.points;
            List<DataPoint> ys = ScatterY.points;
            //float ln2 = Math.Log(2.0f); //oldest point will be half the diameter of the newest point
            int ix = xs.Count - 1, iy = ys.Count - 1;
            while (ix >= 0 && iy >= 0)
            {
                double x = xs[ix].Value, y = ys[iy].Value;
                if (xs[ix].Timestamp > ys[iy].Timestamp)
                {
                    if(iy < ys.Count-1){
                        double t = (double)(xs[ix].Timestamp - ys[iy].Timestamp).Ticks / (double)(ys[iy + 1].Timestamp - ys[iy].Timestamp).Ticks;
                        y = ys[iy].Value * (1 - t) + ys[iy + 1].Value * t;
                    }
                    ix--;
                }
                else
                {
                    if (ix < xs.Count - 1)
                    {
                        double t = (double)(ys[iy].Timestamp - xs[ix].Timestamp).Ticks / (double)(xs[ix + 1].Timestamp - xs[ix].Timestamp).Ticks;
                        x = xs[ix].Value * (1 - t) + xs[ix + 1].Value * t;
                    }
                    iy--;
                }

                PointF pScreen = worldToScreen(new PointF((float)x, (float)y));
                //float size = Math.Exp(ln2 * (i - scatterPoints.Count + 1) / (scatterPoints.Count));
                float size = 2f; //(float)(scatterPoints.Count - i + 1) / (scatterPoints.Count); //0 == oldest point, 1 = newest point
                //size = (size * size * size + 0.5f) * 2.0f; // newest point = 3.0f, oldest point = 1.0f
                g.FillEllipse(Brushes.Yellow, pScreen.X - size / 2f, pScreen.Y - size / 2f, size, size);
                
            }

            //draw cursor overlay
            if (hover)
            {
                PointF world = screenToWorld(mouse);
                string label = string.Format("({0:0.000}, {1:0.000})", world.X, world.Y);
                drawCursor(g, label);
            }
        }

        bool refreshing = false;
        public override void Refresh()
        {
            //not real locking, just a heuristic
            if (!refreshing)
            {
                refreshing = true;
                base.Refresh();
                refreshing = false;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            TimeSpan tickInterval = new TimeSpan(0, 0, 0, 1);
            Graphics g = e.Graphics;
            g.Clear(Color.Black);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.DrawImageUnscaled((Image)drawnGraph, 0, 0);

            if (Type == GraphType.Timeseries)
            {
                drawTimeseries(g, e.ClipRectangle);
            }
            else if (Type == GraphType.Scatter)
            {
                drawScatter(g, e.ClipRectangle);
            }

            base.OnPaint(e);
        }
        protected override void OnScroll(ScrollEventArgs se)
        {
            /*Debug.Assert(se.ScrollOrientation == ScrollOrientation.HorizontalScroll);
            int vaxisWidth = 20;
            int invalid = se.OldValue - se.NewValue;
            if (invalid < 0)
                invalid = 0;
            if (invalid + vaxisWidth > Width)
                invalid = Width - vaxisWidth;
            Invalidate(new Rectangle(0, 0, invalid + vaxisWidth, Height));*/
            Invalidate();

            base.OnScroll(se);
        }
        protected override void OnResize(EventArgs e)
        {
            //drawnGraph = new Bitmap(Width, Height);
            Update();
            base.OnResize(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            mouse = new PointF(e.X, e.Y);
            invalidateCursor(mouse);
            if (dragging)
            {
                if (type == GraphType.Timeseries)
                {
                    int x = e.X - AutoScrollPosition.X;
                    if (x < dragStart.X)
                    {
                        drag = new Rectangle(x, 0, dragStart.X - x, (int)vheight);
                    }
                    else
                    {
                        drag = new Rectangle(dragStart.X, 0, x - dragStart.X, (int)vheight);
                    }
                    /*int invalidWidth = drag.Right - x;
                    drag.Width = x - drag.X;
                    Invalidate(new Rectangle(e.X, 0, invalidWidth, Height));*/
                }
                Invalidate();
            }
            base.OnMouseMove(e);
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            hover = true;
            Cursor.Hide();
            base.OnMouseEnter(e);
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            hover = false;
            invalidateCursor(mouse);
            Cursor.Show();
            base.OnMouseLeave(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            dragging = true;
            dragStart = new Point(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
            drag = new Rectangle(e.X - AutoScrollPosition.X, 0, 0, Height);
            base.OnMouseDown(e);
        }
        protected override void OnMouseUp(MouseEventArgs margs)
        {
            dragging = false;
            if (type == GraphType.Timeseries)
            {
                if (IntegralDragged != null && drag.Width > 0)
                {
                    IntegralEventArgs e = new IntegralEventArgs();
                    e.Start = screenXToTime(drag.Left);
                    e.End = screenXToTime(drag.Right);
                    e.Integrals = new Dictionary<string, double>(integrals);
                    IntegralDragged(this, e);
                }
                Invalidate(drag);
            }
            base.OnMouseUp(margs);
        }
    }
}
