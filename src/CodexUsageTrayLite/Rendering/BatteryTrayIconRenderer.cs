using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using CodexUsageTrayLite.Infrastructure;
using CodexUsageTrayLite.Models;

namespace CodexUsageTrayLite.Rendering
{
    internal enum BatteryIconStyle
    {
        A,
        B,
        C,
        D
    }

    internal enum QuotaBand
    {
        Normal,
        Reminder,
        Low
    }

    internal sealed class BatteryTrayIconRenderer : IDisposable
    {
        internal const BatteryIconStyle SelectedStyle = BatteryIconStyle.A;
        internal const float SelectedStyleMagnification = 53f / 48f;

        private const int SmCxSmIcon = 49;
        private const int MinimumTrayIconSize = 16;
        private const int MaximumTrayIconSize = 32;
        internal const string AttentionGlyphColorHex = "#f04444";
        private const float GlyphMeasurementEmSize = 100f;

        private readonly Dictionary<string, Icon> cache = new Dictionary<string, Icon>(StringComparer.Ordinal);
        private bool disposed;
        private readonly Queue<string> dualKeys = new Queue<string>();

        public Icon GetDualIcon(
            int? fiveHour,
            int? weekly,
            TrayVisualState state,
            bool dark,
            bool? fiveHourLimitApplies = true,
            QuotaIconMode iconMode = QuotaIconMode.Both,
            bool fiveHourUnavailableInformational = false)
        {
            ThrowIfDisposed();
            iconMode = NormalizeIconMode(iconMode);
            var informationOnly = state == TrayVisualState.Normal &&
                iconMode == QuotaIconMode.FiveHourOnly &&
                fiveHourUnavailableInformational &&
                fiveHourLimitApplies == false;
            if (state == TrayVisualState.Normal)
            {
                var missingVisibleQuota = iconMode == QuotaIconMode.WeeklyOnly
                    ? !weekly.HasValue
                    : iconMode == QuotaIconMode.FiveHourOnly
                        ? !informationOnly && (fiveHourLimitApplies != true || !fiveHour.HasValue)
                        : !weekly.HasValue || !fiveHourLimitApplies.HasValue || (fiveHourLimitApplies == true && !fiveHour.HasValue);
                if (missingVisibleQuota) state = TrayVisualState.Unknown;
            }
            var size = GetPreferredIconSize();
            informationOnly = state == TrayVisualState.Normal && informationOnly;
            var drawFive = state == TrayVisualState.Normal && !informationOnly && fiveHourLimitApplies == true &&
                (iconMode == QuotaIconMode.FiveHourOnly || IsDualQuotaMode(iconMode));
            var drawWeekly = state == TrayVisualState.Normal && iconMode != QuotaIconMode.FiveHourOnly;
            var five = drawFive ? Percentage.Clamp(fiveHour.Value) : 0;
            var week = drawWeekly ? Percentage.Clamp(weekly.Value) : 0;
            var fiveKey = drawFive
                ? FillPixels(five, size - 2) + ":" + GetQuotaBand(five)
                : "hidden";
            var weeklyKey = drawWeekly
                ? FillPixels(week, size - 2) + ":" + GetQuotaBand(week)
                : "hidden";
            var key = "quota:" + size + ":" + dark + ":" + state + ":" + iconMode + ":" + informationOnly + ":" + fiveKey + ":" + weeklyKey;
            Icon icon;
            if (cache.TryGetValue(key, out icon)) return icon;
            using (var bitmap = RenderDualBitmap(five, week, state, size, dark, fiveHourLimitApplies, iconMode, informationOnly))
            {
                var handle = bitmap.GetHicon();
                try
                {
                    using (var borrowed = Icon.FromHandle(handle)) icon = (Icon)borrowed.Clone();
                }
                finally { DestroyIcon(handle); }
            }
            cache.Add(key, icon);
            dualKeys.Enqueue(key);
            // Bound native handles across quota pairs, themes and DPI sizes.
            if (dualKeys.Count > 16)
            {
                var oldest = dualKeys.Dequeue();
                cache[oldest].Dispose();
                cache.Remove(oldest);
            }
            return icon;
        }

        internal static int FillPixels(int percent, int available)
        {
            percent = Percentage.Clamp(percent);
            return percent == 0 ? 0 : Math.Min(available, Math.Max(1, (int)Math.Floor(available * percent / 100d + .5d)));
        }

        internal static Bitmap RenderDualBitmap(
            int fiveHour,
            int weekly,
            TrayVisualState state,
            int size,
            bool dark,
            bool? fiveHourLimitApplies = true,
            QuotaIconMode iconMode = QuotaIconMode.Both,
            bool fiveHourUnavailableInformational = false)
        {
            if (size < MinimumTrayIconSize) throw new ArgumentOutOfRangeException("size");
            iconMode = NormalizeIconMode(iconMode);
            var informationOnly = state == TrayVisualState.Normal &&
                iconMode == QuotaIconMode.FiveHourOnly &&
                fiveHourUnavailableInformational &&
                fiveHourLimitApplies == false;
            if (state == TrayVisualState.Normal && iconMode == QuotaIconMode.FiveHourOnly &&
                fiveHourLimitApplies != true && !informationOnly)
            {
                state = TrayVisualState.Unknown;
            }
            if (state == TrayVisualState.Normal && IsDualQuotaMode(iconMode) && !fiveHourLimitApplies.HasValue)
            {
                state = TrayVisualState.Unknown;
            }
            var drawFive = state == TrayVisualState.Normal && !informationOnly && fiveHourLimitApplies == true &&
                (iconMode == QuotaIconMode.FiveHourOnly || IsDualQuotaMode(iconMode));
            var drawWeekly = state == TrayVisualState.Normal && iconMode != QuotaIconMode.FiveHourOnly;
            var neutralOutline = ColorTranslator.FromHtml(dark ? "#f0f3f2" : "#000000");
            var outlineColor = drawFive ? QuotaWarningColor(fiveHour, neutralOutline) : neutralOutline;
            var bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            using (var outline = new SolidBrush(outlineColor))
            using (var track = new SolidBrush(ColorTranslator.FromHtml(dark ? "#3b3f41" : "#cfd3d5")))
            using (var weeklyBrush = new SolidBrush(QuotaWarningColor(weekly, ColorTranslator.FromHtml("#4b926a"))))
            using (var blue = new SolidBrush(ColorTranslator.FromHtml(dark ? "#60cdff" : "#0067c0")))
            {
                graphics.SmoothingMode = SmoothingMode.None;
                graphics.FillRectangle(outline, 0, 0, size, size);
                var inner = size - 2;
                graphics.FillRectangle(track, 1, 1, inner, inner);
                if (state == TrayVisualState.Normal)
                {
                    var weekHeight = FillPixels(weekly, inner);
                    var fiveHeight = FillPixels(fiveHour, inner);
                    if (iconMode == QuotaIconMode.SideBySide)
                    {
                        if (fiveHourLimitApplies == false)
                        {
                            if (drawWeekly) graphics.FillRectangle(weeklyBrush, 1, size - 1 - weekHeight, inner, weekHeight);
                        }
                        else
                        {
                            var weeklyWidth = inner / 2;
                            var fiveHourWidth = inner - weeklyWidth;
                            if (drawWeekly) graphics.FillRectangle(weeklyBrush, 1, size - 1 - weekHeight, weeklyWidth, weekHeight);
                            if (drawFive) graphics.FillRectangle(blue, 1 + weeklyWidth, size - 1 - fiveHeight, fiveHourWidth, fiveHeight);
                        }
                    }
                    else
                    {
                        var width = Math.Max(3, Math.Min(inner, (int)Math.Floor(inner * .72d + .5d)));
                        if (drawWeekly) graphics.FillRectangle(weeklyBrush, 1, size - 1 - weekHeight, inner, weekHeight);
                        if (drawFive)
                        {
                            graphics.FillRectangle(blue, 1 + (inner - width) / 2, size - 1 - fiveHeight, width, fiveHeight);
                        }
                    }
                    if (informationOnly)
                    {
                        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                        DrawInformationMark(graphics, new RectangleF(1, 1, inner, inner), size, dark);
                    }
                }
                else
                {
                    graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    DrawStateMark(graphics, state, new RectangleF(1, 1, inner, inner), size / 32f);
                }
            }
            return bitmap;
        }

        internal static QuotaIconMode NormalizeIconMode(QuotaIconMode iconMode)
        {
            return Enum.IsDefined(typeof(QuotaIconMode), iconMode) ? iconMode : QuotaIconMode.Both;
        }

        private static bool IsDualQuotaMode(QuotaIconMode iconMode)
        {
            return iconMode == QuotaIconMode.Both || iconMode == QuotaIconMode.SideBySide;
        }

        private static Color QuotaWarningColor(int remainingPercent, Color normal)
        {
            switch (GetQuotaBand(remainingPercent))
            {
                case QuotaBand.Low: return ColorTranslator.FromHtml("#f04444");
                case QuotaBand.Reminder: return ColorTranslator.FromHtml("#ff9800");
                default: return normal;
            }
        }

        public Icon GetIcon(int? remainingPercent, TrayVisualState state, BatteryIconStyle style = SelectedStyle)
        {
            ThrowIfDisposed();
            var value = Percentage.Clamp(remainingPercent ?? 0);
            var bucket = state == TrayVisualState.Normal ? (value / 5) * 5 : 0;
            var pixelSize = GetPreferredIconSize();
            var key = style + ":" + state + ":" + bucket + ":" + pixelSize;
            Icon icon;
            if (!cache.TryGetValue(key, out icon))
            {
                using (var bitmap = RenderBitmap(style, bucket, state, pixelSize))
                {
                    var handle = bitmap.GetHicon();
                    try
                    {
                        using (var borrowed = Icon.FromHandle(handle))
                        {
                            icon = (Icon)borrowed.Clone();
                        }
                    }
                    finally
                    {
                        DestroyIcon(handle);
                    }
                }
                cache.Add(key, icon);
            }
            return icon;
        }

        internal static int GetPreferredIconSize()
        {
            try
            {
                var nativeSize = GetSystemMetrics(SmCxSmIcon);
                if (nativeSize > 0)
                {
                    return Math.Max(MinimumTrayIconSize, Math.Min(MaximumTrayIconSize, nativeSize));
                }
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
            return MaximumTrayIconSize;
        }

        internal static Bitmap RenderBitmap(BatteryIconStyle style, int remainingPercent, TrayVisualState state, int size)
        {
            if (size < 16) throw new ArgumentOutOfRangeException("size");
            var bitmap = new Bitmap(size, size, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.Transparent);
                graphics.SmoothingMode = size <= 20 ? SmoothingMode.None : SmoothingMode.AntiAlias;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                DrawBattery(graphics, style, Percentage.Clamp(remainingPercent), state, size);
            }
            return bitmap;
        }

        internal static QuotaBand GetQuotaBand(int remainingPercent)
        {
            var value = Percentage.Clamp(remainingPercent);
            if (value <= 20) return QuotaBand.Low;
            if (value <= 50) return QuotaBand.Reminder;
            return QuotaBand.Normal;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            foreach (var icon in cache.Values)
            {
                icon.Dispose();
            }
            cache.Clear();
        }

        private static void DrawBattery(Graphics graphics, BatteryIconStyle style, int value, TrayVisualState state, int size)
        {
            var scale = size / 32f;
            var geometryScale = style == SelectedStyle ? scale * SelectedStyleMagnification : scale;
            var body = style == BatteryIconStyle.A
                ? GetSelectedStyleBody(size)
                : style == BatteryIconStyle.B
                    ? new RectangleF(8 * scale, 6 * scale, 16 * scale, 23 * scale)
                    : style == BatteryIconStyle.C
                        ? new RectangleF(7 * scale, 5 * scale, 18 * scale, 24 * scale)
                        : new RectangleF(5 * scale, 7 * scale, 22 * scale, 21 * scale);
            var terminal = style == BatteryIconStyle.D
                ? new RectangleF(12 * scale, 4 * scale, 8 * scale, 3 * scale)
                : new RectangleF((body.X + body.Width * 0.31f), body.Y - 3 * geometryScale, body.Width * 0.38f, 3 * geometryScale);
            var outline = state == TrayVisualState.Normal ? Color.FromArgb(226, 232, 231) : Color.FromArgb(163, 168, 169);
            var fill = StateColor(value, state);
            var stroke = Math.Max(1f, (style == BatteryIconStyle.B ? 1.8f : 2f) * geometryScale);

            using (var outlinePen = new Pen(outline, stroke))
            using (var terminalBrush = new SolidBrush(outline))
            using (var fillBrush = new SolidBrush(fill))
            {
                graphics.FillRectangle(terminalBrush, terminal);
                graphics.DrawRectangle(outlinePen, body.X, body.Y, body.Width, body.Height);
                var inset = Math.Max(2f * geometryScale, stroke + geometryScale * .5f);
                var inner = RectangleF.Inflate(body, -inset, -inset);

                if (state == TrayVisualState.Normal)
                {
                    if (style == BatteryIconStyle.C)
                    {
                        DrawSegments(graphics, fillBrush, inner, value, geometryScale);
                    }
                    else
                    {
                        var height = inner.Height * value / 100f;
                        if (height > 0)
                        {
                            graphics.FillRectangle(fillBrush, inner.X, inner.Bottom - height, inner.Width, height);
                        }
                    }
                }

                if (style == BatteryIconStyle.D && state == TrayVisualState.Normal)
                {
                    DrawNumber(graphics, value, body, size);
                }
                else if (state != TrayVisualState.Normal)
                {
                    DrawStateMark(graphics, state, body, geometryScale);
                }
            }
        }

        internal static RectangleF GetSelectedStyleBody(int size)
        {
            if (size < MinimumTrayIconSize) throw new ArgumentOutOfRangeException("size");
            var geometryScale = size / 32f * SelectedStyleMagnification;
            var width = 18f * geometryScale;
            var height = 24f * geometryScale;
            return new RectangleF((size - width) / 2f, (size - height) / 2f + size / 32f, width, height);
        }

        private static void DrawSegments(Graphics graphics, Brush brush, RectangleF inner, int value, float scale)
        {
            const int count = 5;
            var gap = Math.Max(1f, scale);
            var segmentHeight = (inner.Height - gap * (count - 1)) / count;
            var filled = (int)Math.Ceiling(value / 20d);
            for (var index = 0; index < filled; index++)
            {
                var y = inner.Bottom - (index + 1) * segmentHeight - index * gap;
                graphics.FillRectangle(brush, inner.X, y, inner.Width, segmentHeight);
            }
        }

        private static void DrawNumber(Graphics graphics, int value, RectangleF body, int size)
        {
            using (var font = new Font("Segoe UI", Math.Max(6f, size * .24f), FontStyle.Bold, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color.White))
            using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                graphics.DrawString(value.ToString(), font, brush, body, format);
            }
        }

        private static void DrawStateMark(Graphics graphics, TrayVisualState state, RectangleF body, float scale)
        {
            if (state == TrayVisualState.FetchError)
            {
                using (var pen = new Pen(Color.FromArgb(198, 101, 87), Math.Max(2f, 2.4f * scale)))
                {
                    graphics.DrawLine(pen, body.Left + 3 * scale, body.Bottom - 4 * scale, body.Right - 3 * scale, body.Top + 4 * scale);
                }
                return;
            }
            var mark = state == TrayVisualState.LoginRequired ? "L" : "?";
            DrawAttentionGlyph(graphics, mark, body);
        }

        private static void DrawInformationMark(Graphics graphics, RectangleF body, int size, bool dark)
        {
            DrawAttentionGlyph(graphics, "i", body);
        }

        private static void DrawAttentionGlyph(Graphics graphics, string glyph, RectangleF body)
        {
            // Fit the actual bold glyph bounds, rather than stretching a pre-rendered bitmap.
            // This lets naturally narrow characters such as "i" use the maximum safe height
            // while preserving the typeface's original aspect ratio at every tray-icon size.
            var inset = Math.Max(.75f, Math.Min(body.Width, body.Height) * .04f);
            var target = RectangleF.FromLTRB(body.Left + inset, body.Top + inset, body.Right - inset, body.Bottom - inset);
            using (var family = new FontFamily("Segoe UI"))
            using (var format = (StringFormat)StringFormat.GenericTypographic.Clone())
            using (var measurementPath = new GraphicsPath())
            using (var brush = new SolidBrush(ColorTranslator.FromHtml(AttentionGlyphColorHex)))
            {
                measurementPath.AddString(glyph, family, (int)FontStyle.Bold, GlyphMeasurementEmSize, PointF.Empty, format);
                var measured = measurementPath.GetBounds();
                if (measured.Width <= 0 || measured.Height <= 0) return;

                var uniformScale = Math.Min(target.Width / measured.Width, target.Height / measured.Height);
                var emSize = GlyphMeasurementEmSize * uniformScale;
                using (var glyphPath = new GraphicsPath())
                {
                    glyphPath.AddString(glyph, family, (int)FontStyle.Bold, emSize, PointF.Empty, format);
                    var bounds = glyphPath.GetBounds();
                    using (var transform = new Matrix())
                    {
                        transform.Translate(
                            target.Left + (target.Width - bounds.Width) / 2f - bounds.Left,
                            target.Top + (target.Height - bounds.Height) / 2f - bounds.Top);
                        glyphPath.Transform(transform);
                    }

                    var previousSmoothingMode = graphics.SmoothingMode;
                    try
                    {
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.FillPath(brush, glyphPath);
                    }
                    finally
                    {
                        graphics.SmoothingMode = previousSmoothingMode;
                    }
                }
            }
        }

        private static Color StateColor(int value, TrayVisualState state)
        {
            if (state != TrayVisualState.Normal) return Color.FromArgb(123, 128, 130);
            switch (GetQuotaBand(value))
            {
                case QuotaBand.Low: return Color.FromArgb(184, 88, 80);
                case QuotaBand.Reminder: return Color.FromArgb(185, 149, 69);
                default: return Color.FromArgb(75, 146, 106);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().Name);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr handle);

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int index);
    }
}
