using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace RPA_ULTRA_CORE.Views.Controls
{
    public partial class AnimatedCircularProgress : UserControl
    {
        private float _currentProgress = 0f;
        private float _targetProgress = 0f;
        private float _animationProgress = 0f;
        private DispatcherTimer _animationTimer;

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(nameof(Progress), typeof(double), typeof(AnimatedCircularProgress),
                new PropertyMetadata(0.0, OnProgressChanged));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(AnimatedCircularProgress),
                new PropertyMetadata("Progress"));

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(SKColor), typeof(AnimatedCircularProgress),
                new PropertyMetadata(SKColors.BlueViolet));

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public SKColor Color
        {
            get => (SKColor)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public AnimatedCircularProgress()
        {
            InitializeComponent();

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
        }

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedCircularProgress control)
            {
                control._targetProgress = (float)(double)e.NewValue;
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            // Smooth animation
            float diff = _targetProgress - _currentProgress;
            _currentProgress += diff * 0.1f;

            // Wave animation
            _animationProgress += 0.05f;
            if (_animationProgress > 360f)
                _animationProgress = 0f;

            skiaCanvas.InvalidateVisual();
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            int width = e.Info.Width;
            int height = e.Info.Height;
            float centerX = width / 2f;
            float centerY = height / 2f;
            float radius = Math.Min(width, height) / 2.5f;

            // Draw background circle
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.WithAlpha(30),
                StrokeWidth = 20,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(centerX, centerY, radius, paint);
            }

            // Draw progress arc with gradient
            float sweepAngle = 360f * _currentProgress;
            using (var shader = SKShader.CreateSweepGradient(
                new SKPoint(centerX, centerY),
                new[] { Color.WithAlpha(150), Color, SKColors.White },
                new[] { 0f, 0.5f, 1f }))
            {
                using (var paint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Shader = shader,
                    StrokeWidth = 20,
                    StrokeCap = SKStrokeCap.Round,
                    IsAntialias = true
                })
                {
                    var rect = new SKRect(
                        centerX - radius,
                        centerY - radius,
                        centerX + radius,
                        centerY + radius);

                    canvas.DrawArc(rect, -90, sweepAngle, false, paint);
                }
            }

            // Draw pulsing glow effect
            float glowPulse = (float)(Math.Sin(_animationProgress / 10f) * 0.3f + 0.7f);
            using (var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = Color.WithAlpha((byte)(50 * glowPulse)),
                StrokeWidth = 30,
                IsAntialias = true,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
            })
            {
                var rect = new SKRect(
                    centerX - radius,
                    centerY - radius,
                    centerX + radius,
                    centerY + radius);

                canvas.DrawArc(rect, -90, sweepAngle, false, paint);
            }

            // Draw percentage text
            string percentText = $"{(int)(_currentProgress * 100)}%";
            using (var textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = radius * 0.6f,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
            })
            {
                canvas.DrawText(percentText, centerX, centerY + textPaint.TextSize / 3f, textPaint);
            }

            // Draw title
            using (var titlePaint = new SKPaint
            {
                Color = SKColors.White.WithAlpha(200),
                TextSize = radius * 0.25f,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Normal)
            })
            {
                canvas.DrawText(Title, centerX, centerY + radius + 40, titlePaint);
            }
        }
    }
}
