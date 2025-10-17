using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace RPA_ULTRA_CORE.Views.Controls
{
    public partial class AnimatedBarChart : UserControl
    {
        private float[] _currentValues = new float[0];
        private float[] _targetValues = new float[0];
        private string[] _labels = new string[0];
        private float _animationProgress = 0f;
        private DispatcherTimer _animationTimer;

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(double[]), typeof(AnimatedBarChart),
                new PropertyMetadata(null, OnDataChanged));

        public static readonly DependencyProperty LabelsProperty =
            DependencyProperty.Register(nameof(Labels), typeof(string[]), typeof(AnimatedBarChart),
                new PropertyMetadata(null, OnLabelsChanged));

        public double[]? Data
        {
            get => (double[]?)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public string[]? Labels
        {
            get => (string[]?)GetValue(LabelsProperty);
            set => SetValue(LabelsProperty, value);
        }

        public AnimatedBarChart()
        {
            InitializeComponent();

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _animationTimer.Tick += AnimationTimer_Tick;
            _animationTimer.Start();
        }

        private static void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedBarChart control && e.NewValue is double[] data)
            {
                control._targetValues = data.Select(v => (float)v).ToArray();
                if (control._currentValues.Length != data.Length)
                {
                    control._currentValues = new float[data.Length];
                }
            }
        }

        private static void OnLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is AnimatedBarChart control && e.NewValue is string[] labels)
            {
                control._labels = labels;
            }
        }

        private void AnimationTimer_Tick(object? sender, EventArgs e)
        {
            bool needsUpdate = false;

            for (int i = 0; i < _currentValues.Length && i < _targetValues.Length; i++)
            {
                float diff = _targetValues[i] - _currentValues[i];
                if (Math.Abs(diff) > 0.01f)
                {
                    _currentValues[i] += diff * 0.1f;
                    needsUpdate = true;
                }
            }

            _animationProgress += 0.05f;
            if (_animationProgress > 360f)
                _animationProgress = 0f;

            if (needsUpdate || _animationProgress < 360f)
            {
                skiaCanvas.InvalidateVisual();
            }
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            if (_currentValues.Length == 0)
                return;

            int width = e.Info.Width;
            int height = e.Info.Height;

            float padding = 40;
            float barSpacing = 20;
            float availableWidth = width - (2 * padding);
            float barWidth = (availableWidth - ((_currentValues.Length - 1) * barSpacing)) / _currentValues.Length;

            float maxValue = _targetValues.Length > 0 ? _targetValues.Max() : 1f;
            if (maxValue == 0) maxValue = 1f;

            float chartHeight = height - 100;

            // Draw grid lines
            using (var gridPaint = new SKPaint
            {
                Color = SKColors.White.WithAlpha(20),
                StrokeWidth = 1,
                IsAntialias = true
            })
            {
                for (int i = 0; i <= 5; i++)
                {
                    float y = padding + (chartHeight * i / 5f);
                    canvas.DrawLine(padding, y, width - padding, y, gridPaint);
                }
            }

            // Draw bars
            for (int i = 0; i < _currentValues.Length; i++)
            {
                float x = padding + (i * (barWidth + barSpacing));
                float barHeight = (chartHeight * _currentValues[i] / maxValue);
                float y = padding + chartHeight - barHeight;

                // Gradient for bars
                var colors = new[] {
                    new SKColor(156, 39, 176),  // Deep Purple
                    new SKColor(103, 58, 183),  // Deep Purple lighter
                    new SKColor(205, 220, 57)   // Lime
                };

                using (var shader = SKShader.CreateLinearGradient(
                    new SKPoint(x, y + barHeight),
                    new SKPoint(x, y),
                    colors,
                    null,
                    SKShaderTileMode.Clamp))
                {
                    using (var barPaint = new SKPaint
                    {
                        Shader = shader,
                        IsAntialias = true
                    })
                    {
                        var rect = new SKRect(x, y, x + barWidth, padding + chartHeight);
                        canvas.DrawRoundRect(rect, 8, 8, barPaint);
                    }
                }

                // Glow effect
                float glowPulse = (float)(Math.Sin((_animationProgress + i * 30) / 10f) * 0.3f + 0.7f);
                using (var glowPaint = new SKPaint
                {
                    Color = new SKColor(156, 39, 176).WithAlpha((byte)(80 * glowPulse)),
                    IsAntialias = true,
                    MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 15)
                })
                {
                    var rect = new SKRect(x, y, x + barWidth, padding + chartHeight);
                    canvas.DrawRoundRect(rect, 8, 8, glowPaint);
                }

                // Draw value on top
                string valueText = $"{(int)_currentValues[i]}";
                using (var textPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = 20,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center,
                    Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
                })
                {
                    canvas.DrawText(valueText, x + barWidth / 2, y - 10, textPaint);
                }

                // Draw label
                if (i < _labels.Length)
                {
                    using (var labelPaint = new SKPaint
                    {
                        Color = SKColors.White.WithAlpha(200),
                        TextSize = 16,
                        IsAntialias = true,
                        TextAlign = SKTextAlign.Center,
                        Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Normal)
                    })
                    {
                        canvas.DrawText(_labels[i], x + barWidth / 2, padding + chartHeight + 30, labelPaint);
                    }
                }
            }
        }
    }
}
