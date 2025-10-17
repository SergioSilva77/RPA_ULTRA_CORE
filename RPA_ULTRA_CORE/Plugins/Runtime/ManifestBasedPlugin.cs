using RPA_ULTRA_CORE.Plugins.Abstractions;

namespace RPA_ULTRA_CORE.Plugins.Runtime
{
    /// <summary>
    /// Plugin criado a partir de um manifesto JSON
    /// </summary>
    public class ManifestBasedPlugin : IPlugin
    {
        private readonly PluginManifest _manifest;
        private readonly List<IInventorySection> _sections;

        public string Id => _manifest.Id;
        public string Name => _manifest.Name;
        public string Version => _manifest.Version;

        public ManifestBasedPlugin(PluginManifest manifest)
        {
            _manifest = manifest;
            _sections = new List<IInventorySection>();
            InitializeSections();
        }

        private void InitializeSections()
        {
            foreach (var section in _manifest.Sections)
            {
                _sections.Add(new ManifestBasedSection(section));
            }
        }

        public IEnumerable<IInventorySection> GetSections()
        {
            return _sections;
        }
    }

    /// <summary>
    /// Seção criada a partir de manifesto
    /// </summary>
    public class ManifestBasedSection : IInventorySection
    {
        private readonly ManifestSection _section;
        private readonly List<IInventoryItem> _items;

        public string Id => _section.Id;
        public string Name => _section.Name;
        public string IconResource => _section.Icon;
        public int DisplayOrder => _section.DisplayOrder;

        public ManifestBasedSection(ManifestSection section)
        {
            _section = section;
            _items = new List<IInventoryItem>();
            InitializeItems();
        }

        private void InitializeItems()
        {
            foreach (var item in _section.Items)
            {
                _items.Add(new ManifestBasedItem(item));
            }
        }

        public IEnumerable<IInventoryItem> GetItems()
        {
            return _items;
        }
    }

    /// <summary>
    /// Item criado a partir de manifesto
    /// </summary>
    public class ManifestBasedItem : IInventoryItem
    {
        private readonly ManifestItem _item;

        public string Id => _item.Id;
        public string Name => _item.Name;
        public string Description => _item.Description;
        public string IconResource => _item.Icon;
        public string[] Tags => _item.Tags;
        public object? Defaults => _item.Defaults;
        public Action<CanvasContext>? Action => null; // Manifestos não suportam ações diretas

        public InventoryItemType Type
        {
            get
            {
                return _item.Type?.ToLower() switch
                {
                    "shapeblueprint" => InventoryItemType.ShapeBlueprint,
                    "stepimage" => InventoryItemType.StepImage,
                    "action" => InventoryItemType.Action,
                    _ => InventoryItemType.ShapeBlueprint
                };
            }
        }

        public ManifestBasedItem(ManifestItem item)
        {
            _item = item;
        }
    }
}