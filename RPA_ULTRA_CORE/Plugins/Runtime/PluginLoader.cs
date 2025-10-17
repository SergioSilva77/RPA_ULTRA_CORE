using System.Composition.Hosting;
using System.IO;
using System.Reflection;
using RPA_ULTRA_CORE.Plugins.Abstractions;

namespace RPA_ULTRA_CORE.Plugins.Runtime
{
    /// <summary>
    /// Carregador de plugins usando MEF
    /// </summary>
    public class PluginLoader
    {
        private readonly string _pluginsDirectory;
        private readonly List<IPlugin> _loadedPlugins = new();
        private CompositionHost? _container;

        public IReadOnlyList<IPlugin> LoadedPlugins => _loadedPlugins.AsReadOnly();

        public PluginLoader()
        {
            // Usa BaseDirectory como raiz
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            _pluginsDirectory = Path.Combine(baseDir, "plugins");
        }

        public PluginLoader(string pluginsDirectory)
        {
            _pluginsDirectory = pluginsDirectory;
        }

        /// <summary>
        /// Carrega todos os plugins do diretório
        /// </summary>
        public async Task LoadPluginsAsync()
        {
            if (!Directory.Exists(_pluginsDirectory))
            {
                Directory.CreateDirectory(_pluginsDirectory);
                return;
            }

            var assemblies = new List<Assembly>();

            // Carrega todas as DLLs do diretório
            foreach (var file in Directory.GetFiles(_pluginsDirectory, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file);
                    assemblies.Add(assembly);
                }
                catch (Exception ex)
                {
                    // Log error silently
                    Console.WriteLine($"Failed to load plugin assembly {file}: {ex.Message}");
                }
            }

            // Adiciona a própria assembly para plugins internos
            assemblies.Add(Assembly.GetExecutingAssembly());

            // Configuração sem ConventionBuilder (usando exports diretos)
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);

            _container = configuration.CreateContainer();

            // Obtém todos os plugins exportados
            _loadedPlugins.Clear();
            _loadedPlugins.AddRange(_container.GetExports<IPlugin>());

            // Carrega manifestos JSON se existirem
            await LoadManifestsAsync();
        }

        /// <summary>
        /// Carrega manifestos JSON dos plugins
        /// </summary>
        private async Task LoadManifestsAsync()
        {
            foreach (var manifestFile in Directory.GetFiles(_pluginsDirectory, "*.plugin.json"))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(manifestFile);
                    var manifest = Newtonsoft.Json.JsonConvert.DeserializeObject<PluginManifest>(json);

                    if (manifest != null)
                    {
                        // Cria um plugin dinâmico a partir do manifesto
                        var dynamicPlugin = new ManifestBasedPlugin(manifest);
                        _loadedPlugins.Add(dynamicPlugin);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load manifest {manifestFile}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Obtém todas as seções de todos os plugins
        /// </summary>
        public IEnumerable<IInventorySection> GetAllSections()
        {
            return _loadedPlugins
                .SelectMany(p => p.GetSections())
                .OrderBy(s => s.DisplayOrder);
        }

        /// <summary>
        /// Obtém um plugin por ID
        /// </summary>
        public IPlugin? GetPlugin(string id)
        {
            return _loadedPlugins.FirstOrDefault(p => p.Id == id);
        }

        public void Dispose()
        {
            _container?.Dispose();
        }
    }
}