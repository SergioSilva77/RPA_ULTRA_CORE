namespace RPA_ULTRA_CORE.Utils
{
    /// <summary>
    /// Classe para validações e guardas
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// Garante que objeto não é nulo
        /// </summary>
        public static T NotNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
            return value;
        }

        /// <summary>
        /// Garante que string não é vazia
        /// </summary>
        public static string NotEmpty(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"{paramName} não pode ser vazio", paramName);
            return value;
        }

        /// <summary>
        /// Garante que valor está em intervalo
        /// </summary>
        public static T InRange<T>(T value, T min, T max, string paramName)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                throw new ArgumentOutOfRangeException(paramName,
                    $"{paramName} deve estar entre {min} e {max}");
            return value;
        }

        /// <summary>
        /// Garante que coleção não é vazia
        /// </summary>
        public static IEnumerable<T> NotEmpty<T>(IEnumerable<T> collection, string paramName)
        {
            var list = collection?.ToList() ?? new List<T>();
            if (!list.Any())
                throw new ArgumentException($"{paramName} não pode ser vazio", paramName);
            return list;
        }
    }
}