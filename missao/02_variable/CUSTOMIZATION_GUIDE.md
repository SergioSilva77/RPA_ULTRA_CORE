# Guia de Customização - Sistema de Variáveis

## Índice
1. [Personalizar Aparência](#personalizar-aparência)
2. [Adicionar Novos Tipos](#adicionar-novos-tipos)
3. [Criar Validações](#criar-validações)
4. [Extensões Avançadas](#extensões-avançadas)
5. [Integração com Outros Sistemas](#integração-com-outros-sistemas)

---

## Personalizar Aparência

### Mudar Cores da Variável

Em `VariableShape.cs`, localize o método `Draw()`:

```csharp
// Cor da borda quando selecionada
Color = IsSelected ? SKColors.Orange : SKColors.DodgerBlue

// Cor de preenchimento
Color = new SKColor(30, 144, 255, 80) // R, G, B, Alpha

// Cor do ícone "V"
Color = SKColors.White
```

**Exemplo: Variável vermelha**
```csharp
Color = IsSelected ? SKColors.Red : new SKColor(220, 50, 50)
Color = new SKColor(220, 50, 50, 80) // Vermelho semi-transparente
```

### Mudar Tamanho do Círculo

```csharp
private const float RADIUS = 25f; // Mude este valor
```

### Customizar Ícone

Substitua o desenho do "V" por outro símbolo:

```csharp
// Desenha um "$" para variáveis monetárias
var path = new SKPath();
path.MoveTo(centerX, centerY - iconSize/2);
path.LineTo(centerX, centerY + iconSize/2);
path.MoveTo(centerX - iconSize/3, centerY - iconSize/4);
path.LineTo(centerX + iconSize/3, centerY + iconSize/4);
canvas.DrawPath(path, iconPaint);
```

### Adicionar Cor por Tipo

```csharp
public class VariableShape : BaseShape
{
    public VariableType Type { get; set; } = VariableType.String;
    
    private SKColor GetColorForType()
    {
        return Type switch
        {
            VariableType.String => SKColors.DodgerBlue,
            VariableType.Number => SKColors.Green,
            VariableType.Boolean => SKColors.Orange,
            VariableType.Object => SKColors.Purple,
            _ => SKColors.Gray
        };
    }
}

public enum VariableType
{
    String,
    Number,
    Boolean,
    Object,
    Array
}
```

---

## Adicionar Novos Tipos

### Criar Variável de Senha

```csharp
public class PasswordVariableShape : VariableShape
{
    public PasswordVariableShape(SKPoint position) : base(position)
    {
        VariableName = "Password";
    }
    
    public override void Draw(SKCanvas canvas)
    {
        base.Draw(canvas);
        
        // Desenha cadeado sobre o ícone
        DrawLockIcon(canvas, Position);
    }
    
    private void DrawLockIcon(SKCanvas canvas, SKPoint center)
    {
        using var paint = new SKPaint
        {
            Color = SKColors.Yellow,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1.5f
        };
        
        // Desenha um pequeno cadeado
        var lockRect = new SKRect(
            center.X - 4, center.Y - 2,
            center.X + 4, center.Y + 4
        );
        canvas.DrawRect(lockRect, paint);
        
        var arcRect = new SKRect(
            center.X - 3, center.Y - 5,
            center.X + 3, center.Y - 1
        );
        canvas.DrawArc(arcRect, 0, 180, false, paint);
    }
}
```

### Criar Item de Inventário

```csharp
public class PasswordInventoryItem : IInventoryItem
{
    public string Id => "item.password";
    public string Name => "Password";
    public string Description => "Secure password variable";
    
    public BaseShape CreateShape(SKPoint position)
    {
        return new PasswordVariableShape(position);
    }
    
    // ... implementar RenderIcon
}
```

### Adicionar ao Plugin

```csharp
public class DataInventorySection : IInventorySection
{
    public IEnumerable<IInventoryItem> GetItems()
    {
        yield return new VariableInventoryItem();
        yield return new PasswordInventoryItem(); // NOVO
    }
}
```

---

## Criar Validações

### Validar Formato de Email

```csharp
public class EmailVariableShape : VariableShape
{
    public EmailVariableShape(SKPoint position) : base(position)
    {
    }
    
    public bool IsValidEmail()
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(VariableValue);
            return addr.Address == VariableValue;
        }
        catch
        {
            return false;
        }
    }
    
    public override void Draw(SKCanvas canvas)
    {
        base.Draw(canvas);
        
        // Desenha indicador de validação
        if (!string.IsNullOrEmpty(VariableValue))
        {
            var color = IsValidEmail() ? SKColors.Green : SKColors.Red;
            DrawValidationIndicator(canvas, Position, color);
        }
    }
}
```

### Validar Tipo Numérico

```csharp
public class NumericVariableShape : VariableShape
{
    public double? NumericValue
    {
        get
        {
            if (double.TryParse(VariableValue, out var result))
                return result;
            return null;
        }
    }
    
    public bool IsValid => NumericValue.HasValue;
    
    public override void Draw(SKCanvas canvas)
    {
        base.Draw(canvas);
        
        if (!IsValid && !string.IsNullOrEmpty(VariableValue))
        {
            // Desenha X vermelho indicando erro
            DrawErrorIcon(canvas, Position);
        }
    }
}
```

---

## Extensões Avançadas

### Sistema de Expressões

```csharp
public class ExpressionVariableShape : VariableShape
{
    public string Expression { get; set; }
    
    public void EvaluateExpression()
    {
        var allVars = GetAllVariables();
        
        // Substitui variáveis na expressão
        var expr = Expression;
        foreach (var kvp in allVars)
        {
            expr = expr.Replace($"{{{kvp.Key}}}", kvp.Value);
        }
        
        // Avalia a expressão (use biblioteca como NCalc)
        try
        {
            var evaluator = new NCalc.Expression(expr);
            VariableValue = evaluator.Evaluate().ToString();
        }
        catch (Exception ex)
        {
            VariableValue = $"Error: {ex.Message}";
        }
    }
}
```

**Exemplo de uso:**
```
var1: value = "10"
var2: value = "20"
result: expression = "{var1} + {var2}"  → value = "30"
```

### Transformadores de Dados

```csharp
public enum TransformType
{
    ToUpperCase,
    ToLowerCase,
    Trim,
    Reverse,
    Length,
    ParseJSON
}

public class TransformerVariableShape : VariableShape
{
    public TransformType Transform { get; set; }
    
    public void ApplyTransform()
    {
        if (IncomingVariables.Count == 0) return;
        
        var input = IncomingVariables.First().Value;
        
        VariableValue = Transform switch
        {
            TransformType.ToUpperCase => input.ToUpper(),
            TransformType.ToLowerCase => input.ToLower(),
            TransformType.Trim => input.Trim(),
            TransformType.Reverse => new string(input.Reverse().ToArray()),
            TransformType.Length => input.Length.ToString(),
            TransformType.ParseJSON => ParseJSON(input),
            _ => input
        };
    }
}
```

### Variáveis Condicionais

```csharp
public class ConditionalVariableShape : VariableShape
{
    public string Condition { get; set; } // Ex: "{age} > 18"
    public string TrueValue { get; set; }
    public string FalseValue { get; set; }
    
    public void Evaluate()
    {
        var allVars = GetAllVariables();
        
        // Substitui variáveis na condição
        var condition = Condition;
        foreach (var kvp in allVars)
        {
            condition = condition.Replace($"{{{kvp.Key}}}", kvp.Value);
        }
        
        // Avalia (simplificado, use biblioteca de expressões)
        bool result = EvaluateCondition(condition);
        VariableValue = result ? TrueValue : FalseValue;
    }
}
```

---

## Integração com Outros Sistemas

### Salvar/Carregar do Banco de Dados

```csharp
public class DatabaseVariableShape : VariableShape
{
    public string ConnectionString { get; set; }
    public string Query { get; set; }
    
    public async Task LoadFromDatabase()
    {
        using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(Query, connection);
        var result = await command.ExecuteScalarAsync();
        
        VariableValue = result?.ToString() ?? "";
        PropagateVariable();
    }
}
```

### Integração com APIs

```csharp
public class APIVariableShape : VariableShape
{
    public string Endpoint { get; set; }
    public string Method { get; set; } = "GET";
    
    public async Task FetchFromAPI()
    {
        var allVars = GetAllVariables();
        
        // Substitui variáveis no endpoint
        var url = Endpoint;
        foreach (var kvp in allVars)
        {
            url = url.Replace($"{{{kvp.Key}}}", kvp.Value);
        }
        
        using var client = new HttpClient();
        var response = await client.GetStringAsync(url);
        
        VariableValue = response;
        PropagateVariable();
    }
}
```

### Variáveis do Sistema

```csharp
public class SystemVariableShape : VariableShape
{
    public SystemVariableType SysType { get; set; }
    
    public void LoadSystemValue()
    {
        VariableValue = SysType switch
        {
            SystemVariableType.CurrentDate => DateTime.Now.ToString("yyyy-MM-dd"),
            SystemVariableType.CurrentTime => DateTime.Now.ToString("HH:mm:ss"),
            SystemVariableType.UserName => Environment.UserName,
            SystemVariableType.MachineName => Environment.MachineName,
            SystemVariableType.ProcessId => Process.GetCurrentProcess().Id.ToString(),
            _ => ""
        };
    }
}

public enum SystemVariableType
{
    CurrentDate,
    CurrentTime,
    UserName,
    MachineName,
    ProcessId,
    WorkingDirectory,
    TempPath
}
```

---

## Painel de Debug Avançado

```csharp
public class VariableDebugPanel : Window
{
    private DataGrid _grid;
    private SketchViewModel _viewModel;
    
    public VariableDebugPanel(SketchViewModel viewModel)
    {
        _viewModel = viewModel;
        Title = "Variable Debugger";
        Width = 600;
        Height = 400;
        
        InitializeUI();
        RefreshData();
    }
    
    private void InitializeUI()
    {
        _grid = new DataGrid
        {
            AutoGenerateColumns = false,
            IsReadOnly = true
        };
        
        _grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Name",
            Binding = new Binding("Name")
        });
        
        _grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Value",
            Binding = new Binding("Value")
        });
        
        _grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Type",
            Binding = new Binding("Type")
        });
        
        _grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Connections",
            Binding = new Binding("ConnectionCount")
        });
        
        Content = _grid;
    }
    
    public void RefreshData()
    {
        var variables = _viewModel.Shapes
            .OfType<VariableShape>()
            .Select(v => new
            {
                Name = v.VariableName,
                Value = v.VariableValue,
                Type = GetValueType(v.VariableValue),
                ConnectionCount = v.CenterNode.ConnectedLines.Count
            });
        
        _grid.ItemsSource = variables.ToList();
    }
    
    private string GetValueType(string value)
    {
        if (string.IsNullOrEmpty(value)) return "Empty";
        if (int.TryParse(value, out _)) return "Integer";
        if (double.TryParse(value, out _)) return "Number";
        if (bool.TryParse(value, out _)) return "Boolean";
        if (value.StartsWith("{") || value.StartsWith("[")) return "JSON";
        return "String";
    }
}
```

---

## Serialização para JSON

```csharp
public class VariableSerializer
{
    public static string SerializeAll(IEnumerable<VariableShape> variables)
    {
        var data = variables.Select(v => new
        {
            v.VariableName,
            v.VariableValue,
            Position = new { v.Position.X, v.Position.Y },
            IncomingVars = v.IncomingVariables
        });
        
        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
    
    public static IEnumerable<VariableShape> Deserialize(string json)
    {
        var data = JsonSerializer.Deserialize<List<VariableData>>(json);
        
        return data.Select(d => new VariableShape(new SKPoint(d.X, d.Y))
        {
            VariableName = d.Name,
            VariableValue = d.Value
        });
    }
    
    private class VariableData
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}
```

---

## Atalhos e Produtividade

### Adicionar Menu de Contexto

```csharp
public void AddContextMenu(VariableShape variable)
{
    var menu = new ContextMenu();
    
    // Editar
    var editItem = new MenuItem { Header = "Edit Variable" };
    editItem.Click += (s, e) => OpenVariableEditor(variable);
    menu.Items.Add(editItem);
    
    // Copiar valor
    var copyItem = new MenuItem { Header = "Copy Value" };
    copyItem.Click += (s, e) => Clipboard.SetText(variable.VariableValue);
    menu.Items.Add(copyItem);
    
    // Duplicar
    var duplicateItem = new MenuItem { Header = "Duplicate" };
    duplicateItem.Click += (s, e) => DuplicateVariable(variable);
    menu.Items.Add(duplicateItem);
    
    // Deletar
    var deleteItem = new MenuItem { Header = "Delete" };
    deleteItem.Click += (s, e) => DeleteVariable(variable);
    menu.Items.Add(deleteItem);
    
    menu.IsOpen = true;
}
```

---

## Conclusão

Este sistema de variáveis é altamente extensível! Você pode:

- ✅ Customizar cores e ícones
- ✅ Adicionar validações e tipos
- ✅ Criar transformadores de dados
- ✅ Integrar com APIs e bancos de dados
- ✅ Adicionar expressões matemáticas
- ✅ Criar painéis de debug
- ✅ Serializar/deserializar dados

**Dica Final:** Sempre mantenha a simplicidade visual no canvas, mesmo com funcionalidades complexas!
