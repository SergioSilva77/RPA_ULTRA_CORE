using RPA_ULTRA_CORE.Plugins.Abstractions;
using RPA_ULTRA_CORE.Plugins.Runtime;

namespace RPA_ULTRA_CORE.Inventory.Services
{
    /// <summary>
    /// Implementação do serviço de inventário
    /// </summary>
    public class InventoryService : IInventoryService
    {
        private readonly PluginLoader _pluginLoader;
        private List<IInventorySection> _sections = new();

        public InventoryService()
        {
            _pluginLoader = new PluginLoader();
            _ = LoadPluginsAsync();
        }

        public InventoryService(string pluginsDirectory)
        {
            _pluginLoader = new PluginLoader(pluginsDirectory);
            _ = LoadPluginsAsync();
        }

        private async Task LoadPluginsAsync()
        {
            await _pluginLoader.LoadPluginsAsync();
            RefreshSections();
        }

        private void RefreshSections()
        {
            _sections = _pluginLoader.GetAllSections().ToList();
        }

        public IEnumerable<IInventorySection> GetSections()
        {
            return _sections;
        }

        public IInventorySection? GetSection(string sectionId)
        {
            return _sections.FirstOrDefault(s => s.Id == sectionId);
        }

        public IEnumerable<IInventoryItem> SearchItems(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return _sections.SelectMany(s => s.GetItems());
            }

            var searchLower = query.ToLower();
            return _sections
                .SelectMany(s => s.GetItems())
                .Where(item =>
                    item.Name.ToLower().Contains(searchLower) ||
                    item.Tags.Any(tag => tag.ToLower().Contains(searchLower))
                );
        }

        public async Task ReloadPluginsAsync()
        {
            await _pluginLoader.LoadPluginsAsync();
            RefreshSections();
        }

        public void Dispose()
        {
            _pluginLoader?.Dispose();
        }
    }
}