using SkiaSharp;
using System;
using System.Collections.Generic;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Representa um nó de variável que pode armazenar dados e propagar pelos galhos
    /// </summary>
    public class VariableShape : BaseShape
    {
        private string _variableName;
        private string _variableValue;
        private SKPoint _position;
        private const float RADIUS = 25f;
        
        public string VariableName 
        { 
            get => _variableName;
            set
            {
                _variableName = value;
                OnPropertyChanged();
            }
        }
        
        public string VariableValue 
        { 
            get => _variableValue;
            set
            {
                _variableValue = value;
                OnPropertyChanged();
            }
        }

        public SKPoint Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged();
                UpdateConnectedLines();
            }
        }

        public Node CenterNode { get; private set; }

        // Armazena as variáveis que chegaram por linhas conectadas
        public Dictionary<string, string> IncomingVariables { get; private set; }

        public VariableShape(SKPoint position)
        {
            _position = position;
            _variableName = "Variable";
            _variableValue = "";
            IncomingVariables = new Dictionary<string, string>();
            
            // Cria o nó central
            CenterNode = new Node(position.X, position.Y);
            CenterNode.PositionChanged += OnNodePositionChanged;
        }

        private void OnNodePositionChanged(object sender, EventArgs e)
        {
            if (sender is Node node)
            {
                _position = new SKPoint(node.X, node.Y);
                OnPropertyChanged(nameof(Position));
            }
        }

        public override void Draw(SKCanvas canvas)
        {
            // Desenha o círculo externo (borda)
            using (var borderPaint = new SKPaint
            {
                Color = IsSelected ? SKColors.Orange : SKColors.DodgerBlue,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 3,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(_position, RADIUS, borderPaint);
            }

            // Desenha o preenchimento
            using (var fillPaint = new SKPaint
            {
                Color = new SKColor(30, 144, 255, 80), // DodgerBlue semi-transparente
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawCircle(_position, RADIUS - 1.5f, fillPaint);
            }

            // Desenha o ícone "V" no centro
            using (var iconPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2.5f,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true
            })
            {
                var iconSize = 12f;
                var path = new SKPath();
                path.MoveTo(_position.X - iconSize/2, _position.Y - iconSize/3);
                path.LineTo(_position.X, _position.Y + iconSize/2);
                path.LineTo(_position.X + iconSize/2, _position.Y - iconSize/3);
                canvas.DrawPath(path, iconPaint);
            }

            // Desenha o label com o nome da variável se definido
            if (!string.IsNullOrEmpty(_variableName) && _variableName != "Variable")
            {
                using (var textPaint = new SKPaint
                {
                    Color = SKColors.White,
                    TextSize = 11,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center,
                    Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyle.Bold)
                })
                {
                    var textY = _position.Y + RADIUS + 15;
                    
                    // Desenha fundo semi-transparente para o texto
                    var textBounds = new SKRect();
                    textPaint.MeasureText(_variableName, ref textBounds);
                    var bgRect = new SKRect(
                        _position.X - textBounds.Width/2 - 4,
                        textY - textBounds.Height - 2,
                        _position.X + textBounds.Width/2 + 4,
                        textY + 2
                    );
                    
                    using (var bgPaint = new SKPaint
                    {
                        Color = new SKColor(0, 0, 0, 180),
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRoundRect(bgRect, 3, 3, bgPaint);
                    }
                    
                    canvas.DrawText(_variableName, _position.X, textY, textPaint);
                }
            }

            // Desenha o nó central
            CenterNode?.Draw(canvas);
        }

        public override bool HitTest(SKPoint point)
        {
            var distance = SKPoint.Distance(point, _position);
            return distance <= RADIUS;
        }

        public override void Move(float deltaX, float deltaY)
        {
            Position = new SKPoint(_position.X + deltaX, _position.Y + deltaY);
            CenterNode.X += deltaX;
            CenterNode.Y += deltaY;
        }

        private void UpdateConnectedLines()
        {
            // Atualiza as linhas conectadas ao nó central
            CenterNode.NotifyPositionChanged();
        }

        /// <summary>
        /// Propaga a variável para os nós conectados
        /// </summary>
        public void PropagateVariable()
        {
            if (string.IsNullOrEmpty(_variableName)) return;

            // Adiciona a própria variável
            var allVariables = new Dictionary<string, string>(IncomingVariables)
            {
                [_variableName] = _variableValue
            };

            // Propaga para todos os nós conectados
            PropagateToConnectedNodes(CenterNode, allVariables);
        }

        private void PropagateToConnectedNodes(Node fromNode, Dictionary<string, string> variables)
        {
            if (fromNode == null) return;

            foreach (var line in fromNode.ConnectedLines)
            {
                Node targetNode = null;
                
                if (line.StartNode == fromNode)
                    targetNode = line.EndNode;
                else if (line.EndNode == fromNode)
                    targetNode = line.StartNode;

                if (targetNode != null)
                {
                    // Encontra a shape associada ao nó de destino
                    var targetShape = FindShapeByNode(targetNode);
                    
                    if (targetShape is VariableShape varShape)
                    {
                        // Mescla as variáveis
                        foreach (var kvp in variables)
                        {
                            varShape.IncomingVariables[kvp.Key] = kvp.Value;
                        }
                        
                        // Propaga recursivamente
                        varShape.PropagateVariable();
                    }
                }
            }
        }

        // Este método precisa ser implementado no SketchViewModel
        // para encontrar shapes pelo nó
        private BaseShape FindShapeByNode(Node node)
        {
            // TODO: Implementar busca no SketchViewModel
            return null;
        }

        public override BaseShape Clone()
        {
            return new VariableShape(_position)
            {
                VariableName = this.VariableName,
                VariableValue = this.VariableValue
            };
        }

        /// <summary>
        /// Retorna todas as variáveis disponíveis (própria + recebidas)
        /// </summary>
        public Dictionary<string, string> GetAllVariables()
        {
            var allVars = new Dictionary<string, string>(IncomingVariables);
            if (!string.IsNullOrEmpty(_variableName))
            {
                allVars[_variableName] = _variableValue;
            }
            return allVars;
        }

        public List<Node> GetAnchorPoints()
        {
            return new List<Node> { CenterNode };
        }
    }
}
