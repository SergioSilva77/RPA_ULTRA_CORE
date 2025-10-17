namespace RPA_ULTRA_CORE.Models.Geometry
{
    /// <summary>
    /// Ponto de conexão compartilhado entre formas.
    /// Implementa padrão Observer para notificar mudanças de posição.
    /// </summary>
    public sealed class Node
    {
        private double _x;
        private double _y;

        public Guid Id { get; }
        public double X => _x;
        public double Y => _y;

        /// <summary>
        /// Evento disparado quando a posição do Node muda
        /// </summary>
        public event EventHandler? PositionChanged;

        public Node(double x, double y)
        {
            Id = Guid.NewGuid();
            _x = x;
            _y = y;
        }

        /// <summary>
        /// Define nova posição e dispara evento se houver mudança
        /// </summary>
        public void Set(double x, double y)
        {
            if (Math.Abs(_x - x) > 0.001 || Math.Abs(_y - y) > 0.001)
            {
                _x = x;
                _y = y;
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Move o Node por deslocamento relativo
        /// </summary>
        public void Move(double dx, double dy)
        {
            Set(_x + dx, _y + dy);
        }

        /// <summary>
        /// Cria cópia do Node com nova ID
        /// </summary>
        public Node Clone()
        {
            return new Node(_x, _y);
        }

        public override bool Equals(object? obj)
        {
            return obj is Node node && Id.Equals(node.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}