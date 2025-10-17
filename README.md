# RPA ULTRA CORE

Um editor vetorial avançado com interface estilo Minecraft, desenvolvido em WPF com SkiaSharp, oferecendo recursos de desenho de linhas com snap inteligente e sistema de plugins extensível.

## 📋 Índice

- [Características](#-características)
- [Requisitos](#-requisitos)
- [Instalação](#-instalação)
- [Como Usar](#-como-usar)
- [Arquitetura](#-arquitetura)
- [Alterações Recentes](#-alterações-recentes)
- [Sistema de Plugins](#-sistema-de-plugins)
- [Desenvolvimento](#-desenvolvimento)
- [Estrutura do Projeto](#-estrutura-do-projeto)

## 🚀 Características

### Editor Vetorial
- **Desenho de Linhas com Preview**: Segure SHIFT para visualizar preview da linha com 35% de opacidade antes de desenhar
- **Sistema de Snap Inteligente**:
  - Snap-to-Grid: Alinhamento automático à grade de 8px
  - Snap-to-Endpoint: Conexão automática com pontos existentes
  - Snap-to-Line: Conexão no meio de linhas existentes (mid-span)
  - **Snap-to-Shape**: Conexão ao perímetro de formas geométricas (círculos e retângulos)
- **Indicadores Visuais de Snap**:
  - Círculo amarelo para snap em linhas
  - Círculo ciano para snap em formas
  - Símbolos indicando tipo de âncora (C=centro, ◆=canto, │=meio da aresta, ●=perímetro)

### Inventário Estilo Minecraft 1.5.2
- **Layout de 7 Colunas**: Grid fixo com 7 colunas
- **Seções com Ícones**: Abas de seções mostrando apenas ícones (sem texto)
- **Sistema de Paginação**: Formato "1 of N" para múltiplas páginas
- **Busca Integrada**: Filtro de itens dentro da seção atual
- **Hotbar com 7 Slots**:
  - Sempre visível na parte inferior
  - Teclas de atalho 1-7
  - Exibe nome dos itens (único lugar com labels)
- **Drag & Drop**: Arraste itens do inventário para o canvas

### Sistema de Formas
- **Círculos**: Snap ao perímetro e centro
- **Retângulos**: Snap aos cantos, meio das arestas e perímetro
- **Linhas**: Conexões dinâmicas entre pontos

## 📦 Requisitos

- **.NET 8.0** ou superior
- **Windows** (aplicação WPF)
- **Visual Studio 2022** ou **Visual Studio Code** com C# extension

### Pacotes NuGet Utilizados
```xml
<PackageReference Include="SkiaSharp" Version="2.88.6" />
<PackageReference Include="SkiaSharp.Views.WPF" Version="2.88.6" />
<PackageReference Include="System.Composition" Version="8.0.0" />
<PackageReference Include="System.Composition.AttributedModel" Version="8.0.0" />
<PackageReference Include="System.Composition.Hosting" Version="8.0.0" />
```

## 🔧 Instalação

1. **Clone o repositório**:
```bash
git clone https://github.com/seu-usuario/RPA_ULTRA_CORE.git
cd RPA_ULTRA_CORE
```

2. **Restaure os pacotes**:
```bash
dotnet restore
```

3. **Compile o projeto**:
```bash
dotnet build
```

4. **Execute a aplicação**:
```bash
dotnet run --project RPA_ULTRA_CORE
```

## 🎮 Como Usar

### Controles Básicos

#### Desenho de Linhas
- **SHIFT (segurar)**: Ativa o modo de preview de linha
- **SHIFT + Click**: Inicia o desenho de uma linha
- **Click (com SHIFT)**: Finaliza a linha no ponto clicado
- **Soltar SHIFT**: Cancela o desenho

#### Inventário
- **Tecla E**: Abre/fecha o inventário
- **ESC**: Fecha o inventário
- **1-7**: Seleciona slots da hotbar
- **Setas ← →**: Navega entre páginas do inventário
- **Click em Seção**: Muda para outra seção de itens
- **Drag & Drop**: Arrasta item para o canvas ou hotbar

#### Edição
- **DELETE**: Remove forma selecionada
- **Click em forma**: Seleciona a forma
- **Arrastar handles**: Modifica pontos das linhas
- **Arrastar forma**: Move a forma inteira

### Sistema de Snap

O sistema de snap funciona automaticamente com a seguinte prioridade:

1. **Snap em Formas** (maior prioridade)
   - Detecta proximidade com círculos e retângulos
   - Snap ao perímetro, centro, cantos e meio das arestas
   - Indicador visual ciano

2. **Snap em Endpoints**
   - Conecta com pontos existentes de outras linhas
   - Merge automático de nodes

3. **Snap em Linhas (mid-span)**
   - Conecta no meio de segmentos existentes
   - Indicador visual amarelo

4. **Snap na Grade** (menor prioridade)
   - Alinha aos pontos da grade de 8px
   - Sempre ativo como fallback

### Usando o Inventário

1. Pressione **E** para abrir
2. Clique nos ícones de seção no topo para mudar de categoria
3. Use a barra de busca para filtrar itens
4. Arraste itens para:
   - **Canvas**: Cria a forma na posição do drop
   - **Hotbar**: Adiciona o item ao slot rápido
5. Use as teclas **1-7** para acessar rapidamente itens da hotbar

## 🏗 Arquitetura

### Padrão MVVM
- **Views**: MainWindow (WPF)
- **ViewModels**: SketchViewModel (lógica de negócio)
- **Models**: Shapes, Nodes, Inventory

### Sistema de Eventos
- **EventBus**: Singleton para comunicação entre componentes
- **Observer Pattern**: Nodes notificam mudanças de posição

### Renderização
- **SkiaSharp**: Renderização 2D acelerada
- **Canvas Customizado**: SketchCanvas para desenho vetorial

## 🔄 Alterações Recentes

### Sistema de Snap em Formas (Novo!)
- **IAnchorProvider Interface**: Define contrato para formas fornecerem pontos de snap
- **ShapeAttachment Class**: Gerencia conexões entre nodes e âncoras de formas
- **CircleShape com Snap**: Snap ao perímetro do círculo
- **RectShape com Snap**: Snap a cantos, arestas e perímetro
- **Indicadores Visuais**: Feedback visual diferenciado por tipo de snap

### Inventário Minecraft 1.5.2
- **Ícones apenas**: Removido texto das seções e grid principal
- **Labels na Hotbar**: Apenas hotbar mostra nomes dos itens
- **Paginação em Inglês**: Mudado de "1 de N" para "1 of N"
- **Teclas 1-7**: Números visíveis nos slots da hotbar

### Melhorias no SnapService
- **DetectShapeSnap**: Novo método para detectar snap em formas
- **SnapWithPriority**: Sistema de prioridade de snap
- **ProjectPointOnSegment**: Projeção precisa em segmentos

## 🔌 Sistema de Plugins

### Criando um Plugin

1. **Implemente IPlugin**:
```csharp
[Export(typeof(IPlugin))]
public class MeuPlugin : IPlugin
{
    public string Id => "meu.plugin";
    public string Name => "Meu Plugin";
    public string Version => "1.0.0";

    public IEnumerable<IInventorySection> GetSections()
    {
        yield return new MinhaSecao();
    }
}
```

2. **Defina uma Seção**:
```csharp
public class MinhaSecao : IInventorySection
{
    public string Id => "secao.custom";
    public string Name => "Custom";
    public string IconResource => "embedded:custom_icon.png";

    public IEnumerable<IInventoryItem> GetItems()
    {
        // Retorne seus itens customizados
    }
}
```

3. **Compile como DLL** e coloque na pasta `Plugins/`

### Recursos Embarcados

Os ícones podem ser:
- **Embarcados**: `embedded:nome_icone.png`
- **Arquivo local**: `file:caminho/para/icone.png`
- **URL**: `http://exemplo.com/icone.png`

## 💻 Desenvolvimento

### Estrutura de Pastas
```
RPA_ULTRA_CORE/
├── Models/
│   └── Geometry/       # Shapes, Nodes, Attachments
├── ViewModels/         # SketchViewModel
├── Views/              # MainWindow
├── Services/           # SnapService, EventBus
├── Inventory/
│   ├── UI/            # InventoryView
│   ├── Models/        # InventorySlot
│   └── Services/      # InventoryService
├── Plugins/
│   └── Abstractions/  # Interfaces
├── Resources/         # ResourceManager
└── Utils/            # Math2D, Helpers
```

### Compilação e Debug

```bash
# Modo Debug
dotnet build --configuration Debug

# Modo Release
dotnet build --configuration Release

# Executar com logs
dotnet run --project RPA_ULTRA_CORE --verbosity detailed
```

### Testes

```bash
# Executar testes (quando implementados)
dotnet test
```

## 📁 Estrutura do Projeto

### Classes Principais

- **SketchViewModel**: Gerencia estado do canvas e interações
- **SnapService**: Lógica de snap (grid, endpoint, linha, forma)
- **InventoryView**: UI do inventário estilo Minecraft
- **LineShape**: Linhas com conexões dinâmicas
- **CircleShape**: Círculos com snap ao perímetro
- **RectShape**: Retângulos com snap múltiplo
- **Node**: Pontos de conexão compartilhados
- **ShapeAttachment**: Conexão node-forma
- **SegmentAttachment**: Conexão node-linha

### Configuração (appsettings.json)

```json
{
  "Canvas": {
    "GridSize": 8,
    "SnapTolerance": 10,
    "EnableGridSnap": true,
    "EnableEndpointSnap": true,
    "EnableLineSnap": true,
    "EnableShapeSnap": true
  },
  "Inventory": {
    "Columns": 7,
    "RowsPerPage": 5,
    "SlotSize": 48
  },
  "Plugins": {
    "Directory": "Plugins",
    "LoadOnStartup": true
  }
}
```

## 🐛 Solução de Problemas

### Erro de Compilação com MEF
Se receber erro sobre `ConventionBuilder`:
- O projeto usa `System.Composition` (não `System.ComponentModel.Composition`)
- Verifique se todos os pacotes MEF estão instalados

### Snap não funciona
- Verifique se `EnableShapeSnap` está `true` no SnapService
- A tolerância padrão é 10px, ajuste se necessário
- Certifique-se que as formas implementam `IAnchorProvider`

### Inventário não abre
- Tecla **E** deve ser pressionada quando o canvas tem foco
- Verifique se há plugins carregados com seções

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE.txt) para mais detalhes.

## 🤝 Contribuindo

1. Faça um Fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📞 Contato

Para dúvidas, sugestões ou problemas, abra uma [Issue](https://github.com/seu-usuario/RPA_ULTRA_CORE/issues) no GitHub.

---

**Desenvolvido com WPF + SkiaSharp + MEF**