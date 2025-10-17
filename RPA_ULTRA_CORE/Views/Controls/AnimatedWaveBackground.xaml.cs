using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;

namespace RPA_ULTRA_CORE.Views.Controls
{
    public partial class AnimatedWaveBackground : UserControl
    {
        private float _waveOffset = 0f;
        private DispatcherTimer _animationTimer;

        public static readonly DependencyProperty WaveColorProperty =
            DependencyProperty.Register(nameof(WaveColor), typeof(SKColor), typeof(AnimatedWaveBackground),
                new PropertyMetadata(SKColors.Purple));

        public SKColor WaveColor
        {
            get => (SKColor)GetValue(WaveColorProperty);
            set => SetValue(WaveColorProperty, value);
        }

        public AnimatedWaveBackground()
        {
            InitializeComponent();

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _animationTimer.Tick += (s, e) =>
            {
                _waveOffset += 2f;
                if (_waveOffset > 1000f)
                    _waveOffset = 0f;

                skiaCanvas.InvalidateVisual();
            };
            _animationTimer.Start();
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            int width = e.Info.Width;
            int height = e.Info.Height;

            // Draw multiple wave layers
            DrawWave(canvas, width, height, _waveOffset, 0.02f, WaveColor.WithAlpha(60), 30);
            DrawWave(canvas, width, height, _waveOffset * 1.3f, 0.025f, WaveColor.WithAlpha(40), 50);
            DrawWave(canvas, width, height, _waveOffset * 0.7f, 0.015f, WaveColor.WithAlpha(80), 70);

            // Draw particles
            DrawParticles(canvas, width, height);
        }

        private void DrawWave(SKCanvas canvas, int width, int height, float offset, float frequency, SKColor color, float amplitude)
        {
            using var path = new SKPath();
            path.MoveTo(0, height);

            for (float x = 0; x <= width; x += 5)
            {
                float y = (float)(height / 2 + Math.Sin((x + offset) * frequency) * amplitude);
                path.LineTo(x, y);
            }

            path.LineTo(width, height);
            path.Close();

            using var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            canvas.DrawPath(path, paint);
        }

        private void DrawParticles(SKCanvas canvas, int width, int height)
        {
            Random random = new Random((int)_waveOffset);

            for (int i = 0; i < 20; i++)
            {
                float x = (float)((random.NextDouble() * width + _waveOffset * 0.5) % width);
                float y = (float)(random.NextDouble() * height);
                float size = (float)(random.NextDouble() * 4 + 2);
                byte alpha = (byte)(random.NextDouble() * 150 + 50);

                using var paint = new SKPaint
                {
                    Color = SKColors.White.WithAlpha(alpha),
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                    MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 2)
                };

                canvas.DrawCircle(x, y, size, paint);
            }
        }
    }
}
