using RPA_ULTRA_CORE.Plugins.Abstractions;

namespace RPA_ULTRA_CORE.Inventory.Services
{
    /// <summary>
    /// Serviço para gerenciar o inventário
    /// </summary>
    public interface IInventoryService
    {
        /// <summary>
        /// Obtém todas as seções disponíveis
        /// </summary>
        IEnumerable<IInventorySection> GetSections();

        /// <summary>
        /// Obtém uma seção por ID
        /// </summary>
        IInventorySection? GetSection(string sectionId);

        /// <summary>
        /// Busca itens por nome ou tags
        /// </summary>
        IEnumerable<IInventoryItem> SearchItems(string query);

        /// <summary>
        /// Recarrega os plugins
        /// </summary>
        Task ReloadPluginsAsync();
    }
}