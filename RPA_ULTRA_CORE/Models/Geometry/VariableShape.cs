using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Representa um nó de variável que pode armazenar dados e propagar pelos galhos
    /// </summary>
    public class VariableShape : BaseShape
    {
        private string _variableName = "Variable";
        private string _variableValue = "";
        private SKPoint _position;
        private const float RADIUS = 25f;
        private Node _centerNode;

        public string VariableName
        {
            get => _variableName;
            set => _variableName = value ?? "Variable";
        }

        public string VariableValue
        {
            get => _variableValue;
            set => _variableValue = value ?? "";
        }

        public SKPoint Position => _position;

        public Node CenterNode => _centerNode;

        // Armazena as variáveis que chegaram por linhas conectadas
        public Dictionary<string, string> IncomingVariables { get; private set; }

        // Referência ao ViewModel para buscar shapes
        public ViewModels.SketchViewModel? ViewModel { get; set; }

        public VariableShape(SKPoint position) : base()
        {
            _position = position;
            _variableName = "Variable";
            _variableValue = "";
            IncomingVariables = new Dictionary<string, string>();

            // Cria o nó central
            _centerNode = new Node(position.X, position.Y);
            _centerNode.PositionChanged += OnNodePositionChanged;
        }

        private void OnNodePositionChanged(object? sender, EventArgs e)
        {
            if (sender is Node node)
            {
                _position = new SKPoint((float)node.X, (float)node.Y);
            }
        }

        public override void Draw(SKCanvas canvas, float dpiScale)
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
        }

        public override bool HitTestPoint(SKPoint point, float tolerance)
        {
            var distance = SKPoint.Distance(point, _position);
            return distance <= (RADIUS + tolerance);
        }

        public bool HitTestPoint(SKPoint point)
        {
            return HitTestPoint(point, 0);
        }

        public override void Move(double deltaX, double deltaY)
        {
            _position = new SKPoint(_position.X + (float)deltaX, _position.Y + (float)deltaY);
            _centerNode.Set(_centerNode.X + deltaX, _centerNode.Y + deltaY);
        }

        public override SKRect GetBounds()
        {
            return new SKRect(
                _position.X - RADIUS,
                _position.Y - RADIUS,
                _position.X + RADIUS,
                _position.Y + RADIUS
            );
        }

        public override IEnumerable<Node> GetNodes()
        {
            return new List<Node> { _centerNode };
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

            // Por enquanto, a propagação vai ser simplificada
            // já que o sistema de Attachments é diferente do esperado
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
    }
}