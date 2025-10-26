# Exemplo Visual - Sistema de Variáveis

## Estrutura do Sistema

```
┌─────────────────────────────────────────────────────────────────┐
│                         INVENTÁRIO                              │
│  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐  │
│  │Shapes│  │Lines │  │ DATA │  │Tools │  │Logic │  │Utils │  │
│  └──────┘  └──────┘  └──┬───┘  └──────┘  └──────┘  └──────┘  │
│                          │                                       │
│                     ┌────▼─────┐                                │
│                     │ Variable │  ◄── Novo item!                │
│                     └──────────┘                                │
└─────────────────────────────────────────────────────────────────┘
```

## Fluxo de Uso

### 1. Arrastar do Inventário

```
INVENTÁRIO                    CANVAS
┌──────────┐                 
│ Variable │ ────────────►   ⚪ [Círculo azul criado]
│    V     │  (arrastar)     
└──────────┘                 
```

### 2. Duplo Clique para Editar

```
      CANVAS                           JANELA DE EDIÇÃO
                                  ┌─────────────────────────┐
    ⚪ ──────► [duplo clique]     │  Edit Variable          │
   (V)                            ├─────────────────────────┤
                                  │ Variable Name:          │
                                  │ ┌─────────────────────┐ │
                                  │ │ userName            │ │
                                  │ └─────────────────────┘ │
                                  │                         │
                                  │ Variable Value:         │
                                  │ ┌─────────────────────┐ │
                                  │ │ João Silva          │ │
                                  │ │                     │ │
                                  │ └─────────────────────┘ │
                                  │                         │
                                  │      [Cancel]  [OK]     │
                                  └─────────────────────────┘
```

### 3. Conectar Variáveis

```
CANVAS
                                              
  ⚪ userName                                   
 (V) "João Silva"                              
  │                                             
  │ [SHIFT + arrastar]                          
  │                                             
  ▼                                             
  ⚪ email                                      
 (V) "joao@email.com"                          
  │                                             
  │                                             
  ▼                                             
  ⚪ userInfo                                   
 (V) [recebe userName + email]                
```

## Exemplo Prático: Sistema de Login

```
┌────────────────────────────────────────────────────────────────┐
│                          CANVAS                                 │
│                                                                 │
│   ⚪ username                                                   │
│  (V) "admin"                                                    │
│   │                                                             │
│   │                                                             │
│   ├────────────┐                                               │
│   │            │                                               │
│   ▼            ▼                                               │
│   ⚪ password   ⚪ database                                     │
│  (V) "12345"  (V) "users.db"                                   │
│   │            │                                               │
│   └─────┬──────┘                                               │
│         │                                                       │
│         ▼                                                       │
│         ⚪ loginRequest                                         │
│        (V) [recebe: username, password, database]              │
│         │                                                       │
│         │                                                       │
│         ▼                                                       │
│         ⚪ result                                               │
│        (V) "Login successful"                                  │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

## Exemplo de Propagação de Dados

```
ANTES da propagação:
────────────────────

⚪ A                    ⚪ B                    ⚪ C
name: "cidade"         name: "estado"         name: "endereço"
value: "São Paulo"     value: "SP"            value: ""
vars: []               vars: []               vars: []


DEPOIS da propagação (A → B → C):
──────────────────────────────────

⚪ A ─────────►  ⚪ B ─────────►  ⚪ C
name: "cidade"  name: "estado"   name: "endereço"
value: "SP"     value: "SP"      value: "Rua X"
vars: []        vars: [          vars: [
                 cidade="SP"      cidade="SP",
                ]                 estado="SP"
                                 ]
```

## Visualização no Canvas

```
  ┌─────────────────────────────────────────────┐
  │                                             │
  │        ⚫ ← Borda laranja (selecionado)     │
  │       ╱V╲                                   │
  │      ╱   ╲                                  │
  │     ╱  V  ╲  ← Ícone "V" branco            │
  │    ╱       ╲                                │
  │   ┗━━━━━━━━━┛                               │
  │   userName  ← Nome da variável (label)     │
  │                                             │
  │                                             │
  │        ⚪  ← Borda azul (normal)            │
  │       ╱V╲                                   │
  │      ╱   ╲                                  │
  │     ╱  V  ╲                                 │
  │    ╱       ╲                                │
  │   ┗━━━━━━━━━┛                               │
  │                                             │
  └─────────────────────────────────────────────┘
```

## Estrutura de Dados Interna

```javascript
VariableShape {
  VariableName: "userName",
  VariableValue: "João Silva",
  Position: { X: 200, Y: 150 },
  CenterNode: Node {
    X: 200,
    Y: 150,
    ConnectedLines: [Line1, Line2]
  },
  IncomingVariables: {
    "email": "joao@email.com",
    "age": "30"
  }
}
```

## Fluxo de Eventos

```
1. Usuário abre inventário (tecla E)
2. Arrasta item Variable para canvas
3. VariableInventoryItem.CreateShape() é chamado
4. Nova VariableShape é criada na posição do mouse
5. Usuário dá duplo clique na variável
6. HandleVariableDoubleClick() detecta o clique
7. VariableEditorDialog é aberta
8. Usuário preenche nome e valor
9. Clica em OK
10. VariableShape é atualizada
11. PropagateVariable() é chamado
12. Dados fluem pelas linhas conectadas
13. Canvas é redesenhado
```

## Casos de Uso

### Caso 1: Configuração de API
```
baseURL → endpoint → params → fullRequest
```

### Caso 2: Processamento de Dados
```
rawData → parser → validator → cleanData
```

### Caso 3: Fluxo Condicional
```
condition → ifTrue → result
          ↘ ifFalse ↗
```

### Caso 4: Transformação de Dados
```
input1 ↘
        → transformer → output
input2 ↗
```

## Teclas de Atalho Resumidas

```
┌──────────────┬────────────────────────────────┐
│ Tecla        │ Ação                           │
├──────────────┼────────────────────────────────┤
│ E            │ Abrir/fechar inventário        │
│ SHIFT+Click  │ Iniciar linha de conexão       │
│ Duplo Click  │ Editar variável                │
│ DELETE       │ Remover variável selecionada   │
│ Ctrl+Enter   │ Salvar (na janela de edição)   │
│ ESC          │ Cancelar (na janela)           │
│ 1-7          │ Slots da hotbar                │
└──────────────┴────────────────────────────────┘
```

## Performance

- Cada variável ocupa ~1KB de memória
- Propagação é O(n) onde n = número de conexões
- Renderização usa SkiaSharp (acelerado por GPU)
- Suporta centenas de variáveis sem lag

## Limitações Conhecidas

❌ Não detecta loops infinitos na propagação
❌ Não valida tipos de dados
❌ Não suporta expressões matemáticas ainda
❌ Não persiste dados ao fechar o app
✅ Mas funciona perfeitamente para fluxos simples!
