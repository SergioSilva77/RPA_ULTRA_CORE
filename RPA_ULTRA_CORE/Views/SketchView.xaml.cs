using System.Windows;
using System.Windows.Input;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using RPA_ULTRA_CORE.ViewModels;
using RPA_ULTRA_CORE.Services;

namespace RPA_ULTRA_CORE.Views
{
    /// <summary>
    /// Code-behind da SketchView - apenas repassa eventos ao ViewModel
    /// </summary>
    public partial class SketchView : Window
    {
        private SketchViewModel _viewModel;
        private readonly EventBus _eventBus;

        public SketchView()
        {
            InitializeComponent();

            _viewModel = new SketchViewModel();
            DataContext = _viewModel;

            _eventBus = EventBus.Instance;

            // Inscreve para invalidação do canvas
            _eventBus.Subscribe<CanvasInvalidatedEvent>(OnCanvasInvalidatedEvent);

            // Registra eventos de teclado na janela
            PreviewKeyDown += OnKeyDown;
            PreviewKeyUp += OnKeyUp;

            // Foco inicial no canvas
            skiaCanvas.Focus();

            // Adiciona o evento de duplo clique
            skiaCanvas.MouseLeftButtonDown += OnSkiaCanvasMouseLeftButtonDown;
        }

        /// <summary>
        /// Evento de pintura do canvas SkiaSharp
        /// </summary>
        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            var info = e.Info;

            // Calcula DPI scale
            var dpiScale = (float)(info.Width / skiaCanvas.ActualWidth);

            // Delega desenho ao ViewModel
            _viewModel.Draw(canvas, dpiScale);
        }

        /// <summary>
        /// Mouse down - converte coordenadas e repassa ao VM
        /// </summary>
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var point = e.GetPosition(skiaCanvas);
            var skPoint = ConvertToSKPoint(point);

            _viewModel.OnMouseDown(skPoint);

            // Captura mouse para continuar recebendo eventos fora da janela
            skiaCanvas.CaptureMouse();
            skiaCanvas.InvalidateVisual();
        }

        /// <summary>
        /// Mouse move - converte coordenadas e repassa ao VM
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(skiaCanvas);
            var skPoint = ConvertToSKPoint(point);

            _viewModel.OnMouseMove(skPoint);
            skiaCanvas.InvalidateVisual();
        }

        /// <summary>
        /// Mouse up - converte coordenadas e repassa ao VM
        /// </summary>
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var point = e.GetPosition(skiaCanvas);
            var skPoint = ConvertToSKPoint(point);

            _viewModel.OnMouseUp(skPoint);

            // Libera captura do mouse
            skiaCanvas.ReleaseMouseCapture();
            skiaCanvas.InvalidateVisual();
        }

        /// <summary>
        /// Detecta duplo clique no canvas
        /// </summary>
        private void OnSkiaCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Verifica se é duplo clique
            if (e.ClickCount == 2)
            {
                var point = e.GetPosition(skiaCanvas);
                var skPoint = ConvertToSKPoint(point);

                _viewModel.OnMouseDoubleClick(skPoint);
                skiaCanvas.InvalidateVisual();

                // Marca o evento como manipulado para evitar processamento duplo
                e.Handled = true;
            }
        }

        /// <summary>
        /// Tecla pressionada - repassa ao VM
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            _viewModel.OnKeyDown(e.Key);
            skiaCanvas.InvalidateVisual();
        }

        /// <summary>
        /// Tecla solta - repassa ao VM
        /// </summary>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            _viewModel.OnKeyUp(e.Key);
            skiaCanvas.InvalidateVisual();
        }

        /// <summary>
        /// Converte Point WPF para SKPoint
        /// </summary>
        private SKPoint ConvertToSKPoint(Point wpfPoint)
        {
            var scaleX = (float)(skiaCanvas.CanvasSize.Width / skiaCanvas.ActualWidth);
            var scaleY = (float)(skiaCanvas.CanvasSize.Height / skiaCanvas.ActualHeight);

            return new SKPoint(
                (float)(wpfPoint.X * scaleX),
                (float)(wpfPoint.Y * scaleY));
        }

        /// <summary>
        /// Responde a evento de invalidação do canvas
        /// </summary>
        private void OnCanvasInvalidatedEvent(CanvasInvalidatedEvent evt)
        {
            Dispatcher.Invoke(() => skiaCanvas.InvalidateVisual());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _eventBus.Unsubscribe<CanvasInvalidatedEvent>(OnCanvasInvalidatedEvent);
        }
    }
}