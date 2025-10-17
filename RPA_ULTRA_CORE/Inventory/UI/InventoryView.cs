using System.Windows.Input;
using SkiaSharp;
using RPA_ULTRA_CORE.Inventory.Models;
using RPA_ULTRA_CORE.Inventory.Services;
using RPA_ULTRA_CORE.Plugins.Abstractions;
using RPA_ULTRA_CORE.Resources;

namespace RPA_ULTRA_CORE.Inventory.UI
{
    /// <summary>
    /// View do inventário estilo Minecraft
    /// </summary>
    public class InventoryView
    {
        private readonly IInventoryService _inventoryService;
        private readonly ResourceManager _resourceManager;

        // Layout constants
        private const int COLUMNS = 7;
        private const float SLOT_SIZE = 48f;
        private const float SLOT_PADDING = 4f;
        private const float SECTION_ICON_SIZE = 32f;
        private const float HOTBAR_HEIGHT = 60f;
        private const float HEADER_HEIGHT = 80f;
        private const float SEARCH_HEIGHT = 40f;

        // State
        private bool _isVisible;
        private IInventorySection? _currentSection;
        private List<IInventoryItem> _filteredItems = new();
        private string _searchText = "";
        private int _currentPage = 0;
        private int _totalPages = 1;
        private int _rowsPerPage;

        // Slots
        private List<InventorySlot> _inventorySlots = new();
        private List<InventorySlot> _hotbarSlots = new();
        private List<InventorySlot> _sectionSlots = new();

        // Interaction
        private InventorySlot? _hoveredSlot;
        private InventorySlot? _draggingSlot;
        private SKPoint _dragOffset;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                if (_isVisible)
                {
                    RefreshInventory();
                }
            }
        }

        public event EventHandler<InventoryItemDroppedEventArgs>? ItemDropped;
        public event EventHandler? InventoryClosed;

        public InventoryView(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
            _resourceManager = ResourceManager.Instance;
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            // Initialize hotbar slots (7 slots)
            _hotbarSlots.Clear();
            for (int i = 0; i < 7; i++)
            {
                _hotbarSlots.Add(new InventorySlot
                {
                    Index = i,
                    IsHotbarSlot = true
                });
            }
        }

        /// <summary>
        /// Calcula o layout baseado no tamanho da tela
        /// </summary>
        public void UpdateLayout(SKSize screenSize)
        {
            float centerX = screenSize.Width / 2;
            float inventoryWidth = COLUMNS * (SLOT_SIZE + SLOT_PADDING);
            float startX = centerX - (inventoryWidth / 2);

            // Calcula linhas por página
            float availableHeight = screenSize.Height - HEADER_HEIGHT - SEARCH_HEIGHT - HOTBAR_HEIGHT - 100;
            _rowsPerPage = Math.Max(3, (int)(availableHeight / (SLOT_SIZE + SLOT_PADDING)));

            // Layout das seções (topo)
            _sectionSlots.Clear();
            var sections = _inventoryService.GetSections().ToList();
            float sectionStartX = startX;

            for (int i = 0; i < sections.Count; i++)
            {
                _sectionSlots.Add(new InventorySlot
                {
                    Index = i,
                    Bounds = new SKRect(
                        sectionStartX + i * (SECTION_ICON_SIZE + SLOT_PADDING),
                        20,
                        sectionStartX + i * (SECTION_ICON_SIZE + SLOT_PADDING) + SECTION_ICON_SIZE,
                        20 + SECTION_ICON_SIZE
                    )
                });
            }

            // Layout do grid principal
            _inventorySlots.Clear();
            float inventoryStartY = HEADER_HEIGHT + SEARCH_HEIGHT;

            int totalSlots = COLUMNS * _rowsPerPage;
            for (int i = 0; i < totalSlots; i++)
            {
                int col = i % COLUMNS;
                int row = i / COLUMNS;

                _inventorySlots.Add(new InventorySlot
                {
                    Index = i,
                    Bounds = new SKRect(
                        startX + col * (SLOT_SIZE + SLOT_PADDING),
                        inventoryStartY + row * (SLOT_SIZE + SLOT_PADDING),
                        startX + col * (SLOT_SIZE + SLOT_PADDING) + SLOT_SIZE,
                        inventoryStartY + row * (SLOT_SIZE + SLOT_PADDING) + SLOT_SIZE
                    )
                });
            }

            // Layout da hotbar (bottom)
            float hotbarY = screenSize.Height - HOTBAR_HEIGHT - 20;
            float hotbarStartX = centerX - (7 * (SLOT_SIZE + SLOT_PADDING)) / 2;

            for (int i = 0; i < _hotbarSlots.Count; i++)
            {
                _hotbarSlots[i].Bounds = new SKRect(
                    hotbarStartX + i * (SLOT_SIZE + SLOT_PADDING),
                    hotbarY,
                    hotbarStartX + i * (SLOT_SIZE + SLOT_PADDING) + SLOT_SIZE,
                    hotbarY + SLOT_SIZE
                );
            }

            UpdatePagination();
        }

        /// <summary>
        /// Atualiza itens baseado na seção e filtro
        /// </summary>
        private void RefreshInventory()
        {
            if (_currentSection == null)
            {
                var sections = _inventoryService.GetSections().ToList();
                if (sections.Any())
                {
                    _currentSection = sections.First();
                }
            }

            if (_currentSection != null)
            {
                var allItems = _currentSection.GetItems().ToList();

                // Aplica filtro de pesquisa
                if (!string.IsNullOrWhiteSpace(_searchText))
                {
                    var searchLower = _searchText.ToLower();
                    _filteredItems = allItems.Where(item =>
                        item.Name.ToLower().Contains(searchLower) ||
                        item.Tags.Any(tag => tag.ToLower().Contains(searchLower))
                    ).ToList();
                }
                else
                {
                    _filteredItems = allItems;
                }

                UpdatePagination();
                AssignItemsToSlots();
            }
        }

        /// <summary>
        /// Atualiza paginação
        /// </summary>
        private void UpdatePagination()
        {
            int slotsPerPage = COLUMNS * _rowsPerPage;
            _totalPages = Math.Max(1, (int)Math.Ceiling((float)_filteredItems.Count / slotsPerPage));
            _currentPage = Math.Min(_currentPage, _totalPages - 1);
        }

        /// <summary>
        /// Atribui itens aos slots da página atual
        /// </summary>
        private void AssignItemsToSlots()
        {
            // Limpa slots
            foreach (var slot in _inventorySlots)
            {
                slot.Item = null;
            }

            int slotsPerPage = COLUMNS * _rowsPerPage;
            int startIndex = _currentPage * slotsPerPage;
            int endIndex = Math.Min(startIndex + slotsPerPage, _filteredItems.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                int slotIndex = i - startIndex;
                if (slotIndex < _inventorySlots.Count)
                {
                    _inventorySlots[slotIndex].Item = _filteredItems[i];
                }
            }
        }

        /// <summary>
        /// Desenha o inventário
        /// </summary>
        public void Draw(SKCanvas canvas, float dpiScale)
        {
            if (!IsVisible) return;

            var bounds = canvas.LocalClipBounds;

            // Background overlay
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.Black.WithAlpha(180);
                canvas.DrawRect(bounds, paint);
            }

            // Draw sections
            DrawSections(canvas, dpiScale);

            // Draw search bar
            DrawSearchBar(canvas, dpiScale);

            // Draw pagination
            DrawPagination(canvas, dpiScale);

            // Draw inventory slots
            DrawSlots(canvas, _inventorySlots, dpiScale);

            // Draw hotbar
            DrawHotbar(canvas, dpiScale);

            // Draw dragging item
            if (_draggingSlot?.Item != null)
            {
                DrawDraggingItem(canvas, dpiScale);
            }

            // Draw tooltip
            if (_hoveredSlot?.Item != null && _draggingSlot == null)
            {
                DrawTooltip(canvas, _hoveredSlot.Item, dpiScale);
            }
        }

        private void DrawSections(SKCanvas canvas, float dpiScale)
        {
            var sections = _inventoryService.GetSections().ToList();

            for (int i = 0; i < Math.Min(sections.Count, _sectionSlots.Count); i++)
            {
                var section = sections[i];
                var slot = _sectionSlots[i];

                // Draw section icon
                var icon = _resourceManager.LoadImage(section.IconResource);

                using (var paint = new SKPaint())
                {
                    // Background
                    paint.Color = section == _currentSection ?
                        SKColors.White.WithAlpha(60) :
                        SKColors.White.WithAlpha(20);
                    canvas.DrawRoundRect(slot.Bounds, 4, 4, paint);

                    // Icon
                    if (icon != null)
                    {
                        var destRect = slot.Bounds;
                        destRect.Inflate(-4, -4);
                        canvas.DrawImage(icon, destRect, new SKPaint());
                    }

                    // Hover effect
                    if (slot.IsHovered)
                    {
                        paint.Color = SKColors.White.WithAlpha(40);
                        paint.Style = SKPaintStyle.Stroke;
                        paint.StrokeWidth = 2 * dpiScale;
                        canvas.DrawRoundRect(slot.Bounds, 4, 4, paint);
                    }
                }
            }
        }

        private void DrawSearchBar(SKCanvas canvas, float dpiScale)
        {
            var bounds = canvas.LocalClipBounds;
            float centerX = bounds.MidX;
            float searchY = HEADER_HEIGHT;
            float searchWidth = 300;

            var searchRect = new SKRect(
                centerX - searchWidth / 2,
                searchY,
                centerX + searchWidth / 2,
                searchY + 30
            );

            using (var paint = new SKPaint())
            {
                // Background
                paint.Color = SKColors.Black.WithAlpha(100);
                canvas.DrawRoundRect(searchRect, 4, 4, paint);

                // Border
                paint.Color = SKColors.Gray;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1 * dpiScale;
                canvas.DrawRoundRect(searchRect, 4, 4, paint);

                // Text
                if (!string.IsNullOrEmpty(_searchText))
                {
                    paint.Color = SKColors.White;
                    paint.Style = SKPaintStyle.Fill;
                    paint.TextSize = 14 * dpiScale;
                    paint.IsAntialias = true;

                    canvas.DrawText(_searchText,
                        searchRect.Left + 10,
                        searchRect.MidY + paint.TextSize / 3,
                        paint);
                }
                else
                {
                    paint.Color = SKColors.Gray;
                    paint.Style = SKPaintStyle.Fill;
                    paint.TextSize = 14 * dpiScale;
                    paint.IsAntialias = true;

                    canvas.DrawText("Search...",
                        searchRect.Left + 10,
                        searchRect.MidY + paint.TextSize / 3,
                        paint);
                }
            }
        }

        private void DrawPagination(SKCanvas canvas, float dpiScale)
        {
            if (_totalPages <= 1) return;

            var bounds = canvas.LocalClipBounds;
            float paginationX = bounds.Right - 150;
            float paginationY = HEADER_HEIGHT + 5;

            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.White;
                paint.TextSize = 16 * dpiScale;
                paint.IsAntialias = true;

                string pageText = $"{_currentPage + 1} of {_totalPages}";
                canvas.DrawText(pageText, paginationX, paginationY + paint.TextSize, paint);

                // Draw arrows
                paint.Color = _currentPage > 0 ? SKColors.White : SKColors.Gray;
                canvas.DrawText("<", paginationX - 30, paginationY + paint.TextSize, paint);

                paint.Color = _currentPage < _totalPages - 1 ? SKColors.White : SKColors.Gray;
                canvas.DrawText(">", paginationX + 80, paginationY + paint.TextSize, paint);
            }
        }

        private void DrawSlots(SKCanvas canvas, List<InventorySlot> slots, float dpiScale)
        {
            foreach (var slot in slots)
            {
                DrawSlot(canvas, slot, dpiScale);
            }
        }

        private void DrawSlot(SKCanvas canvas, InventorySlot slot, float dpiScale)
        {
            using (var paint = new SKPaint())
            {
                // Background
                paint.Color = slot.IsSelected ?
                    SKColors.Yellow.WithAlpha(40) :
                    slot.IsHovered ?
                        SKColors.White.WithAlpha(30) :
                        SKColors.Black.WithAlpha(60);

                canvas.DrawRoundRect(slot.Bounds, 4, 4, paint);

                // Border
                paint.Color = SKColors.Gray.WithAlpha(100);
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1 * dpiScale;
                canvas.DrawRoundRect(slot.Bounds, 4, 4, paint);

                // Item icon
                if (slot.Item != null)
                {
                    var icon = _resourceManager.LoadImage(slot.Item.IconResource);
                    if (icon != null)
                    {
                        var iconRect = slot.Bounds;
                        iconRect.Inflate(-8, -8);
                        canvas.DrawImage(icon, iconRect, new SKPaint());
                    }

                    // Only show item name text in hotbar slots
                    if (slot.IsHotbarSlot)
                    {
                        paint.Color = SKColors.White;
                        paint.Style = SKPaintStyle.Fill;
                        paint.TextSize = 10 * dpiScale;
                        paint.IsAntialias = true;

                        var text = slot.Item.Name;
                        if (text.Length > 8)
                            text = text.Substring(0, 8) + "...";

                        var textBounds = new SKRect();
                        paint.MeasureText(text, ref textBounds);

                        canvas.DrawText(text,
                            slot.Bounds.MidX - textBounds.Width / 2,
                            slot.Bounds.Bottom - 2,
                            paint);
                    }
                }

                // Hotkey number for hotbar
                if (slot.IsHotbarSlot && slot.HotkeyNumber.HasValue)
                {
                    paint.Color = SKColors.Yellow;
                    paint.Style = SKPaintStyle.Fill;
                    paint.TextSize = 12 * dpiScale;
                    paint.IsAntialias = true;

                    canvas.DrawText(slot.HotkeyNumber.Value.ToString(),
                        slot.Bounds.Left + 4,
                        slot.Bounds.Top + 12 * dpiScale,
                        paint);
                }
            }
        }

        private void DrawHotbar(SKCanvas canvas, float dpiScale)
        {
            // Draw separator line
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.Gray.WithAlpha(100);
                paint.StrokeWidth = 1 * dpiScale;

                var bounds = canvas.LocalClipBounds;
                float hotbarY = bounds.Height - HOTBAR_HEIGHT - 25;
                canvas.DrawLine(0, hotbarY, bounds.Width, hotbarY, paint);
            }

            // Draw hotbar slots
            DrawSlots(canvas, _hotbarSlots, dpiScale);
        }

        private void DrawDraggingItem(SKCanvas canvas, float dpiScale)
        {
            if (_draggingSlot?.Item == null) return;

            var mousePos = GetMousePosition();
            var icon = _resourceManager.LoadImage(_draggingSlot.Item.IconResource);

            if (icon != null)
            {
                var destRect = new SKRect(
                    mousePos.X - SLOT_SIZE / 2 + _dragOffset.X,
                    mousePos.Y - SLOT_SIZE / 2 + _dragOffset.Y,
                    mousePos.X + SLOT_SIZE / 2 + _dragOffset.X,
                    mousePos.Y + SLOT_SIZE / 2 + _dragOffset.Y
                );

                using (var paint = new SKPaint())
                {
                    paint.Color = SKColors.White.WithAlpha(180);
                    canvas.DrawImage(icon, destRect, paint);
                }
            }
        }

        private void DrawTooltip(SKCanvas canvas, IInventoryItem item, float dpiScale)
        {
            var mousePos = GetMousePosition();

            using (var paint = new SKPaint())
            {
                paint.TextSize = 14 * dpiScale;
                paint.IsAntialias = true;

                // Measure text
                var lines = new List<string> { item.Name };
                if (!string.IsNullOrEmpty(item.Description))
                {
                    var descLines = item.Description.Split('\n').Take(3);
                    lines.AddRange(descLines);
                }

                float maxWidth = 0;
                float lineHeight = paint.TextSize + 4;

                foreach (var line in lines)
                {
                    var bounds = new SKRect();
                    paint.MeasureText(line, ref bounds);
                    maxWidth = Math.Max(maxWidth, bounds.Width);
                }

                float tooltipWidth = maxWidth + 20;
                float tooltipHeight = lines.Count * lineHeight + 10;

                var tooltipRect = new SKRect(
                    mousePos.X + 20,
                    mousePos.Y - tooltipHeight - 10,
                    mousePos.X + 20 + tooltipWidth,
                    mousePos.Y - 10
                );

                // Background
                paint.Color = SKColors.Black.WithAlpha(230);
                paint.Style = SKPaintStyle.Fill;
                canvas.DrawRoundRect(tooltipRect, 4, 4, paint);

                // Border
                paint.Color = SKColors.Gray;
                paint.Style = SKPaintStyle.Stroke;
                paint.StrokeWidth = 1 * dpiScale;
                canvas.DrawRoundRect(tooltipRect, 4, 4, paint);

                // Text
                paint.Style = SKPaintStyle.Fill;
                float y = tooltipRect.Top + lineHeight;

                for (int i = 0; i < lines.Count; i++)
                {
                    paint.Color = i == 0 ? SKColors.White : SKColors.LightGray;
                    canvas.DrawText(lines[i],
                        tooltipRect.Left + 10,
                        y,
                        paint);
                    y += lineHeight;
                }
            }
        }

        /// <summary>
        /// Processa clique do mouse
        /// </summary>
        public void OnMouseDown(SKPoint point, MouseButton button)
        {
            if (!IsVisible) return;

            // Check section clicks
            foreach (var slot in _sectionSlots)
            {
                if (slot.HitTest(point))
                {
                    var sections = _inventoryService.GetSections().ToList();
                    if (slot.Index < sections.Count)
                    {
                        _currentSection = sections[slot.Index];
                        _currentPage = 0;
                        _searchText = "";
                        RefreshInventory();
                    }
                    return;
                }
            }

            // Check pagination clicks
            if (_totalPages > 1)
            {
                var bounds = new SKRect();
                // TODO: Check arrow bounds
                // For now, simple keyboard navigation
            }

            // Check inventory slots
            foreach (var slot in _inventorySlots)
            {
                if (slot.HitTest(point) && slot.Item != null)
                {
                    _draggingSlot = slot;
                    _dragOffset = new SKPoint(
                        slot.Bounds.MidX - point.X,
                        slot.Bounds.MidY - point.Y
                    );
                    return;
                }
            }

            // Check hotbar slots
            foreach (var slot in _hotbarSlots)
            {
                if (slot.HitTest(point))
                {
                    if (_draggingSlot != null && _draggingSlot.Item != null)
                    {
                        // Drop item into hotbar
                        slot.Item = _draggingSlot.Item;
                        _draggingSlot = null;
                    }
                    else if (slot.Item != null)
                    {
                        // Start dragging from hotbar
                        _draggingSlot = slot;
                        _dragOffset = new SKPoint(
                            slot.Bounds.MidX - point.X,
                            slot.Bounds.MidY - point.Y
                        );
                    }
                    return;
                }
            }
        }

        /// <summary>
        /// Processa movimento do mouse
        /// </summary>
        public void OnMouseMove(SKPoint point)
        {
            if (!IsVisible) return;

            _hoveredSlot = null;

            // Update hover states
            foreach (var slot in _sectionSlots)
            {
                slot.IsHovered = slot.HitTest(point);
            }

            foreach (var slot in _inventorySlots)
            {
                slot.IsHovered = slot.HitTest(point);
                if (slot.IsHovered && slot.Item != null)
                {
                    _hoveredSlot = slot;
                }
            }

            foreach (var slot in _hotbarSlots)
            {
                slot.IsHovered = slot.HitTest(point);
                if (slot.IsHovered && slot.Item != null)
                {
                    _hoveredSlot = slot;
                }
            }
        }

        /// <summary>
        /// Processa soltar o mouse
        /// </summary>
        public void OnMouseUp(SKPoint point)
        {
            if (!IsVisible) return;

            if (_draggingSlot?.Item != null)
            {
                // Check if dropped on canvas (outside inventory)
                bool droppedOnInventory = false;

                // Check hotbar slots
                foreach (var slot in _hotbarSlots)
                {
                    if (slot.HitTest(point))
                    {
                        slot.Item = _draggingSlot.Item;
                        droppedOnInventory = true;
                        break;
                    }
                }

                // If not dropped on inventory, fire event for canvas drop
                if (!droppedOnInventory)
                {
                    ItemDropped?.Invoke(this, new InventoryItemDroppedEventArgs
                    {
                        Item = _draggingSlot.Item,
                        DropPosition = point
                    });
                }

                _draggingSlot = null;
            }
        }

        /// <summary>
        /// Processa tecla pressionada
        /// </summary>
        public bool OnKeyDown(Key key)
        {
            if (!IsVisible) return false;

            switch (key)
            {
                case Key.E:
                case Key.Escape:
                    IsVisible = false;
                    InventoryClosed?.Invoke(this, EventArgs.Empty);
                    return true;

                case Key.D1:
                case Key.D2:
                case Key.D3:
                case Key.D4:
                case Key.D5:
                case Key.D6:
                case Key.D7:
                    int slotIndex = (int)key - (int)Key.D1;
                    if (_hoveredSlot?.Item != null && slotIndex < _hotbarSlots.Count)
                    {
                        _hotbarSlots[slotIndex].Item = _hoveredSlot.Item;
                    }
                    return true;

                case Key.Left:
                    if (_currentPage > 0)
                    {
                        _currentPage--;
                        AssignItemsToSlots();
                    }
                    return true;

                case Key.Right:
                    if (_currentPage < _totalPages - 1)
                    {
                        _currentPage++;
                        AssignItemsToSlots();
                    }
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Processa entrada de texto (para pesquisa)
        /// </summary>
        public void OnTextInput(string text)
        {
            if (!IsVisible) return;

            _searchText += text;
            RefreshInventory();
        }

        /// <summary>
        /// Processa backspace (para pesquisa)
        /// </summary>
        public void OnBackspace()
        {
            if (!IsVisible || string.IsNullOrEmpty(_searchText)) return;

            _searchText = _searchText.Substring(0, _searchText.Length - 1);
            RefreshInventory();
        }

        /// <summary>
        /// Obtém item do slot da hotbar
        /// </summary>
        public IInventoryItem? GetHotbarItem(int index)
        {
            if (index >= 0 && index < _hotbarSlots.Count)
            {
                return _hotbarSlots[index].Item;
            }
            return null;
        }

        /// <summary>
        /// Define item no slot da hotbar
        /// </summary>
        public void SetHotbarItem(int index, IInventoryItem? item)
        {
            if (index >= 0 && index < _hotbarSlots.Count)
            {
                _hotbarSlots[index].Item = item;
            }
        }

        private SKPoint GetMousePosition()
        {
            // This should be replaced with actual mouse position tracking
            return new SKPoint(0, 0);
        }
    }

    public class InventoryItemDroppedEventArgs : EventArgs
    {
        public IInventoryItem Item { get; set; } = null!;
        public SKPoint DropPosition { get; set; }
    }
}