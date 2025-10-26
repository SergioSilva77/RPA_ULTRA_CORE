using SkiaSharp;
using System;
using System.Collections.Generic;
using RPA_ULTRA_CORE.Services;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Representa um nó de variável que pode armazenar dados e propagar pelos galhos
    /// AGORA COM SUPORTE A SNAP COMPLETO
    /// </summary>
    public class VariableShape : BaseShape, IAnchorProvider
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

        #region IAnchorProvider Implementation - SISTEMA DE SNAP

        /// <summary>
        /// Retorna todos os pontos de âncora disponíveis para snap
        /// IMPLEMENTAÇÃO CRÍTICA PARA O SNAP FUNCIONAR!
        /// </summary>
        public IEnumerable<AnchorPoint> GetAnchorPoints(SKPoint mousePosition)
        {
            // 1. ÂNCORA DO CENTRO (sempre disponível - maior prioridade)
            yield return new AnchorPoint
            {
                Position = _position,
                Type = AnchorType.Center,
                Symbol = "C",
                Node = CenterNode,
                Shape = this
            };

            // 2. ÂNCORAS DO PERÍMETRO DO CÍRCULO (snap ao redor)
            // Calcula 8 pontos ao redor do círculo para snap suave
            var angles = new[] { 0, 45, 90, 135, 180, 225, 270, 315 };
            foreach (var angle in angles)
            {
                var radians = angle * Math.PI / 180.0;
                var perimeterPoint = new SKPoint(
                    _position.X + RADIUS * (float)Math.Cos(radians),
                    _position.Y + RADIUS * (float)Math.Sin(radians)
                );

                yield return new AnchorPoint
                {
                    Position = perimeterPoint,
                    Type = AnchorType.Perimeter,
                    Symbol = "●",
                    Node = null, // Perímetro não tem node, será criado ao conectar
                    Shape = this
                };
            }

            // 3. SNAP DINÂMICO AO PERÍMETRO MAIS PRÓXIMO DO MOUSE
            // Isso permite snap em qualquer ponto do círculo
            var direction = mousePosition - _position;
            var length = direction.Length;
            
            if (length > 0.1f) // Evita divisão por zero
            {
                var normalized = new SKPoint(direction.X / length, direction.Y / length);
                var closestPerimeterPoint = new SKPoint(
                    _position.X + normalized.X * RADIUS,
                    _position.Y + normalized.Y * RADIUS
                );

                yield return new AnchorPoint
                {
                    Position = closestPerimeterPoint,
                    Type = AnchorType.Perimeter,
                    Symbol = "●",
                    Node = null,
                    Shape = this,
                    Priority = 1 // Maior prioridade para snap dinâmico
                };
            }
        }

        /// <summary>
        /// Testa se um ponto está próximo o suficiente para fazer snap
        /// </summary>
        public bool IsNearAnchor(SKPoint point, float tolerance)
        {
            // Verifica se está próximo ao centro
            var distanceToCenter = SKPoint.Distance(point, _position);
            if (distanceToCenter <= tolerance)
                return true;

            // Verifica se está próximo ao perímetro
            var distanceFromEdge = Math.Abs(distanceToCenter - RADIUS);
            return distanceFromEdge <= tolerance;
        }

        /// <summary>
        /// Retorna o ponto de âncora mais próximo do ponto dado
        /// </summary>
        public AnchorPoint GetClosestAnchor(SKPoint point)
        {
            var distanceToCenter = SKPoint.Distance(point, _position);
            
            // Se estiver muito próximo ao centro, retorna âncora do centro
            if (distanceToCenter <= RADIUS * 0.3f)
            {
                return new AnchorPoint
                {
                    Position = _position,
                    Type = AnchorType.Center,
                    Symbol = "C",
                    Node = CenterNode,
                    Shape = this
                };
            }

            // Caso contrário, retorna ponto no perímetro mais próximo do mouse
            var direction = point - _position;
            var length = direction.Length;
            
            if (length > 0.1f)
            {
                var normalized = new SKPoint(direction.X / length, direction.Y / length);
                var perimeterPoint = new SKPoint(
                    _position.X + normalized.X * RADIUS,
                    _position.Y + normalized.Y * RADIUS
                );

                return new AnchorPoint
                {
                    Position = perimeterPoint,
                    Type = AnchorType.Perimeter,
                    Symbol = "●",
                    Node = null,
                    Shape = this
                };
            }

            // Fallback: retorna centro
            return new AnchorPoint
            {
                Position = _position,
                Type = AnchorType.Center,
                Symbol = "C",
                Node = CenterNode,
                Shape = this
            };
        }

        #endregion
    }
}
