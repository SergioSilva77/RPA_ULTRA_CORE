# Guia de Troubleshooting - Sistema de Variáveis

## Índice Rápido
- [Problemas de Compilação](#problemas-de-compilação)
- [Problemas de Execução](#problemas-de-execução)
- [Problemas de Interface](#problemas-de-interface)
- [Problemas de Propagação](#problemas-de-propagação)
- [Problemas de Performance](#problemas-de-performance)

---

## Problemas de Compilação

### ❌ Erro: "VariableShape não encontrado"

**Sintoma:**
```
error CS0246: The type or namespace name 'VariableShape' could not be found
```

**Causa:** Arquivo não foi adicionado ao projeto ou namespace incorreto.

**Solução:**
```bash
# 1. Verificar se arquivo existe
ls Models/Geometry/VariableShape.cs

# 2. Adicionar ao .csproj se necessário
# Em RPA_ULTRA_CORE.csproj:
<ItemGroup>
  <Compile Include="Models\Geometry\VariableShape.cs" />
</ItemGroup>

# 3. Reconstruir
dotnet clean
dotnet build
```

---

### ❌ Erro: "Namespace 'Views' não encontrado"

**Sintoma:**
```
error CS0246: The type or namespace name 'Views' could not be found
```

**Causa:** Referência de namespace faltando.

**Solução:**
```csharp
// Adicionar no topo do arquivo:
using RPA_ULTRA_CORE.Views;
using RPA_ULTRA_CORE.Models.Geometry;
```

---

### ❌ Erro: "IPlugin não existe no contexto atual"

**Sintoma:**
```
error CS0246: The type or namespace name 'IPlugin' could not be found
```

**Causa:** Namespace do plugin system não importado.

**Solução:**
```csharp
using RPA_ULTRA_CORE.Plugins.Abstractions;
using System.Composition;
```

---

### ❌ Erro: "SKCanvas não pode ser resolvido"

**Sintoma:**
```
error CS0246: The type or namespace name 'SKCanvas' could not be found
```

**Causa:** SkiaSharp não está instalado ou referenciado.

**Solução:**
```bash
# Instalar SkiaSharp
dotnet add package SkiaSharp --version 2.88.6
dotnet add package SkiaSharp.Views.WPF --version 2.88.6

# Adicionar using
using SkiaSharp;
```

---

## Problemas de Execução

### ❌ Variável não aparece no inventário

**Sintoma:** Seção "Data" não existe ou está vazia no inventário.

**Diagnóstico:**
```csharp
// Adicionar log no método GetSections():
public IEnumerable<IInventorySection> GetSections()
{
    System.Diagnostics.Debug.WriteLine("DataPlugin.GetSections() called");
    yield return new DataInventorySection();
}
```

**Possíveis Causas:**

1. **MEF não está carregando o plugin**
   ```csharp
   // Verificar se [Export] está presente
   [Export(typeof(IPlugin))]
   public class DataPlugin : IPlugin
   ```

2. **DLL não está na pasta Plugins/**
   ```bash
   # Verificar localização
   ls Plugins/
   
   # Copiar se necessário
   cp bin/Debug/net8.0/RPA_ULTRA_CORE.dll Plugins/
   ```

3. **Plugin está desabilitado na configuração**
   ```json
   // Verificar appsettings.json
   {
     "Plugins": {
       "LoadOnStartup": true,  // deve ser true
       "Enabled": ["plugin.data"]  // deve incluir plugin.data
     }
   }
   ```

**Solução:**
```csharp
// Em SketchViewModel ou MainWindow, adicionar log:
var plugins = pluginLoader.GetPlugins();
foreach (var plugin in plugins)
{
    Debug.WriteLine($"Plugin loaded: {plugin.Id} - {plugin.Name}");
}
```

---

### ❌ Duplo clique não abre o editor

**Sintoma:** Clicar duas vezes na variável não faz nada.

**Diagnóstico:**
```csharp
// Adicionar no método de duplo clique:
protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
{
    var point = e.GetPosition(this);
    Debug.WriteLine($"Double click at: {point}");
    
    // Seu código existente...
}
```

**Possíveis Causas:**

1. **HandleVariableDoubleClick não foi integrado**
   ```csharp
   // Adicionar no OnMouseDoubleClick:
   var skPoint = new SKPoint((float)point.X, (float)point.Y);
   HandleVariableDoubleClick(skPoint);
   ```

2. **Evento está sendo consumido antes**
   ```csharp
   // Verificar se e.Handled está sendo definido em outro lugar
   // Remover ou adicionar verificação:
   if (!e.Handled)
   {
       HandleVariableDoubleClick(skPoint);
   }
   ```

3. **HitTest não está funcionando**
   ```csharp
   // Testar HitTest manualmente:
   var testPoint = new SKPoint(100, 100);
   var hit = variableShape.HitTest(testPoint);
   Debug.WriteLine($"HitTest result: {hit}");
   ```

**Solução:**
```csharp
// Método completo de duplo clique:
protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
{
    try
    {
        var point = e.GetPosition(this);
        var skPoint = new SKPoint((float)point.X, (float)point.Y);
        
        Debug.WriteLine($"Double click at: {skPoint}");
        
        // Tenta abrir editor de variável
        var clicked = Shapes.OfType<VariableShape>()
            .FirstOrDefault(v => v.HitTest(skPoint));
            
        if (clicked != null)
        {
            Debug.WriteLine($"Variable found: {clicked.VariableName}");
            OpenVariableEditor(clicked);
            e.Handled = true;
            return;
        }
        
        // Outros handlers...
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"Error in double click: {ex}");
        MessageBox.Show($"Error: {ex.Message}", "Error", 
            MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
```

---

### ❌ Janela de edição abre mas está vazia

**Sintoma:** Dialog abre mas campos estão em branco mesmo quando variável tem valores.

**Causa:** DataContext não está sendo definido corretamente.

**Solução:**
```csharp
// Em VariableEditorDialog.cs:
public VariableEditorDialog(VariableShape variableShape)
{
    _variableShape = variableShape;
    
    // Carregar valores ANTES de InitializeComponent
    VariableName = variableShape.VariableName;
    VariableValue = variableShape.VariableValue;
    
    InitializeComponent();
    
    // Definir DataContext
    DataContext = this;
    
    Debug.WriteLine($"Dialog opened with: {VariableName} = {VariableValue}");
}
```

---

### ❌ Salvar não atualiza a variável

**Sintoma:** Clicar OK não salva os valores.

**Diagnóstico:**
```csharp
private void BtnOk_Click(object sender, RoutedEventArgs e)
{
    Debug.WriteLine($"OK clicked");
    Debug.WriteLine($"Name: {VariableName}");
    Debug.WriteLine($"Value: {VariableValue}");
    Debug.WriteLine($"Shape before: {_variableShape.VariableName}");
    
    // Seu código...
    
    Debug.WriteLine($"Shape after: {_variableShape.VariableName}");
}
```

**Causa:** Binding não está funcionando ou valores não estão sendo transferidos.

**Solução:**
```csharp
// Garantir que properties notificam mudanças:
public string VariableName 
{ 
    get => _variableName;
    set
    {
        _variableName = value;
        OnPropertyChanged(nameof(VariableName));
    }
}

// No BtnOk_Click, forçar atualização:
_variableShape.VariableName = txtName.Text.Trim();
_variableShape.VariableValue = txtValue.Text ?? "";
```

---

## Problemas de Interface

### ❌ Variável não é desenhada no canvas

**Sintoma:** Variável foi criada mas não aparece visualmente.

**Diagnóstico:**
```csharp
public override void Draw(SKCanvas canvas)
{
    Debug.WriteLine($"Drawing variable at: {Position}");
    Debug.WriteLine($"Canvas is null: {canvas == null}");
    
    // Seu código de desenho...
}
```

**Possíveis Causas:**

1. **Canvas não está sendo invalidado**
   ```csharp
   // Após adicionar shape:
   Shapes.Add(variableShape);
   InvalidateCanvas(); // ← Importante!
   ```

2. **Posição está fora dos limites visíveis**
   ```csharp
   // Verificar bounds:
   Debug.WriteLine($"Canvas size: {ActualWidth}x{ActualHeight}");
   Debug.WriteLine($"Variable pos: {shape.Position}");
   ```

3. **Shape está sendo adicionada mas Draw() não é chamado**
   ```csharp
   // No loop de renderização:
   foreach (var shape in Shapes)
   {
       Debug.WriteLine($"Drawing: {shape.GetType().Name}");
       shape.Draw(canvas);
   }
   ```

**Solução:**
```csharp
// Método para adicionar shape com validação:
public void AddVariableShape(VariableShape shape)
{
    if (shape == null)
    {
        Debug.WriteLine("ERROR: Shape is null");
        return;
    }
    
    // Verificar posição válida
    if (shape.Position.X < 0 || shape.Position.Y < 0)
    {
        Debug.WriteLine($"WARNING: Invalid position: {shape.Position}");
        shape.Position = new SKPoint(100, 100); // Posição padrão
    }
    
    Shapes.Add(shape);
    Debug.WriteLine($"Shape added: {shape.VariableName} at {shape.Position}");
    
    InvalidateCanvas();
}
```

---

### ❌ Ícone da variável aparece distorcido

**Sintoma:** Círculo ou ícone "V" não está sendo desenhado corretamente.

**Causa:** Problemas com coordenadas ou tamanhos.

**Solução:**
```csharp
public override void Draw(SKCanvas canvas)
{
    if (canvas == null) return;
    
    // Garantir que valores são válidos
    var radius = Math.Max(RADIUS, 10f);
    var pos = Position;
    
    // Log para debug
    Debug.WriteLine($"Drawing at ({pos.X}, {pos.Y}) with radius {radius}");
    
    // Desenhar com valores validados
    using (var paint = new SKPaint
    {
        Color = SKColors.DodgerBlue,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 3,
        IsAntialias = true // Importante!
    })
    {
        canvas.DrawCircle(pos, radius, paint);
    }
}
```

---

## Problemas de Propagação

### ❌ Variáveis não propagam pelos galhos

**Sintoma:** Conectar variáveis mas dados não fluem.

**Diagnóstico:**
```csharp
public void PropagateVariable()
{
    Debug.WriteLine($"=== Propagating {VariableName} ===");
    Debug.WriteLine($"Value: {VariableValue}");
    Debug.WriteLine($"Connected lines: {CenterNode.ConnectedLines.Count}");
    
    foreach (var line in CenterNode.ConnectedLines)
    {
        Debug.WriteLine($"Line: {line.StartNode} -> {line.EndNode}");
    }
    
    // Seu código...
}
```

**Possíveis Causas:**

1. **FindShapeByNode retorna null**
   ```csharp
   private BaseShape FindShapeByNode(Node node)
   {
       var result = ViewModel?.FindShapeByNode(node);
       Debug.WriteLine($"FindShapeByNode({node}): {result?.GetType().Name ?? "null"}");
       return result;
   }
   ```

2. **Linhas não estão conectadas aos nós corretos**
   ```csharp
   // Verificar conexões:
   Debug.WriteLine($"Node {CenterNode} connections:");
   foreach (var line in CenterNode.ConnectedLines)
   {
       Debug.WriteLine($"  Start: {line.StartNode == CenterNode}");
       Debug.WriteLine($"  End: {line.EndNode == CenterNode}");
   }
   ```

3. **Referência ao ViewModel não está definida**
   ```csharp
   // Ao criar variável:
   var varShape = new VariableShape(position)
   {
       ViewModel = this // ← Não esquecer!
   };
   ```

**Solução Completa:**
```csharp
// Em VariableShape.cs:
public SketchViewModel ViewModel { get; set; }

// Método de propagação com logs:
public void PropagateVariable()
{
    if (string.IsNullOrEmpty(VariableName))
    {
        Debug.WriteLine("Skipping propagation: Name is empty");
        return;
    }
    
    if (ViewModel == null)
    {
        Debug.WriteLine("ERROR: ViewModel is null!");
        return;
    }
    
    Debug.WriteLine($"Propagating {VariableName} = {VariableValue}");
    
    var allVars = new Dictionary<string, string>(IncomingVariables)
    {
        [VariableName] = VariableValue
    };
    
    Debug.WriteLine($"Total variables to propagate: {allVars.Count}");
    
    PropagateToConnectedNodes(CenterNode, allVars);
}

private void PropagateToConnectedNodes(Node fromNode, Dictionary<string, string> variables)
{
    if (fromNode == null || ViewModel == null) return;
    
    Debug.WriteLine($"Checking {fromNode.ConnectedLines.Count} connections");
    
    foreach (var line in fromNode.ConnectedLines)
    {
        Node targetNode = line.StartNode == fromNode ? line.EndNode : line.StartNode;
        
        if (targetNode == null)
        {
            Debug.WriteLine("WARNING: Target node is null");
            continue;
        }
        
        var targetShape = ViewModel.FindShapeByNode(targetNode);
        
        if (targetShape == null)
        {
            Debug.WriteLine($"WARNING: No shape found for node");
            continue;
        }
        
        if (targetShape is VariableShape varShape)
        {
            Debug.WriteLine($"Propagating to: {varShape.VariableName}");
            
            foreach (var kvp in variables)
            {
                varShape.IncomingVariables[kvp.Key] = kvp.Value;
                Debug.WriteLine($"  Set {kvp.Key} = {kvp.Value}");
            }
            
            varShape.PropagateVariable();
        }
    }
}
```

---

### ❌ Loop infinito na propagação

**Sintoma:** Aplicação trava ou fica lenta ao conectar variáveis.

**Causa:** Variáveis conectadas em círculo causam recursão infinita.

**Solução:**
```csharp
// Adicionar detecção de loop:
private HashSet<VariableShape> _propagationStack = new HashSet<VariableShape>();

public void PropagateVariable()
{
    // Detectar loop
    if (_propagationStack.Contains(this))
    {
        Debug.WriteLine($"LOOP DETECTED at {VariableName}!");
        return;
    }
    
    _propagationStack.Add(this);
    
    try
    {
        // Lógica de propagação...
    }
    finally
    {
        _propagationStack.Remove(this);
    }
}
```

---

## Problemas de Performance

### ❌ Canvas lento com muitas variáveis

**Sintoma:** Lag ao mover ou adicionar variáveis.

**Diagnóstico:**
```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
foreach (var shape in Shapes)
{
    shape.Draw(canvas);
}
stopwatch.Stop();
Debug.WriteLine($"Draw time: {stopwatch.ElapsedMilliseconds}ms for {Shapes.Count} shapes");
```

**Soluções:**

1. **Implementar culling (desenhar só o visível)**
   ```csharp
   public override void Draw(SKCanvas canvas)
   {
       // Pular se fora da tela
       if (!IsVisible(canvas.LocalClipBounds))
           return;
       
       // Desenhar normalmente...
   }
   
   private bool IsVisible(SKRect clipBounds)
   {
       return clipBounds.Contains(Position.X, Position.Y);
   }
   ```

2. **Otimizar criação de Paint objects**
   ```csharp
   // Reusar paints estáticos
   private static readonly SKPaint BorderPaint = new SKPaint
   {
       Color = SKColors.DodgerBlue,
       Style = SKPaintStyle.Stroke,
       StrokeWidth = 3,
       IsAntialias = true
   };
   
   public override void Draw(SKCanvas canvas)
   {
       // Usar paint estático
       canvas.DrawCircle(Position, RADIUS, BorderPaint);
   }
   ```

3. **Batch rendering**
   ```csharp
   // Desenhar todas as variáveis de uma vez
   public static void DrawBatch(SKCanvas canvas, IEnumerable<VariableShape> variables)
   {
       using (var paint = new SKPaint { /* ... */ })
       {
           foreach (var var in variables)
           {
               canvas.DrawCircle(var.Position, RADIUS, paint);
           }
       }
   }
   ```

---

### ❌ Propagação lenta

**Sintoma:** Demora para propagar quando há muitas variáveis conectadas.

**Solução:**
```csharp
// Propagar em lote ao invés de recursivamente
public void PropagateAllVariablesOptimized()
{
    var processed = new HashSet<VariableShape>();
    var queue = new Queue<VariableShape>();
    
    // Adicionar todas as variáveis raiz
    var roots = Shapes.OfType<VariableShape>()
        .Where(v => !v.IncomingVariables.Any());
    
    foreach (var root in roots)
        queue.Enqueue(root);
    
    while (queue.Count > 0)
    {
        var current = queue.Dequeue();
        
        if (processed.Contains(current))
            continue;
        
        processed.Add(current);
        
        // Propagar para vizinhos
        foreach (var neighbor in GetConnectedVariables(current))
        {
            if (!processed.Contains(neighbor))
                queue.Enqueue(neighbor);
        }
    }
}
```

---

## Ferramentas de Debug

### Console de Debug

Adicionar ao MainWindow:

```csharp
private void ShowDebugConsole()
{
    var console = new Window
    {
        Title = "Debug Console",
        Width = 600,
        Height = 400
    };
    
    var textBox = new TextBox
    {
        IsReadOnly = true,
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        FontFamily = new FontFamily("Consolas")
    };
    
    console.Content = textBox;
    
    // Redirecionar Debug.WriteLine
    var writer = new StringWriter();
    Debug.Listeners.Add(new TextWriterTraceListener(writer));
    
    // Atualizar a cada segundo
    var timer = new System.Windows.Threading.DispatcherTimer
    {
        Interval = TimeSpan.FromSeconds(1)
    };
    
    timer.Tick += (s, e) => textBox.Text = writer.ToString();
    timer.Start();
    
    console.Show();
}
```

### Painel de Inspeção

```csharp
private void ShowInspectorPanel(VariableShape variable)
{
    var panel = new Window
    {
        Title = $"Inspector: {variable.VariableName}",
        Width = 400,
        Height = 500
    };
    
    var stack = new StackPanel { Margin = new Thickness(10) };
    
    stack.Children.Add(new TextBlock { Text = $"Name: {variable.VariableName}", FontWeight = FontWeights.Bold });
    stack.Children.Add(new TextBlock { Text = $"Value: {variable.VariableValue}" });
    stack.Children.Add(new TextBlock { Text = $"Position: {variable.Position}" });
    stack.Children.Add(new TextBlock { Text = $"Connections: {variable.CenterNode.ConnectedLines.Count}" });
    
    if (variable.IncomingVariables.Any())
    {
        stack.Children.Add(new TextBlock { Text = "\nIncoming Variables:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 5) });
        foreach (var kvp in variable.IncomingVariables)
        {
            stack.Children.Add(new TextBlock { Text = $"  {kvp.Key} = {kvp.Value}" });
        }
    }
    
    panel.Content = new ScrollViewer { Content = stack };
    panel.Show();
}
```

---

## Checklist de Verificação

Antes de reportar um problema, verificar:

- [ ] Todos os arquivos foram copiados corretamente
- [ ] Projeto compila sem erros
- [ ] Namespaces estão corretos
- [ ] NuGet packages estão instalados (SkiaSharp, etc)
- [ ] MEF está configurado corretamente
- [ ] Plugin DataPlugin tem [Export]
- [ ] ViewModel foi injetado na VariableShape
- [ ] InvalidateCanvas() é chamado após mudanças
- [ ] Debug.WriteLine mostra mensagens esperadas
- [ ] Testes unitários passam (dotnet test)

---

## Obter Ajuda

Se o problema persistir:

1. **Ativar logs detalhados**
   ```csharp
   Debug.AutoFlush = true;
   Trace.Listeners.Add(new TextWriterTraceListener("debug.log"));
   ```

2. **Executar testes**
   ```bash
   dotnet test --logger "console;verbosity=detailed"
   ```

3. **Criar issue no GitHub com:**
   - Versão do .NET
   - Sistema operacional
   - Passos para reproduzir
   - Logs completos
   - Screenshots se aplicável

4. **Consultar documentação:**
   - INTEGRATION_GUIDE.md
   - ARCHITECTURE.md
   - CUSTOMIZATION_GUIDE.md
