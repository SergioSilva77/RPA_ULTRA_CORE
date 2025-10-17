using System.IO;
using System.Reflection;
using SkiaSharp;

namespace RPA_ULTRA_CORE.Resources
{
    /// <summary>
    /// Gerenciador de recursos com cache de imagens
    /// </summary>
    public class ResourceManager
    {
        private static ResourceManager? _instance;
        private readonly Dictionary<string, SKImage> _imageCache = new();
        private readonly string _baseDirectory;
        private readonly string _assetsDirectory;
        private SKImage? _placeholderImage;

        public static ResourceManager Instance => _instance ??= new ResourceManager();

        public bool UseEmbeddedAsDefault { get; set; } = true;
        public bool AllowDiskOverrides { get; set; } = true;

        private ResourceManager()
        {
            _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _assetsDirectory = Path.Combine(_baseDirectory, "assets", "icons");
        }

        /// <summary>
        /// Carrega uma imagem do recurso especificado
        /// </summary>
        public SKImage LoadImage(string resourceName)
        {
            if (string.IsNullOrEmpty(resourceName))
                return GetPlaceholderImage();

            // Verifica cache
            if (_imageCache.TryGetValue(resourceName, out var cached))
                return cached;

            SKImage? image = null;

            // Tenta carregar conforme o tipo de recurso
            if (resourceName.StartsWith("embedded:"))
            {
                image = LoadEmbeddedImage(resourceName.Substring(9));
            }
            else if (AllowDiskOverrides)
            {
                // Tenta carregar do disco primeiro
                var diskPath = Path.Combine(_assetsDirectory, resourceName);
                if (File.Exists(diskPath))
                {
                    image = LoadDiskImage(diskPath);
                }
            }

            // Fallback para embedded se configurado
            if (image == null && UseEmbeddedAsDefault)
            {
                image = LoadEmbeddedImage(resourceName);
            }

            // Se ainda não carregou, usa placeholder
            image ??= GetPlaceholderImage();

            // Adiciona ao cache
            _imageCache[resourceName] = image;
            return image;
        }

        /// <summary>
        /// Carrega imagem embedded do assembly
        /// </summary>
        private SKImage? LoadEmbeddedImage(string resourceName)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var fullResourceName = $"RPA_ULTRA_CORE.Resources.Icons.{resourceName}";

                using var stream = assembly.GetManifestResourceStream(fullResourceName);
                if (stream != null)
                {
                    using var skStream = new SKManagedStream(stream);
                    return SKImage.FromEncodedData(skStream);
                }
            }
            catch
            {
                // Log silently
            }
            return null;
        }

        /// <summary>
        /// Carrega imagem do disco
        /// </summary>
        private SKImage? LoadDiskImage(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using var stream = File.OpenRead(path);
                    using var skStream = new SKManagedStream(stream);
                    return SKImage.FromEncodedData(skStream);
                }
            }
            catch
            {
                // Log silently
            }
            return null;
        }

        /// <summary>
        /// Cria ou retorna imagem placeholder
        /// </summary>
        private SKImage GetPlaceholderImage()
        {
            if (_placeholderImage == null)
            {
                // Cria um placeholder simples (quadrado cinza com borda)
                using var surface = SKSurface.Create(new SKImageInfo(32, 32));
                var canvas = surface.Canvas;
                canvas.Clear(SKColors.DarkGray);

                using var paint = new SKPaint
                {
                    Color = SKColors.Gray,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2
                };
                canvas.DrawRect(1, 1, 30, 30, paint);

                _placeholderImage = surface.Snapshot();
            }
            return _placeholderImage;
        }

        /// <summary>
        /// Limpa o cache de imagens
        /// </summary>
        public void ClearCache()
        {
            foreach (var image in _imageCache.Values)
            {
                image?.Dispose();
            }
            _imageCache.Clear();
        }

        /// <summary>
        /// Remove uma imagem específica do cache
        /// </summary>
        public void RemoveFromCache(string resourceName)
        {
            if (_imageCache.TryGetValue(resourceName, out var image))
            {
                image?.Dispose();
                _imageCache.Remove(resourceName);
            }
        }

        public void Dispose()
        {
            ClearCache();
            _placeholderImage?.Dispose();
        }
    }
}