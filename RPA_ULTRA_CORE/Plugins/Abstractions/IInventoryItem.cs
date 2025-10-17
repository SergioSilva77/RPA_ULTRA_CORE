using SkiaSharp;

namespace RPA_ULTRA_CORE.Plugins.Abstractions
{
    /// <summary>
    /// Tipo de item do inventário
    /// </summary>
    public enum InventoryItemType
    {
        ShapeBlueprint,  // Cria uma forma no canvas
        StepImage,       // Cria uma imagem/passo no canvas
        Action           // Executa uma ação
    }

    /// <summary>
    /// Interface para um item do inventário
    /// </summary>
    public interface IInventoryItem
    {
        /// <summary>
        /// ID único do item
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Nome de exibição
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Descrição (aparece no tooltip)
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Tipo do item
        /// </summary>
        InventoryItemType Type { get; }

        /// <summary>
        /// Tags para busca
        /// </summary>
        string[] Tags { get; }

        /// <summary>
        /// Recurso do ícone
        /// </summary>
        string IconResource { get; }

        /// <summary>
        /// Configurações padrão ao criar (JSON serializable)
        /// </summary>
        object? Defaults { get; }

        /// <summary>
        /// Ação a executar (se Type == Action)
        /// </summary>
        Action<CanvasContext>? Action { get; }
    }

    /// <summary>
    /// Contexto do canvas para execução de ações
    /// </summary>
    public class CanvasContext
    {
        public SKPoint DropPosition { get; set; }
        public float DpiScale { get; set; }
        public object? UserData { get; set; }
    }
}