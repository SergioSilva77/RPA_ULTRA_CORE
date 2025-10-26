using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SkiaSharp;
using RPA_ULTRA_CORE.Models.Geometry;
using RPA_ULTRA_CORE.Services;
using RPA_ULTRA_CORE.Utils;
using RPA_ULTRA_CORE.Helpers;
using RPA_ULTRA_CORE.Inventory.UI;
using RPA_ULTRA_CORE.Inventory.Services;
using RPA_ULTRA_CORE.Plugins.Abstractions;
using RPA_ULTRA_CORE.Views;

namespace RPA_ULTRA_CORE.ViewModels
{
    /// <summary>
    /// ViewModel principal para o editor de desenho vetorial
    /// </summary>
    public class SketchViewModel : ViewModelBase
    {
        private readonly SnapService _snapService;
        private readonly EventBus _eventBus;
        private readonly IInventoryService _inventoryService;
        private readonly InventoryView _inventoryView;

        private ToolState _currentToolState = ToolState.Idle;
        private BaseShape? _selectedShape;
        private Node? _draggingNode;
        private Node? _tempStartNode;
        private SKPoint _dragStartPoint;
        private SKPoint _dragOffset;
        private bool _isShiftPressed;
        private bool _needsRedraw = true;
        private LineSnapResult? _currentSnapPreview;
        private ShapeSnapResult? _currentShapeSnapPreview;
        private SKPoint _currentMousePosition;
        private bool _isPreviewingLine;
        private int _selectedHotbarSlot = -1;
        private SKSize _canvasSize;

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

            // Initialize inventory
            _inventoryService = new InventoryService();
            _inventoryView = new InventoryView(_inventoryService);
            _inventoryView.ItemDropped += OnInventoryItemDropped;
            _inventoryView.InventoryClosed += OnInventoryClosed;

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
            // Se inventário visível, delega para ele
            if (_inventoryView.IsVisible)
            {
                _inventoryView.OnMouseDown(point, MouseButton.Left);
                RequestRedraw();
                return;
            }

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
            _currentMousePosition = point;

            // Se inventário visível, delega para ele
            if (_inventoryView.IsVisible)
            {
                _inventoryView.OnMouseMove(point);
                RequestRedraw();
                return;
            }

            // Se SHIFT pressionado e não está desenhando, mostra preview
            if (IsShiftPressed && CurrentToolState == ToolState.Idle)
            {
                _isPreviewingLine = true;
                RequestRedraw();
            }

            switch (CurrentToolState)
            {
                case ToolState.DrawingLine:
                    // Detecta snap em shape primeiro
                    _currentShapeSnapPreview = _snapService.DetectShapeSnap(point, Shapes);

                    if (_currentShapeSnapPreview?.CanSnap == true)
                    {
                        _currentMousePosition = _currentShapeSnapPreview.SnapPoint;
                    }
                    else
                    {
                        // Fallback para snap tradicional
                        _currentMousePosition = _snapService.Snap(point, AllNodes);
                    }
                    RequestRedraw();
                    break;

                case ToolState.DraggingHandle:
                    if (_draggingNode != null)
                    {
                        // Marca que o usuário está movendo
                        _draggingNode.BeginUserMove();

                        // Detecta snap em shape primeiro
                        _currentShapeSnapPreview = _snapService.DetectShapeSnap(point, Shapes, SelectedShape);

                        if (_currentShapeSnapPreview?.CanSnap == true)
                        {
                            _draggingNode.Set(_currentShapeSnapPreview.SnapPoint.X, _currentShapeSnapPreview.SnapPoint.Y);
                        }
                        else
                        {
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
            // Se inventário visível, delega para ele
            if (_inventoryView.IsVisible)
            {
                _inventoryView.OnMouseUp(point);
                RequestRedraw();
                return;
            }

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
                    // Tenta conectar com shape, linha ou node ao soltar
                    if (_draggingNode != null)
                    {
                        if (_currentShapeSnapPreview?.CanSnap == true &&
                            _currentShapeSnapPreview.TargetShape != null &&
                            _currentShapeSnapPreview.Anchor != null)
                        {
                            // Conecta ao shape (perímetro ou centro)
                            _draggingNode.AttachToShape(_currentShapeSnapPreview.TargetShape, _currentShapeSnapPreview.Anchor);
                        }
                        else if (_currentSnapPreview != null)
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
                        else
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
                    }
                    _draggingNode = null;
                    _currentSnapPreview = null;
                    _currentShapeSnapPreview = null;
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
            // Se inventário visível, delega para ele primeiro
            if (_inventoryView.IsVisible)
            {
                if (_inventoryView.OnKeyDown(key))
                {
                    RequestRedraw();
                    return;
                }
            }

            switch (key)
            {
                case Key.E:
                    // Toggle inventário
                    _inventoryView.IsVisible = !_inventoryView.IsVisible;
                    if (_inventoryView.IsVisible)
                    {
                        _inventoryView.UpdateLayout(_canvasSize);
                        // Pausa edição do canvas
                        CurrentToolState = ToolState.Idle;
                        _isPreviewingLine = false;
                    }
                    RequestRedraw();
                    break;

                case Key.LeftShift:
                case Key.RightShift:
                    IsShiftPressed = true;
                    if (CurrentToolState == ToolState.Idle && !_inventoryView.IsVisible)
                    {
                        // Ativa preview de linha
                        _isPreviewingLine = true;
                        RequestRedraw();
                    }
                    break;

                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                    // Teclas de hotbar (quando inventário fechado)
                    if (!_inventoryView.IsVisible)
                    {
                        int slotIndex = (int)key - (int)Key.D1;
                        SelectHotbarSlot(slotIndex);
                    }
                    break;

                case Key.Delete:
                    ExecuteDeleteSelection(null);
                    break;

                case Key.Escape:
                    if (_inventoryView.IsVisible)
                    {
                        _inventoryView.IsVisible = false;
                    }
                    else
                    {
                        CurrentToolState = ToolState.Idle;
                        _tempStartNode = null;
                        _draggingNode = null;
                    }
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
                    _isPreviewingLine = false;
                    if (CurrentToolState == ToolState.DrawingLine)
                    {
                        CurrentToolState = ToolState.Idle;
                        _tempStartNode = null;
                    }
                    RequestRedraw();
                    break;
            }
        }

        /// <summary>
        /// Desenha o canvas
        /// </summary>
        public void Draw(SKCanvas canvas, float dpiScale)
        {
            // Atualiza tamanho do canvas
            _canvasSize = new SKSize(canvas.LocalClipBounds.Width, canvas.LocalClipBounds.Height);

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
                DrawLinePreview(canvas, dpiScale,
                    new SKPoint((float)_tempStartNode.X, (float)_tempStartNode.Y),
                    _currentMousePosition,
                    false);
            }

            // Desenha preview contínua com SHIFT pressionado (antes de começar a desenhar)
            if (_isPreviewingLine && CurrentToolState == ToolState.Idle && IsShiftPressed)
            {
                var snappedPoint = _snapService.Snap(_currentMousePosition, AllNodes);
                DrawLinePreview(canvas, dpiScale,
                    _currentMousePosition,
                    snappedPoint,
                    true);
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

            // Desenha indicador de snap em shape
            if (_currentShapeSnapPreview?.CanSnap == true)
            {
                using var snapPaint = new SKPaint
                {
                    Color = SKColors.Cyan,
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

                // Desenha um círculo indicando o ponto de snap no shape
                canvas.DrawCircle(_currentShapeSnapPreview.SnapPoint, 10f * dpiScale, snapPaint);
                canvas.DrawCircle(_currentShapeSnapPreview.SnapPoint, 10f * dpiScale, outlinePaint);

                // Indicador adicional para tipo de âncora
                var anchor = _currentShapeSnapPreview.Anchor;
                if (anchor != null)
                {
                    using var textPaint = new SKPaint
                    {
                        Color = SKColors.White,
                        TextSize = 10f * dpiScale,
                        IsAntialias = true
                    };

                    string anchorText = anchor.Kind switch
                    {
                        AnchorKind.Center => "C",
                        AnchorKind.Corner => "◆",
                        AnchorKind.EdgeMid => "│",
                        AnchorKind.Perimeter => "●",
                        _ => ""
                    };

                    if (!string.IsNullOrEmpty(anchorText))
                    {
                        canvas.DrawText(anchorText,
                            _currentShapeSnapPreview.SnapPoint.X - 4f * dpiScale,
                            _currentShapeSnapPreview.SnapPoint.Y + 3f * dpiScale,
                            textPaint);
                    }
                }

                // Desenha linha conectora fantasma
                if (_draggingNode != null)
                {
                    using var linePaint = new SKPaint
                    {
                        Color = SKColors.Cyan.WithAlpha(128),
                        StrokeWidth = 1.5f * dpiScale,
                        PathEffect = SKPathEffect.CreateDash(new[] { 3f, 3f }, 0),
                        IsAntialias = true
                    };

                    canvas.DrawLine(
                        (float)_draggingNode.X, (float)_draggingNode.Y,
                        _currentShapeSnapPreview.SnapPoint.X, _currentShapeSnapPreview.SnapPoint.Y,
                        linePaint);
                }
            }

            // Desenha hotbar sempre visível (mesmo com inventário fechado)
            DrawHotbar(canvas, dpiScale);

            // Desenha inventário se visível
            _inventoryView.Draw(canvas, dpiScale);
        }

        /// <summary>
        /// Desenha preview de linha com opacidade e handles fantasmas
        /// </summary>
        private void DrawLinePreview(SKCanvas canvas, float dpiScale, SKPoint start, SKPoint end, bool showHandles)
        {
            // Linha com opacidade reduzida (35% conforme spec)
            using var linePaint = new SKPaint
            {
                Color = SKColors.White.WithAlpha((byte)(255 * 0.35f)),
                StrokeWidth = 2f * dpiScale,
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            canvas.DrawLine(start, end, linePaint);

            // Handles fantasmas
            if (showHandles)
            {
                using var handlePaint = new SKPaint
                {
                    Color = SKColors.Yellow.WithAlpha((byte)(255 * 0.35f)),
                    IsAntialias = true,
                    Style = SKPaintStyle.Fill
                };

                using var handleBorderPaint = new SKPaint
                {
                    Color = SKColors.Black.WithAlpha((byte)(255 * 0.35f)),
                    StrokeWidth = 1f * dpiScale,
                    IsAntialias = true,
                    Style = SKPaintStyle.Stroke
                };

                float handleRadius = 6f * dpiScale;

                // Handle no início
                canvas.DrawCircle(start, handleRadius, handlePaint);
                canvas.DrawCircle(start, handleRadius, handleBorderPaint);

                // Handle no fim
                canvas.DrawCircle(end, handleRadius, handlePaint);
                canvas.DrawCircle(end, handleRadius, handleBorderPaint);
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

        /// <summary>
        /// Desenha a hotbar na parte inferior
        /// </summary>
        private void DrawHotbar(SKCanvas canvas, float dpiScale)
        {
            if (_inventoryView.IsVisible) return; // Hotbar já é desenhada pelo inventário

            var bounds = canvas.LocalClipBounds;
            float hotbarY = bounds.Height - 80;
            float slotSize = 48f;
            float padding = 4f;
            float totalWidth = 7 * (slotSize + padding);
            float startX = bounds.MidX - totalWidth / 2;

            using (var paint = new SKPaint())
            {
                // Background da hotbar
                paint.Color = SKColors.Black.WithAlpha(100);
                var hotbarBounds = new SKRect(
                    startX - 10,
                    hotbarY - 10,
                    startX + totalWidth + 10,
                    hotbarY + slotSize + 10
                );
                canvas.DrawRoundRect(hotbarBounds, 8, 8, paint);

                // Desenha os 7 slots
                for (int i = 0; i < 7; i++)
                {
                    float x = startX + i * (slotSize + padding);
                    var slotBounds = new SKRect(x, hotbarY, x + slotSize, hotbarY + slotSize);

                    // Background do slot
                    paint.Color = i == _selectedHotbarSlot ?
                        SKColors.Yellow.WithAlpha(40) :
                        SKColors.Black.WithAlpha(60);
                    canvas.DrawRoundRect(slotBounds, 4, 4, paint);

                    // Borda do slot
                    paint.Color = i == _selectedHotbarSlot ?
                        SKColors.Yellow.WithAlpha(100) :
                        SKColors.Gray.WithAlpha(100);
                    paint.Style = SKPaintStyle.Stroke;
                    paint.StrokeWidth = 1 * dpiScale;
                    canvas.DrawRoundRect(slotBounds, 4, 4, paint);

                    // Número do slot
                    paint.Color = SKColors.Yellow;
                    paint.Style = SKPaintStyle.Fill;
                    paint.TextSize = 12 * dpiScale;
                    paint.IsAntialias = true;
                    canvas.DrawText((i + 1).ToString(),
                        x + 4,
                        hotbarY + 12 * dpiScale,
                        paint);

                    // Item icon se houver
                    var item = _inventoryView.GetHotbarItem(i);
                    if (item != null)
                    {
                        // Aqui poderia desenhar o ícone do item
                        paint.Color = SKColors.White;
                        paint.TextSize = 10 * dpiScale;
                        var text = item.Name.Length > 6 ?
                            item.Name.Substring(0, 6) + "..." :
                            item.Name;
                        canvas.DrawText(text,
                            x + 4,
                            hotbarY + slotSize - 4,
                            paint);
                    }
                }
            }
        }

        /// <summary>
        /// Seleciona um slot da hotbar
        /// </summary>
        private void SelectHotbarSlot(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < 7)
            {
                _selectedHotbarSlot = slotIndex;
                var item = _inventoryView.GetHotbarItem(slotIndex);

                if (item != null)
                {
                    // Aqui poderia ativar a ferramenta correspondente
                    // Por enquanto apenas marca como selecionado
                }

                RequestRedraw();
            }
        }

        /// <summary>
        /// Callback quando item é dropado do inventário
        /// </summary>
        private void OnInventoryItemDropped(object? sender, InventoryItemDroppedEventArgs e)
        {
            // Implementar criação do item no canvas
            var snappedPos = _snapService.Snap(e.DropPosition, AllNodes);

            switch (e.Item.Type)
            {
                case InventoryItemType.ShapeBlueprint:
                    // Cria forma com tamanho padrão
                    CreateShapeFromBlueprint(e.Item, snappedPos);
                    break;

                case InventoryItemType.StepImage:
                    // Cria imagem/step
                    CreateStepImage(e.Item, snappedPos);
                    break;

                case InventoryItemType.Action:
                    // Executa ação
                    ExecuteItemAction(e.Item, snappedPos);
                    break;
            }

            RequestRedraw();
        }

        /// <summary>
        /// Cria forma a partir de blueprint
        /// </summary>
        private void CreateShapeFromBlueprint(IInventoryItem item, SKPoint position)
        {
            BaseShape? shape = null;

            // Se o item é um VariableInventoryItem ou tem CreateShape
            if (item is Inventory.Items.VariableInventoryItem varItem)
            {
                shape = varItem.CreateShape(position);
            }
            // Formas padrão
            else if (item.Id.Contains("rect"))
            {
                shape = new RectShape(position.X - 50, position.Y - 25, 100, 50);
            }
            else if (item.Id.Contains("circle"))
            {
                shape = new CircleShape(position.X, position.Y, 30);
            }

            if (shape != null)
            {
                // Se for VariableShape, define a referência ao ViewModel
                if (shape is VariableShape varShape)
                {
                    varShape.ViewModel = this;
                }

                Shapes.Add(shape);
            }
        }

        /// <summary>
        /// Cria imagem/step no canvas
        /// </summary>
        private void CreateStepImage(IInventoryItem item, SKPoint position)
        {
            // TODO: Implementar criação de StepImage
            // Por enquanto cria um placeholder
        }

        /// <summary>
        /// Executa ação do item
        /// </summary>
        private void ExecuteItemAction(IInventoryItem item, SKPoint position)
        {
            if (item.Action != null)
            {
                var context = new CanvasContext
                {
                    DropPosition = position,
                    DpiScale = 1.0f,
                    UserData = this
                };
                item.Action(context);
            }
        }

        /// <summary>
        /// Callback quando inventário é fechado
        /// </summary>
        private void OnInventoryClosed(object? sender, EventArgs e)
        {
            // Retoma edição normal do canvas
            CurrentToolState = ToolState.Idle;
            RequestRedraw();
        }

        #region Variable Support Methods

        /// <summary>
        /// Manipula duplo clique no canvas
        /// </summary>
        public void OnMouseDoubleClick(SKPoint point)
        {
            // Tenta manipular duplo clique em variável
            HandleVariableDoubleClick(point);
        }

        /// <summary>
        /// Manipula o duplo clique em variáveis para abrir o editor
        /// </summary>
        private void HandleVariableDoubleClick(SKPoint point)
        {
            // Encontra a variável clicada
            var clickedVariable = Shapes
                .OfType<VariableShape>()
                .FirstOrDefault(v => v.HitTestPoint(point));

            if (clickedVariable != null)
            {
                OpenVariableEditor(clickedVariable);
            }
        }

        /// <summary>
        /// Abre o editor de variáveis
        /// </summary>
        public void OpenVariableEditor(VariableShape variableShape)
        {
            var dialog = new VariableEditorDialog(variableShape)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                // A variável já foi atualizada e propagada no diálogo
                // Força o redesenho
                RequestRedraw();
            }
        }

        /// <summary>
        /// Encontra uma shape pelo nó
        /// </summary>
        public BaseShape? FindShapeByNode(Node node)
        {
            if (node == null) return null;

            // Procura em todas as shapes
            foreach (var shape in Shapes)
            {
                if (shape is VariableShape varShape)
                {
                    if (varShape.CenterNode == node)
                        return varShape;
                }
                else if (shape is RectShape rectShape)
                {
                    var nodes = rectShape.GetNodes();
                    if (nodes.Contains(node))
                        return rectShape;
                }
                else if (shape is LineShape lineShape)
                {
                    if (lineShape.Start == node || lineShape.End == node)
                        return lineShape;
                }
                else if (shape is CircleShape circleShape)
                {
                    // CircleShape não tem CenterNode no sistema atual
                    var nodes = circleShape.GetNodes();
                    if (nodes.Contains(node))
                        return circleShape;
                }
            }

            return null;
        }

        /// <summary>
        /// Propaga todas as variáveis do canvas
        /// </summary>
        public void PropagateAllVariables()
        {
            // Limpa todas as variáveis recebidas
            foreach (var varShape in Shapes.OfType<VariableShape>())
            {
                varShape.IncomingVariables.Clear();
            }

            // Propaga cada variável
            foreach (var varShape in Shapes.OfType<VariableShape>())
            {
                varShape.PropagateVariable();
            }

            RequestRedraw();
        }

        /// <summary>
        /// Retorna informações de debug sobre variáveis
        /// </summary>
        public string GetVariablesDebugInfo()
        {
            var variables = Shapes.OfType<VariableShape>().ToList();
            if (!variables.Any())
                return "No variables in canvas";

            var info = $"Total Variables: {variables.Count}\n\n";

            foreach (var varShape in variables)
            {
                info += $"Variable: {varShape.VariableName}\n";
                info += $"  Value: {varShape.VariableValue}\n";
                info += $"  Position: ({varShape.Position.X:F0}, {varShape.Position.Y:F0})\n";
                info += $"  Connections: 0\n"; // TODO: implementar contagem de conexões

                if (varShape.IncomingVariables.Any())
                {
                    info += "  Incoming Variables:\n";
                    foreach (var kvp in varShape.IncomingVariables)
                    {
                        info += $"    {kvp.Key} = {kvp.Value}\n";
                    }
                }

                info += "\n";
            }

            return info;
        }

        #endregion
    }
}