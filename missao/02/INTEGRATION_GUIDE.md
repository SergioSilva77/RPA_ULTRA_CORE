# Integração do Sistema de Variáveis - RPA Mechanics

## Arquivos Criados

Este sistema adiciona as seguintes funcionalidades ao projeto:

### 1. **Models/Geometry/VariableShape.cs**
Classe principal que representa uma variável no canvas. 

**Funcionalidades:**
- Armazena nome e valor da variável
- Possui um nó central para conexões
- Propaga dados para variáveis conectadas
- Renderiza visualmente com ícone "V"
- Gerencia variáveis recebidas de outras conexões

### 2. **Views/VariableEditorDialog.cs**
Janela de diálogo para editar variáveis.

**Funcionalidades:**
- Campo de nome da variável
- Campo de valor (multilinhas)
- Validação de entrada
- Tema escuro (consistente com o app)
- Atalhos de teclado (Ctrl+Enter, Esc)

### 3. **Inventory/Items/VariableInventoryItem.cs**
Item do inventário para criar variáveis.

**Funcionalidades:**
- Define como a variável aparece no inventário
- Renderiza o ícone no slot
- Cria instâncias de VariableShape ao arrastar

### 4. **Plugins/Data/DataPlugin.cs**
Plugin que adiciona a seção "Data" ao inventário.

**Funcionalidades:**
- Registra a seção "Data"
- Adiciona o item Variable à seção
- Renderiza ícone de banco de dados na aba

### 5. **ViewModels/SketchViewModelExtensions.cs**
Extensões para o SketchViewModel.

**Funcionalidades:**
- `HandleVariableDoubleClick()`: Detecta duplo clique em variáveis
- `OpenVariableEditor()`: Abre a janela de edição
- `FindShapeByNode()`: Encontra shapes por nó conectado
- `PropagateAllVariables()`: Propaga todas as variáveis
- `GetVariablesDebugInfo()`: Retorna info de debug

## Instruções de Integração

### Passo 1: Adicionar arquivos ao projeto

Copie todos os arquivos criados para seus respectivos diretórios no projeto:

```
RPA_ULTRA_CORE/
├── Models/
│   └── Geometry/
│       └── VariableShape.cs          [NOVO]
├── Views/
│   └── VariableEditorDialog.cs       [NOVO]
├── Inventory/
│   └── Items/
│       └── VariableInventoryItem.cs  [NOVO]
├── Plugins/
│   └── Data/
│       └── DataPlugin.cs             [NOVO]
└── ViewModels/
    └── SketchViewModelExtensions.cs  [NOVO]
```

### Passo 2: Atualizar o .csproj

Adicione as referências se necessário:

```xml
<ItemGroup>
  <Compile Include="Models\Geometry\VariableShape.cs" />
  <Compile Include="Views\VariableEditorDialog.cs" />
  <Compile Include="Inventory\Items\VariableInventoryItem.cs" />
  <Compile Include="Plugins\Data\DataPlugin.cs" />
  <Compile Include="ViewModels\SketchViewModelExtensions.cs" />
</ItemGroup>
```

### Passo 3: Modificar SketchViewModel

Adicione ao arquivo `ViewModels/SketchViewModel.cs`:

#### 3.1. No método de detecção de duplo clique

Localize onde o canvas detecta cliques duplos e adicione:

```csharp
protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
{
    var point = e.GetPosition(this);
    var skPoint = new SKPoint((float)point.X, (float)point.Y);
    
    // Tenta manipular duplo clique em variável
    HandleVariableDoubleClick(skPoint);
    
    // ... resto do código existente
}
```

#### 3.2. Na criação de shapes

Quando uma shape é adicionada ao canvas (método `AddShape` ou similar):

```csharp
public void AddShape(BaseShape shape)
{
    Shapes.Add(shape);
    
    // Se for variável, configura a referência ao ViewModel
    if (shape is VariableShape varShape)
    {
        // Permite que a variável encontre outras shapes
        // (isso já está implementado no SketchViewModelExtensions)
    }
    
    InvalidateCanvas();
}
```

#### 3.3. Quando linhas são criadas/modificadas

Para garantir que a propagação funcione, adicione:

```csharp
private void OnLineCreated(LineShape line)
{
    // ... código existente ...
    
    // Propaga variáveis quando nova linha é criada
    PropagateAllVariables();
}

private void OnLineDeleted(LineShape line)
{
    // ... código existente ...
    
    // Repropaga variáveis quando linha é removida
    PropagateAllVariables();
}
```

### Passo 4: Corrigir método FindShapeByNode

No arquivo `VariableShape.cs`, o método `FindShapeByNode` precisa acessar o ViewModel. 

**Opção A: Injetar referência ao ViewModel**

```csharp
public class VariableShape : BaseShape
{
    public SketchViewModel ViewModel { get; set; }
    
    private BaseShape FindShapeByNode(Node node)
    {
        return ViewModel?.FindShapeByNode(node);
    }
}
```

E ao criar a variável:

```csharp
var varShape = new VariableShape(position)
{
    ViewModel = this // referência ao SketchViewModel
};
```

**Opção B: Usar EventBus (já existente no projeto)**

```csharp
// Na VariableShape
private BaseShape FindShapeByNode(Node node)
{
    var request = new FindShapeRequest { Node = node };
    EventBus.Instance.Publish("FindShapeByNode", request);
    return request.Result;
}

// No SketchViewModel
private void SetupEventBus()
{
    EventBus.Instance.Subscribe<FindShapeRequest>("FindShapeByNode", request =>
    {
        request.Result = FindShapeByNode(request.Node);
    });
}
```

### Passo 5: Testar

1. Compile o projeto: `dotnet build`
2. Execute: `dotnet run --project RPA_ULTRA_CORE`
3. Pressione **E** para abrir o inventário
4. Deve aparecer a nova seção "Data" com o item "Variable"
5. Arraste o item Variable para o canvas
6. Dê duplo clique no círculo azul criado
7. A janela de edição deve abrir

## Exemplo de Uso Completo

```csharp
// Código de teste/exemplo
public void TestVariableSystem()
{
    // Cria duas variáveis
    var var1 = new VariableShape(new SKPoint(100, 100))
    {
        VariableName = "firstName",
        VariableValue = "João"
    };
    
    var var2 = new VariableShape(new SKPoint(300, 100))
    {
        VariableName = "lastName", 
        VariableValue = "Silva"
    };
    
    // Adiciona ao canvas
    Shapes.Add(var1);
    Shapes.Add(var2);
    
    // Conecta as variáveis
    var line = new LineShape(var1.CenterNode, var2.CenterNode);
    Shapes.Add(line);
    
    // Propaga os dados
    var1.PropagateVariable();
    
    // var2 agora tem acesso a:
    // IncomingVariables["firstName"] = "João"
    // E também tem sua própria: lastName = "Silva"
    
    var allVars = var2.GetAllVariables();
    // allVars contém: firstName="João", lastName="Silva"
}
```

## Possíveis Problemas e Soluções

### Problema 1: Variável não aparece no inventário

**Solução:** Verifique se o plugin foi carregado:
- O MEF está configurado corretamente?
- O atributo `[Export(typeof(IPlugin))]` está presente?
- A DLL está na pasta Plugins?

### Problema 2: Duplo clique não funciona

**Solução:** 
- Verifique se `HandleVariableDoubleClick` foi integrado
- Confirme que o evento de duplo clique está sendo capturado
- Teste com Debug.WriteLine para confirmar que o método é chamado

### Problema 3: Janela de edição não abre

**Solução:**
- Verifique se há erros de compilação em `VariableEditorDialog.cs`
- Confirme que `Application.Current.MainWindow` existe
- Tente sem definir Owner da janela inicialmente

### Problema 4: Variáveis não propagam

**Solução:**
- Confirme que as linhas estão conectadas aos nós centrais
- Verifique se `FindShapeByNode` retorna resultados válidos
- Use `GetVariablesDebugInfo()` para debugar o estado

## Próximos Passos

1. **Adicionar painel de debug**: Criar uma janela que lista todas as variáveis e seus valores
2. **Expressões**: Permitir que variáveis calculem valores baseados em outras
3. **Tipos de dados**: Adicionar validação de tipos (string, number, boolean, etc.)
4. **Serialização**: Salvar/carregar variáveis com o projeto
5. **API de acesso**: Permitir que outros componentes acessem variáveis por nome

## Documentação Adicional

Veja `VARIABLES_GUIDE.md` para o guia do usuário completo.

## Suporte

Para dúvidas sobre a integração, abra uma issue no repositório do GitHub.
