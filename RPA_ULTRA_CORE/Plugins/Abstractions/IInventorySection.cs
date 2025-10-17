namespace RPA_ULTRA_CORE.Plugins.Abstractions
{
    /// <summary>
    /// Interface para uma seção do inventário
    /// </summary>
    public interface IInventorySection
    {
        /// <summary>
        /// ID único da seção
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Nome de exibição da seção
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Recurso do ícone (embedded ou caminho)
        /// </summary>
        string IconResource { get; }

        /// <summary>
        /// Ordem de exibição (menor aparece primeiro)
        /// </summary>
        int DisplayOrder { get; }

        /// <summary>
        /// Retorna todos os itens desta seção
        /// </summary>
        IEnumerable<IInventoryItem> GetItems();
    }
}