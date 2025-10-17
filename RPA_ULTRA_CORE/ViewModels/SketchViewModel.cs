using System.Collections.ObjectModel;
using System.Windows.Input;
using SkiaSharp;
using RPA_ULTRA_CORE.Models.Geometry;
using RPA_ULTRA_CORE.Services;
using RPA_ULTRA_CORE.Utils;
using RPA_ULTRA_CORE.Helpers;

namespace RPA_ULTRA_CORE.ViewModels
{
    /// <summary>
    /// ViewModel principal para o editor de desenho vetorial
    /// </summary>
    public class SketchViewModel : ViewModelBase
    {
        private readonly SnapService _snapService;
        private readonly EventBus _eventBus;

        private ToolState _currentToolState = ToolState.Idle;
        private BaseShape? _selectedShape;
        private Node? _draggingNode;
        private Node? _tempStartNode;
        private SKPoint _dragStartPoint;
        private SKPoint _dragOffset;
        private bool _isShiftPressed;
        private bool _needsRedraw = true;
        private LineSnapResult? _currentSnapPreview;

        public ObservableCollection<BaseShape> Shapes { get; }
        public ObservableCollection<Node> AllNodes { get; }

        public ToolState CurrentToolState
        {
            get => _currentToolState;
            set => SetProperty(ref _currentToolState, value);
        }

        public BaseShape? SelectedShape
        {
            get => _selectedShape;
            set
            {
                if (_selectedShape != null)
                    _selectedShape.IsSelected = false;

                SetProperty(ref _selectedShape, value);

                if (_selectedShape != null)
                    _selectedShape.IsSelected = true;

                _eventBus.Publish(new SelectionChangedEvent { Shape = _selectedShape });
            }
        }

        public bool IsShiftPressed
        {
            get => _isShiftPressed;
            set => SetProperty(ref _isShiftPressed, value);
        }

        public ICommand DeleteSelectionCommand { get; }
        public ICommand ClearCanvasCommand { get; }

        public SketchViewModel()
        {
            _snapService = new SnapService();
            _eventBus = EventBus.Instance;

            Shapes = new ObservableCollection<BaseShape>();
            AllNodes = new ObservableCollection<Node>();

            DeleteSelectionCommand = new RelayCommand(ExecuteDeleteSelection);
            ClearCanvasCommand = new RelayCommand(ExecuteClearCanvas);

            // Inscreve-se para eventos
            _eventBus.Subscribe<CanvasInvalidatedEvent>(OnCanvasInvalidated);
        }

        /// <summary>
        /// Processa mouse down
        /// </summary>
        public void OnMouseDown(SKPoint point)
        {
            _dragStartPoint = point;
            point = _snapService.Snap(point, AllNodes);

            if (IsShiftPressed && CurrentToolState == ToolState.Idle)
            {
                // Inicia desenho de linha
                CurrentToolState = ToolState.DrawingLine;
                _tempStartNode = new Node(point.X, point.Y);
                RequestRedraw();
                return;
            }

            // Verifica hit em handles de linha
            foreach (var shape in Shapes)
            {
                if (shape is LineShape line && line.IsSelected)
                {
                    var handleNode = line.HitTestHandle(point, LineShape.HandleRadius);
                    if (handleNode != null)
                    {
                        CurrentToolState = ToolState.DraggingHandle;
                        _draggingNode = handleNode;
                        SelectedShape = line;
                        return;
                    }
                }
            }

            // Verifica hit em formas
            BaseShape? hitShape = null;
            foreach (var shape in Shapes.Reverse()) // Verifica de cima para baixo
            {
                if (shape.HitTestPoint(point, 6f))
                {
                    hitShape = shape;
                    break;
                }
            }

            if (hitShape != null)
            {
                CurrentToolState = ToolState.DraggingShape;
                SelectedShape = hitShape;
                _dragOffset = new SKPoint(
                    point.X - (float)hitShape.GetBounds().MidX,
                    point.Y - (float)hitShape.GetBounds().MidY);
            }
            else
            {
                SelectedShape = null;
            }
        }

        /// <summary>
        /// Processa mouse move
        /// </summary>
        public void OnMouseMove(SKPoint point)
        {
            switch (CurrentToolState)
            {
                case ToolState.DrawingLine:
                    // Atualiza preview da linha
                    RequestRedraw();
                    break;

                case ToolState.DraggingHandle:
                    if (_draggingNode != null)
                    {
                        // Marca que o usuário está movendo
                        _draggingNode.BeginUserMove();

                        // Detecta snap em linha
                        _currentSnapPreview = _snapService.DetectLineSnap(point, Shapes, SelectedShape);

                        if (_currentSnapPreview != null)
                        {
                            _draggingNode.Set(_currentSnapPreview.SnapPoint.X, _currentSnapPreview.SnapPoint.Y);
                        }
                        else
                        {
                            var snappedPoint = _snapService.Snap(point, AllNodes.Where(n => n != _draggingNode));
                            _draggingNode.Set(snappedPoint.X, snappedPoint.Y);
                        }

                        _draggingNode.EndUserMove();
                        RequestRedraw();
                    }
                    break;

                case ToolState.DraggingShape:
                    if (SelectedShape != null)
                    {
                        var dx = point.X - _dragStartPoint.X;
                        var dy = point.Y - _dragStartPoint.Y;
                        SelectedShape.Move(dx, dy);
                        _dragStartPoint = point;
                        RequestRedraw();
                    }
                    break;
            }

            // Atualiza hover
            foreach (var shape in Shapes)
            {
                shape.IsHovered = shape.HitTestPoint(point, 6f);
            }
        }

        /// <summary>
        /// Processa mouse up
        /// </summary>
        public void OnMouseUp(SKPoint point)
        {
            point = _snapService.Snap(point, AllNodes);

            switch (CurrentToolState)
            {
                case ToolState.DrawingLine:
                    if (_tempStartNode != null)
                    {
                        // Cria ou conecta end node
                        var endNode = _snapService.SnapToNode(point, AllNodes) ?? new Node(point.X, point.Y);

                        if (!AllNodes.Contains(_tempStartNode))
                            AllNodes.Add(_tempStartNode);
                        if (!AllNodes.Contains(endNode))
                            AllNodes.Add(endNode);

                        // Cria a linha
                        var line = new LineShape(_tempStartNode, endNode);
                        Shapes.Add(line);
                        SelectedShape = line;

                        _tempStartNode = null;
                        _eventBus.Publish(new ShapeAddedEvent { Shape = line });
                    }
                    break;

                case ToolState.DraggingHandle:
                    // Tenta conectar com linha ou node ao soltar
                    if (_draggingNode != null && _currentSnapPreview != null)
                    {
                        if (_currentSnapPreview.IsEndpoint && _currentSnapPreview.EndpointNode != null)
                        {
                            // Conecta no endpoint existente
                            _draggingNode.AttachToEndpoint(_currentSnapPreview.EndpointNode);
                        }
                        else if (!_currentSnapPreview.IsEndpoint && _currentSnapPreview.TargetLine != null)
                        {
                            // Conecta no meio da linha (mid-span)
                            _draggingNode.AttachTo(_currentSnapPreview.TargetLine, _currentSnapPreview.T);
                        }
                    }
                    else if (_draggingNode != null)
                    {
                        // Tenta snap tradicional em nodes
                        var snappedNode = _snapService.SnapToNode(
                            new SKPoint((float)_draggingNode.X, (float)_draggingNode.Y),
                            AllNodes.Where(n => n != _draggingNode));

                        if (snappedNode != null)
                        {
                            // Merge nodes - conecta linhas ao mesmo ponto
                            MergeNodes(_draggingNode, snappedNode);
                        }
                    }
                    _draggingNode = null;
                    _currentSnapPreview = null;
                    break;
            }

            if (!IsShiftPressed)
                CurrentToolState = ToolState.Idle;

            RequestRedraw();
        }

        /// <summary>
        /// Processa tecla pressionada
        /// </summary>
        public void OnKeyDown(Key key)
        {
            switch (key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    IsShiftPressed = true;
                    if (CurrentToolState == ToolState.Idle)
                    {
                        // Pronto para desenhar linha
                    }
                    break;

                case Key.Delete:
                    ExecuteDeleteSelection(null);
                    break;

                case Key.Escape:
                    CurrentToolState = ToolState.Idle;
                    _tempStartNode = null;
                    _draggingNode = null;
                    RequestRedraw();
                    break;
            }
        }

        /// <summary>
        /// Processa tecla solta
        /// </summary>
        public void OnKeyUp(Key key)
        {
            switch (key)
            {
                case Key.LeftShift:
                case Key.RightShift:
                    IsShiftPressed = false;
                    if (CurrentToolState == ToolState.DrawingLine)
                    {
                        CurrentToolState = ToolState.Idle;
                        _tempStartNode = null;
                        RequestRedraw();
                    }
                    break;
            }
        }

        /// <summary>
        /// Desenha o canvas
        /// </summary>
        public void Draw(SKCanvas canvas, float dpiScale)
        {
            canvas.Clear(SKColors.Black);

            // Desenha grid
            DrawGrid(canvas, dpiScale);

            // Desenha shapes
            foreach (var shape in Shapes)
            {
                shape.Draw(canvas, dpiScale);
            }

            // Desenha preview de linha sendo criada
            if (CurrentToolState == ToolState.DrawingLine && _tempStartNode != null)
            {
                using var previewPaint = new SKPaint
                {
                    Color = SKColors.Gray,
                    StrokeWidth = 2f * dpiScale,
                    PathEffect = SKPathEffect.CreateDash(new[] { 5f, 5f }, 0),
                    IsAntialias = true
                };

                canvas.DrawLine(
                    (float)_tempStartNode.X, (float)_tempStartNode.Y,
                    _dragStartPoint.X, _dragStartPoint.Y,
                    previewPaint);
            }

            // Desenha indicador de snap mid-span
            if (_currentSnapPreview != null && !_currentSnapPreview.IsEndpoint)
            {
                using var snapPaint = new SKPaint
                {
                    Color = SKColors.Yellow,
                    StrokeWidth = 2f * dpiScale,
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                using var outlinePaint = new SKPaint
                {
                    Color = SKColors.Black,
                    StrokeWidth = 1f * dpiScale,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };

                // Desenha um círculo indicando o ponto de snap
                canvas.DrawCircle(_currentSnapPreview.SnapPoint, 8f * dpiScale, snapPaint);
                canvas.DrawCircle(_currentSnapPreview.SnapPoint, 8f * dpiScale, outlinePaint);

                // Desenha linha conectora fantasma
                if (_draggingNode != null)
                {
                    using var linePaint = new SKPaint
                    {
                        Color = SKColors.Yellow.WithAlpha(128),
                        StrokeWidth = 1.5f * dpiScale,
                        PathEffect = SKPathEffect.CreateDash(new[] { 3f, 3f }, 0),
                        IsAntialias = true
                    };

                    canvas.DrawLine(
                        (float)_draggingNode.X, (float)_draggingNode.Y,
                        _currentSnapPreview.SnapPoint.X, _currentSnapPreview.SnapPoint.Y,
                        linePaint);
                }
            }
        }

        private void DrawGrid(SKCanvas canvas, float dpiScale)
        {
            var bounds = canvas.LocalClipBounds;
            var gridSize = _snapService.GridSize;

            using var gridPaint = new SKPaint
            {
                Color = SKColors.Gray.WithAlpha(30),
                StrokeWidth = 0.5f * dpiScale,
                IsAntialias = false
            };

            // Linhas verticais
            for (float x = 0; x <= bounds.Right; x += gridSize)
            {
                canvas.DrawLine(x, bounds.Top, x, bounds.Bottom, gridPaint);
            }

            // Linhas horizontais
            for (float y = 0; y <= bounds.Bottom; y += gridSize)
            {
                canvas.DrawLine(bounds.Left, y, bounds.Right, y, gridPaint);
            }
        }

        private void MergeNodes(Node oldNode, Node newNode)
        {
            foreach (var shape in Shapes)
            {
                if (shape is LineShape line && line.ContainsNode(oldNode))
                {
                    line.ReplaceNode(oldNode, newNode);
                }
            }

            AllNodes.Remove(oldNode);
        }

        private void ExecuteDeleteSelection(object? parameter)
        {
            if (SelectedShape == null) return;

            // Remove shape
            Shapes.Remove(SelectedShape);

            // Remove nodes órfãos
            if (SelectedShape is LineShape line)
            {
                CleanupOrphanNodes();
                line.Dispose();
            }

            _eventBus.Publish(new ShapeRemovedEvent { Shape = SelectedShape });
            SelectedShape = null;
            RequestRedraw();
        }

        private void ExecuteClearCanvas(object? parameter)
        {
            Shapes.Clear();
            AllNodes.Clear();
            SelectedShape = null;
            RequestRedraw();
        }

        private void CleanupOrphanNodes()
        {
            var usedNodes = new HashSet<Node>();
            foreach (var shape in Shapes)
            {
                foreach (var node in shape.GetNodes())
                {
                    usedNodes.Add(node);
                }
            }

            var orphans = AllNodes.Where(n => !usedNodes.Contains(n)).ToList();
            foreach (var orphan in orphans)
            {
                AllNodes.Remove(orphan);
            }
        }

        private void RequestRedraw()
        {
            _needsRedraw = true;
            _eventBus.Publish(new CanvasInvalidatedEvent());
        }

        private void OnCanvasInvalidated(CanvasInvalidatedEvent evt)
        {
            _needsRedraw = true;
        }
    }
}