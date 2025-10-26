# Arquitetura do Sistema de Variáveis

## Diagrama de Componentes

```
┌─────────────────────────────────────────────────────────────────────┐
│                         RPA MECHANICS                                │
│                                                                       │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                      INVENTÁRIO (UI)                         │   │
│  │  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐   │   │
│  │  │ Shapes   │  │  Lines   │  │  *DATA*  │  │  Tools   │   │   │
│  │  └──────────┘  └──────────┘  └────┬─────┘  └──────────┘   │   │
│  │                                    │                         │   │
│  │                              ┌─────▼──────┐                 │   │
│  │                              │  Variable  │ ◄─ Item          │   │
│  │                              └────────────┘                 │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                     │                              │
│                                     │ CreateShape()                │
│                                     ▼                              │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                     CANVAS (SkiaSharp)                       │   │
│  │                                                               │   │
│  │    ⚪ VariableShape ──────► ⚪ VariableShape                │   │
│  │   (V) userName             (V) email                         │   │
│  │    │                        │                                │   │
│  │    │   LineShape            │                                │   │
│  │    └──────────────────────►│                                │   │
│  │         (propagação)        │                                │   │
│  │                             ▼                                │   │
│  │                        ⚪ VariableShape                      │   │
│  │                       (V) fullName                           │   │
│  │                       [recebe: userName, email]              │   │
│  │                                                               │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                     │                              │
│                           [Duplo Clique]                           │
│                                     ▼                              │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │              VARIABLE EDITOR DIALOG (WPF)                    │   │
│  │                                                               │   │
│  │  ┌─────────────────────────────────────────────────────┐   │   │
│  │  │ Variable Name: [_________________]                  │   │   │
│  │  │                                                      │   │   │
│  │  │ Variable Value:                                     │   │   │
│  │  │ ┌────────────────────────────────────────────────┐ │   │   │
│  │  │ │                                                │ │   │   │
│  │  │ │                                                │ │   │   │
│  │  │ └────────────────────────────────────────────────┘ │   │   │
│  │  │                                                      │   │   │
│  │  │                           [Cancel]  [OK]            │   │   │
│  │  └─────────────────────────────────────────────────────┘   │   │
│  │                                                               │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                     │                              │
│                                     │ Save                         │
│                                     ▼                              │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                  PROPAGATION ENGINE                          │   │
│  │                                                               │   │
│  │  1. Atualiza VariableShape                                   │   │
│  │  2. Coleta todas IncomingVariables                           │   │
│  │  3. Adiciona própria variável                                │   │
│  │  4. Percorre ConnectedLines                                  │   │
│  │  5. Encontra VariableShape de destino                        │   │
│  │  6. Passa variáveis para destino                             │   │
│  │  7. Recursivamente propaga                                   │   │
│  │                                                               │   │
│  └───────────────────────────────────────────────────────────┘   │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

## Fluxo de Dados Detalhado

```
┌─────────────┐
│   INÍCIO    │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────────┐
│ Usuário pressiona E             │
│ (abre inventário)               │
└──────┬──────────────────────────┘
       │
       ▼
┌─────────────────────────────────┐
│ Usuário navega para seção DATA  │
│ (DataPlugin carregado via MEF)  │
└──────┬──────────────────────────┘
       │
       ▼
┌─────────────────────────────────┐
│ Usuário arrasta Variable        │
│ para o canvas                   │
└──────┬──────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│ VariableInventoryItem.CreateShape()    │
│ → new VariableShape(mousePosition)     │
└──────┬──────────────────────────────────┘
       │
       ▼
┌─────────────────────────────────────────┐
│ VariableShape adicionada ao Canvas     │
│ - Cria Node central                    │
│ - Desenha círculo azul com "V"         │
│ - Adiciona à lista Shapes              │
└──────┬──────────────────────────────────┘
       │
       ├─────────────────────┬──────────────────────┐
       │                     │                      │
       ▼                     ▼                      ▼
┌──────────────┐    ┌────────────────┐    ┌──────────────┐
│ Usuário      │    │ Usuário        │    │ Usuário      │
│ move shape   │    │ conecta linhas │    │ duplo-clica  │
└──────┬───────┘    └────────┬───────┘    └──────┬───────┘
       │                     │                     │
       ▼                     ▼                     ▼
┌──────────────┐    ┌────────────────┐    ┌──────────────────┐
│ Move()       │    │ SHIFT+Drag     │    │ HandleVariable   │
│ atualiza     │    │ cria LineShape │    │ DoubleClick()    │
│ Position     │    │ conecta Nodes  │    └──────┬───────────┘
└──────────────┘    └────────────────┘           │
                                                  ▼
                                         ┌────────────────────┐
                                         │ VariableEditor     │
                                         │ Dialog é aberto    │
                                         └─────────┬──────────┘
                                                   │
                                                   ▼
                                         ┌────────────────────┐
                                         │ Usuário preenche:  │
                                         │ - Nome             │
                                         │ - Valor            │
                                         └─────────┬──────────┘
                                                   │
                                                   ▼
                                         ┌────────────────────┐
                                         │ Clica OK           │
                                         │ (ou Ctrl+Enter)    │
                                         └─────────┬──────────┘
                                                   │
                                                   ▼
                                         ┌────────────────────────┐
                                         │ VariableShape.         │
                                         │ VariableName atualizado│
                                         │ VariableValue atualizado│
                                         └─────────┬──────────────┘
                                                   │
                                                   ▼
                                         ┌────────────────────────┐
                                         │ PropagateVariable()    │
                                         │ é chamado              │
                                         └─────────┬──────────────┘
                                                   │
                                                   ▼
                    ┌──────────────────────────────┴────────────────────────────┐
                    │                                                            │
                    ▼                                                            ▼
         ┌──────────────────────┐                                    ┌──────────────────┐
         │ Para cada LineShape  │                                    │ Para cada Node   │
         │ conectada ao Node    │                                    │ conectado        │
         └──────────┬───────────┘                                    └────────┬─────────┘
                    │                                                          │
                    ▼                                                          ▼
         ┌──────────────────────┐                                    ┌─────────────────┐
         │ Encontra destino     │                                    │ Encontra        │
         │ (outro VariableShape)│                                    │ VariableShape   │
         └──────────┬───────────┘                                    │ associada       │
                    │                                                 └────────┬────────┘
                    ▼                                                          │
         ┌──────────────────────────────┐                                     │
         │ Adiciona variável em         │                                     │
         │ destino.IncomingVariables    │ ◄───────────────────────────────────┘
         └──────────┬───────────────────┘
                    │
                    ▼
         ┌──────────────────────────────┐
         │ Recursivamente chama         │
         │ PropagateVariable() no       │
         │ destino                      │
         └──────────┬───────────────────┘
                    │
                    ▼
         ┌──────────────────────────────┐
         │ InvalidateCanvas()           │
         │ (redesenha tudo)             │
         └──────────────────────────────┘
```

## Estrutura de Classes

```
┌────────────────────────────────────────────────────────────────┐
│                         BaseShape                              │
│  ┌──────────────────────────────────────────────────────────┐ │
│  │ + bool IsSelected                                        │ │
│  │ + abstract void Draw(SKCanvas)                          │ │
│  │ + abstract bool HitTest(SKPoint)                        │ │
│  │ + abstract void Move(float, float)                      │ │
│  │ + abstract BaseShape Clone()                            │ │
│  └──────────────────────────────────────────────────────────┘ │
└─────────────────────────┬──────────────────────────────────────┘
                          │
              ┌───────────┴───────────┐
              │                       │
              ▼                       ▼
┌─────────────────────────┐  ┌────────────────────────┐
│   VariableShape         │  │   CircleShape          │
│  ┌────────────────────┐ │  │   RectShape            │
│  │ Properties:        │ │  │   LineShape            │
│  │ - VariableName     │ │  │   etc...               │
│  │ - VariableValue    │ │  └────────────────────────┘
│  │ - Position         │ │
│  │ - CenterNode       │ │
│  │ - IncomingVars     │ │
│  │                    │ │
│  │ Methods:           │ │
│  │ + Draw()           │ │
│  │ + HitTest()        │ │
│  │ + Move()           │ │
│  │ + PropagateVar()   │ │
│  │ + GetAllVars()     │ │
│  └────────────────────┘ │
└─────────────────────────┘
           │
           │ has-a
           ▼
┌─────────────────────────┐
│        Node             │
│  ┌────────────────────┐ │
│  │ - X, Y             │ │
│  │ - ConnectedLines   │ │
│  │ + Draw()           │ │
│  │ + HitTest()        │ │
│  │ + NotifyChanged()  │ │
│  └────────────────────┘ │
└─────────────────────────┘
```

## Sistema de Plugins (MEF)

```
┌──────────────────────────────────────────────────────────┐
│                    Plugin System (MEF)                    │
│                                                            │
│  ┌────────────────────────────────────────────────────┐  │
│  │              IPlugin Interface                      │  │
│  │  ┌──────────────────────────────────────────────┐  │  │
│  │  │ + string Id                                  │  │  │
│  │  │ + string Name                                │  │  │
│  │  │ + string Version                             │  │  │
│  │  │ + IEnumerable<IInventorySection> GetSections│  │  │
│  │  └──────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────┘  │
│                          │                                │
│                          │ implements                     │
│                          ▼                                │
│  ┌────────────────────────────────────────────────────┐  │
│  │         [Export(typeof(IPlugin))]                   │  │
│  │              DataPlugin                             │  │
│  │  ┌──────────────────────────────────────────────┐  │  │
│  │  │ Id = "plugin.data"                           │  │  │
│  │  │ Name = "Data Management"                     │  │  │
│  │  │ Version = "1.0.0"                            │  │  │
│  │  │                                              │  │  │
│  │  │ GetSections() → DataInventorySection         │  │  │
│  │  └──────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────┘  │
│                          │                                │
│                          │ provides                       │
│                          ▼                                │
│  ┌────────────────────────────────────────────────────┐  │
│  │          IInventorySection                          │  │
│  │  ┌──────────────────────────────────────────────┐  │  │
│  │  │ + string Id                                  │  │  │
│  │  │ + string Name                                │  │  │
│  │  │ + IEnumerable<IInventoryItem> GetItems()     │  │  │
│  │  └──────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────┘  │
│                          │                                │
│                          │ implements                     │
│                          ▼                                │
│  ┌────────────────────────────────────────────────────┐  │
│  │        DataInventorySection                         │  │
│  │  ┌──────────────────────────────────────────────┐  │  │
│  │  │ Id = "section.data"                          │  │  │
│  │  │ Name = "Data"                                │  │  │
│  │  │                                              │  │  │
│  │  │ GetItems() → VariableInventoryItem           │  │  │
│  │  └──────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────┘  │
│                          │                                │
│                          │ provides                       │
│                          ▼                                │
│  ┌────────────────────────────────────────────────────┐  │
│  │          IInventoryItem                             │  │
│  │  ┌──────────────────────────────────────────────┐  │  │
│  │  │ + string Id                                  │  │  │
│  │  │ + string Name                                │  │  │
│  │  │ + BaseShape CreateShape(SKPoint)             │  │  │
│  │  │ + void RenderIcon(SKCanvas, ...)             │  │  │
│  │  └──────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────┘  │
│                          │                                │
│                          │ implements                     │
│                          ▼                                │
│  ┌────────────────────────────────────────────────────┐  │
│  │        VariableInventoryItem                        │  │
│  │  ┌──────────────────────────────────────────────┐  │  │
│  │  │ Id = "item.variable"                         │  │  │
│  │  │ Name = "Variable"                            │  │  │
│  │  │                                              │  │  │
│  │  │ CreateShape() → new VariableShape()          │  │  │
│  │  │ RenderIcon() → desenha círculo azul com V    │  │  │
│  │  └──────────────────────────────────────────────┘  │  │
│  └────────────────────────────────────────────────────┘  │
│                                                            │
└──────────────────────────────────────────────────────────┘
```

## Ciclo de Vida Completo

```
[APP START]
     │
     ▼
[MEF carrega plugins] ──► DataPlugin encontrado
     │
     ▼
[DataPlugin.GetSections()] ──► DataInventorySection criada
     │
     ▼
[DataInventorySection.GetItems()] ──► VariableInventoryItem criado
     │
     ▼
[Inventário renderizado] ──► Seção DATA aparece com item Variable
     │
     ▼
[USER INPUT: Pressiona E] ──► Inventário abre
     │
     ▼
[USER INPUT: Arrasta Variable] ──► CreateShape(mousePos)
     │
     ▼
[VariableShape criada] ──► Adicionada ao Canvas
     │                      Node central criado
     ▼                      Desenhada na posição
[CANVAS ATUALIZADO]
     │
     ▼
[USER INPUT: Duplo clique] ──► HandleVariableDoubleClick()
     │
     ▼
[VariableEditorDialog abre]
     │
     ▼
[USER INPUT: Preenche e salva] ──► Nome e valor atualizados
     │
     ▼
[PropagateVariable()] ──► Dados fluem pelos galhos
     │
     ▼
[InvalidateCanvas()] ──► Canvas redesenhado
     │
     ▼
[SISTEMA PRONTO PARA PRÓXIMA INTERAÇÃO]
```

## Dependências entre Arquivos

```
VariableEditorDialog.cs
         │
         │ depends on
         ▼
   VariableShape.cs ◄────────── VariableInventoryItem.cs
         │                              │
         │ used by                      │ creates
         ▼                              │
SketchViewModelExtensions.cs            │
         │                              │
         │ extends                      │
         ▼                              │
  SketchViewModel                       │
         │                              │
         │                              │
         └──────────────────────────────┘
                     │
                     │ uses
                     ▼
               DataPlugin.cs
                     │
                     │ registers
                     ▼
                MEF System
```

Esta arquitetura garante:
- ✅ Separação de responsabilidades
- ✅ Baixo acoplamento
- ✅ Alta coesão
- ✅ Facilidade de teste
- ✅ Extensibilidade
