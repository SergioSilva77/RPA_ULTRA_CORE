using System.Linq;
using System.Windows;
using RPA_ULTRA_CORE.Models.Geometry;
using RPA_ULTRA_CORE.Views;
using SkiaSharp;

namespace RPA_ULTRA_CORE.ViewModels
{
    /// <summary>
    /// Extensões para o SketchViewModel para suporte a variáveis
    /// </summary>
    public partial class SketchViewModel
    {
        /// <summary>
        /// Manipula o duplo clique em variáveis para abrir o editor
        /// </summary>
        private void HandleVariableDoubleClick(SKPoint point)
        {
            // Encontra a variável clicada
            var clickedVariable = Shapes
                .OfType<VariableShape>()
                .FirstOrDefault(v => v.HitTest(point));

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
                Owner = Application.Current.MainWindow
            };

            if (dialog.ShowDialog() == true)
            {
                // A variável já foi atualizada e propagada no diálogo
                // Força o redesenho
                InvalidateCanvas();
            }
        }

        /// <summary>
        /// Encontra uma shape pelo nó
        /// </summary>
        public BaseShape FindShapeByNode(Node node)
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
                else if (shape is CircleShape circleShape)
                {
                    if (circleShape.CenterNode == node)
                        return circleShape;
                }
                else if (shape is RectShape rectShape)
                {
                    var nodes = rectShape.GetAnchorPoints();
                    if (nodes.Contains(node))
                        return rectShape;
                }
                else if (shape is LineShape lineShape)
                {
                    if (lineShape.StartNode == node || lineShape.EndNode == node)
                        return lineShape;
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

            InvalidateCanvas();
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
                info += $"  Connections: {varShape.CenterNode.ConnectedLines.Count}\n";
                
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
    }
}
