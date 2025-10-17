using System.Composition;

namespace RPA_ULTRA_CORE.Plugins.Abstractions
{
    /// <summary>
    /// Interface principal para plugins do sistema
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// ID único do plugin
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Nome de exibição do plugin
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Versão do plugin
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Retorna todas as seções de inventário fornecidas pelo plugin
        /// </summary>
        IEnumerable<IInventorySection> GetSections();
    }
}