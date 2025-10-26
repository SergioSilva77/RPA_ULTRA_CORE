using System.Collections.Generic;
using System.Composition;
using RPA_ULTRA_CORE.Inventory.Abstractions;
using RPA_ULTRA_CORE.Inventory.Items;
using RPA_ULTRA_CORE.Plugins.Abstractions;

namespace RPA_ULTRA_CORE.Plugins.Data
{
    [Export(typeof(IPlugin))]
    public class DataPlugin : IPlugin
    {
        public string Id => "plugin.data";

        public string Name => "Data Management";

        public string Version => "1.0.0";

        public string Description => "Provides data variables that can flow through connections";

        public IEnumerable<IInventorySection> GetSections()
        {
            yield return new DataInventorySection();
        }
    }

    /// <summary>
    /// Seção do inventário para itens de dados
    /// </summary>
    public class DataInventorySection : IInventorySection
    {
        public string Id => "section.data";

        public string Name => "Data";

        public string IconResource => "embedded:data_section_icon.png";

        public IEnumerable<IInventoryItem> GetItems()
        {
            yield return new VariableInventoryItem();
        }

        public void RenderSectionIcon(SkiaSharp.SKCanvas canvas, float x, float y, float size)
        {
            // Desenha um ícone de "database" ou "variável" para a seção
            using (var paint = new SkiaSharp.SKPaint
            {
                Color = SkiaSharp.SKColors.DodgerBlue,
                Style = SkiaSharp.SKPaintStyle.Stroke,
                StrokeWidth = 2.5f,
                IsAntialias = true,
                StrokeCap = SkiaSharp.SKStrokeCap.Round
            })
            {
                var centerX = x + size / 2;
                var centerY = y + size / 2;
                var width = size * 0.6f;
                var height = size * 0.5f;

                // Desenha um cilindro (database)
                var ellipseHeight = height * 0.25f;
                
                // Elipse superior
                var topRect = new SkiaSharp.SKRect(
                    centerX - width / 2,
                    centerY - height / 2,
                    centerX + width / 2,
                    centerY - height / 2 + ellipseHeight
                );
                canvas.DrawOval(topRect, paint);

                // Linhas laterais
                canvas.DrawLine(
                    centerX - width / 2, centerY - height / 2 + ellipseHeight / 2,
                    centerX - width / 2, centerY + height / 2 - ellipseHeight / 2,
                    paint
                );
                canvas.DrawLine(
                    centerX + width / 2, centerY - height / 2 + ellipseHeight / 2,
                    centerX + width / 2, centerY + height / 2 - ellipseHeight / 2,
                    paint
                );

                // Elipse inferior
                var bottomRect = new SkiaSharp.SKRect(
                    centerX - width / 2,
                    centerY + height / 2 - ellipseHeight,
                    centerX + width / 2,
                    centerY + height / 2
                );
                canvas.DrawOval(bottomRect, paint);
            }
        }
    }
}
