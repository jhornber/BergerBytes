using BergerBytes.Shared.Models;
using Microsoft.Maui.Graphics;

namespace BergerBytes.App.ViewModels
{
    /// <summary>
    /// A simple line chart drawn with IDrawable (no third-party library).
    /// Groups entries by day (last reading per day) and plots weight in the user's preferred unit.
    /// </summary>
    public class WeightChartDrawable : IDrawable
    {
        private readonly List<(DateTime Date, double Weight)> _points;
        private readonly string _unit;

        // Brand primary: #E8533A
        private static readonly Color LineColor = Color.FromArgb("#E8533A");
        private static readonly Color GridColor = Color.FromArgb("#33808080");
        private static readonly Color LabelColor = Color.FromArgb("#808080");

        public WeightChartDrawable(IEnumerable<WeightLog> logs, string weightUnit = "kg")
        {
            _unit = weightUnit;
            // One entry per calendar day — use the last recorded entry for that day
            _points = logs
                .GroupBy(l => l.LoggedAt.Date)
                .Select(g =>
                {
                    var last = g.OrderByDescending(l => l.LoggedAt).First();
                    double w = weightUnit == "lbs" ? last.WeightKg / 0.45359237 : last.WeightKg;
                    return (last.LoggedAt.Date, w);
                })
                .OrderBy(p => p.Date)
                .ToList();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (_points.Count < 2)
                return;

            const float paddingLeft = 52f;
            const float paddingRight = 16f;
            const float paddingTop = 16f;
            const float paddingBottom = 36f;

            float chartLeft = paddingLeft;
            float chartRight = dirtyRect.Width - paddingRight;
            float chartTop = paddingTop;
            float chartBottom = dirtyRect.Height - paddingBottom;
            float chartWidth = chartRight - chartLeft;
            float chartHeight = chartBottom - chartTop;

            double minW = _points.Min(p => p.Weight);
            double maxW = _points.Max(p => p.Weight);
            double range = maxW - minW;

            // Minimum visible range: 4.4 lbs ≈ 2 kg
            double minRange = _unit == "lbs" ? 4.4 : 2.0;
            if (range < minRange) { minW -= minRange / 2.0; maxW += minRange / 2.0; range = minRange; }
            else { minW -= range * 0.1; maxW += range * 0.1; range = maxW - minW; }

            var minDate = _points[0].Date;
            var maxDate = _points[^1].Date;
            double dateRange = Math.Max(1, (maxDate - minDate).TotalDays);

            // --- Horizontal grid lines + y-axis labels ---
            canvas.StrokeColor = GridColor;
            canvas.StrokeSize = 1;
            canvas.FontSize = 10;
            canvas.FontColor = LabelColor;

            const int gridLines = 4;
            for (int i = 0; i <= gridLines; i++)
            {
                float y = chartTop + (i / (float)gridLines) * chartHeight;
                canvas.DrawLine(chartLeft, y, chartRight, y);
                double labelWeight = maxW - (i / (double)gridLines) * range;
                canvas.DrawString($"{labelWeight:F1}", 0, y - 7f, chartLeft - 4f, 14f,
                    HorizontalAlignment.Right, VerticalAlignment.Top);
            }

            // --- Compute pixel coordinates for each data point ---
            var pts = _points.Select(p =>
            {
                float x = chartLeft + (float)((p.Date - minDate).TotalDays / dateRange) * chartWidth;
                float y = chartTop + (float)(1.0 - (p.Weight - minW) / range) * chartHeight;
                return new PointF(x, y);
            }).ToList();

            // --- Draw the line ---
            canvas.StrokeColor = LineColor;
            canvas.StrokeSize = 2.5f;
            canvas.StrokeLineCap = LineCap.Round;
            canvas.StrokeLineJoin = LineJoin.Round;

            var path = new PathF();
            path.MoveTo(pts[0]);
            for (int i = 1; i < pts.Count; i++)
                path.LineTo(pts[i]);
            canvas.DrawPath(path);

            // --- Draw dots ---
            canvas.FillColor = LineColor;
            foreach (var pt in pts)
                canvas.FillCircle(pt.X, pt.Y, 5f);

            // --- X-axis date labels ---
            canvas.FontColor = LabelColor;
            canvas.FontSize = 9;
            int step = Math.Max(1, _points.Count / 5);
            var labelledIndices = Enumerable.Range(0, _points.Count)
                .Where(i => i % step == 0)
                .ToHashSet();
            // Always include the last point
            labelledIndices.Add(_points.Count - 1);

            foreach (int i in labelledIndices)
            {
                float x = pts[i].X;
                canvas.DrawString(_points[i].Date.ToString("M/d"),
                    x - 20f, chartBottom + 4f, 40f, 14f,
                    HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }
    }
}
