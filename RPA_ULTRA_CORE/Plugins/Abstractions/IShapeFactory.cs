using RPA_ULTRA_CORE.Models.Geometry;
using SkiaSharp;

namespace RPA_ULTRA_CORE.Plugins.Abstractions
{
    /// <summary>
    /// Factory para criar formas no canvas
    /// </summary>
    public interface IShapeFactory
    {
        /// <summary>
        /// ID do tipo de forma que esta factory cria
        /// </summary>
        string ShapeTypeId { get; }

        /// <summary>
        /// Cria uma nova instância da forma
        /// </summary>
        BaseShape Create(SKPoint position, object? defaults);

        /// <summary>
        /// Obtém o tamanho padrão para esta forma
        /// </summary>
        SKSize GetDefaultSize();
    }
}