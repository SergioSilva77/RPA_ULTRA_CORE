# 📚 Documentação Completa - Sistema de Variáveis RPA Mechanics

## 🎯 Início Rápido

**Novo no projeto?** Comece aqui:
1. Leia o [README.md](./README.md) para visão geral
2. Siga o [INTEGRATION_GUIDE.md](./INTEGRATION_GUIDE.md) para integrar
3. Consulte [VARIABLES_GUIDE.md](./VARIABLES_GUIDE.md) para aprender a usar

---

## 📖 Documentação por Tipo de Usuário

### 👨‍💻 Para Desenvolvedores

| Documento | Descrição | Quando Usar |
|-----------|-----------|-------------|
| [README.md](./README.md) | Visão geral completa do sistema | Primeira leitura obrigatória |
| [INTEGRATION_GUIDE.md](./INTEGRATION_GUIDE.md) | Passo a passo de integração | Ao adicionar ao projeto |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Arquitetura e fluxo de dados | Para entender a estrutura |
| [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) | Solução de problemas | Quando encontrar erros |
| [Tests/VariableSystemTests.cs](./Tests/VariableSystemTests.cs) | Testes unitários | Para validar funcionamento |

### 👨‍🎨 Para Designers/Customizadores

| Documento | Descrição | Quando Usar |
|-----------|-----------|-------------|
| [CUSTOMIZATION_GUIDE.md](./CUSTOMIZATION_GUIDE.md) | Guia de personalização | Para modificar aparência |
| [VISUAL_EXAMPLE.md](./VISUAL_EXAMPLE.md) | Exemplos visuais | Para ver como funciona |

### 👥 Para Usuários Finais

| Documento | Descrição | Quando Usar |
|-----------|-----------|-------------|
| [VARIABLES_GUIDE.md](./VARIABLES_GUIDE.md) | Manual do usuário | Para aprender a usar |
| [VISUAL_EXAMPLE.md](./VISUAL_EXAMPLE.md) | Exemplos práticos | Para ver casos de uso |

---

## 📂 Estrutura dos Arquivos

```
📁 outputs/
├── 📄 INDEX.md                          ← Você está aqui!
├── 📄 README.md                         ← Comece por aqui
├── 📄 INTEGRATION_GUIDE.md              ← Como integrar
├── 📄 VARIABLES_GUIDE.md                ← Como usar
├── 📄 ARCHITECTURE.md                   ← Arquitetura do sistema
├── 📄 VISUAL_EXAMPLE.md                 ← Exemplos visuais
├── 📄 CUSTOMIZATION_GUIDE.md            ← Como customizar
├── 📄 TROUBLESHOOTING.md                ← Solução de problemas
│
├── 📁 Models/
│   └── 📁 Geometry/
│       └── 📄 VariableShape.cs          ← Classe principal da variável
│
├── 📁 Views/
│   └── 📄 VariableEditorDialog.cs       ← Janela de edição
│
├── 📁 ViewModels/
│   └── 📄 SketchViewModelExtensions.cs  ← Extensões do ViewModel
│
├── 📁 Inventory/
│   └── 📁 Items/
│       └── 📄 VariableInventoryItem.cs  ← Item do inventário
│
├── 📁 Plugins/
│   └── 📁 Data/
│       └── 📄 DataPlugin.cs             ← Plugin MEF
│
└── 📁 Tests/
    └── 📄 VariableSystemTests.cs        ← Testes unitários (21 testes)
```

---

## 🗺️ Mapa de Navegação

### Fluxo de Leitura Recomendado

```
1. README.md
   ↓
   ├─→ [Quer integrar?] → INTEGRATION_GUIDE.md
   │                       ↓
   │                       ARCHITECTURE.md (opcional)
   │                       ↓
   │                       [Problemas?] → TROUBLESHOOTING.md
   │
   ├─→ [Quer customizar?] → CUSTOMIZATION_GUIDE.md
   │
   └─→ [Quer usar?] → VARIABLES_GUIDE.md
                      ↓
                      VISUAL_EXAMPLE.md
```

---

## 📋 Guias por Tarefa

### 🔧 Integração

**Objetivo:** Adicionar o sistema ao projeto

**Documentos necessários:**
1. [INTEGRATION_GUIDE.md](./INTEGRATION_GUIDE.md) - Passo a passo completo
2. [ARCHITECTURE.md](./ARCHITECTURE.md) - Entender a estrutura
3. [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - Se encontrar problemas

**Arquivos de código:**
- `Models/Geometry/VariableShape.cs`
- `Views/VariableEditorDialog.cs`
- `ViewModels/SketchViewModelExtensions.cs`
- `Inventory/Items/VariableInventoryItem.cs`
- `Plugins/Data/DataPlugin.cs`

**Tempo estimado:** 1-2 horas

---

### 🎨 Customização

**Objetivo:** Personalizar aparência ou comportamento

**Documentos necessários:**
1. [CUSTOMIZATION_GUIDE.md](./CUSTOMIZATION_GUIDE.md) - Guia completo de customização
2. [ARCHITECTURE.md](./ARCHITECTURE.md) - Para entender onde mexer

**Principais pontos de customização:**
- Cores e ícones → `VariableShape.cs` método `Draw()`
- Tamanho → Constante `RADIUS`
- Validações → Criar novas classes derivadas
- Novos tipos → Herdar de `VariableShape`

**Tempo estimado:** 30 minutos - 2 horas (dependendo da customização)

---

### 🐛 Solução de Problemas

**Objetivo:** Resolver erros e bugs

**Documentos necessários:**
1. [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - Problemas comuns e soluções
2. [INTEGRATION_GUIDE.md](./INTEGRATION_GUIDE.md) - Verificar integração correta

**Checklist básico:**
- [ ] Código compila sem erros?
- [ ] Arquivos estão nos lugares corretos?
- [ ] Plugin está sendo carregado?
- [ ] Testes unitários passam?

**Recursos de debug:**
- Logs: `Debug.WriteLine()`
- Testes: `dotnet test`
- Inspeção: Painel de debug (ver TROUBLESHOOTING.md)

---

### 📖 Aprendizado

**Objetivo:** Aprender a usar o sistema

**Documentos necessários:**
1. [VARIABLES_GUIDE.md](./VARIABLES_GUIDE.md) - Manual completo
2. [VISUAL_EXAMPLE.md](./VISUAL_EXAMPLE.md) - Exemplos práticos

**Tópicos principais:**
- Como adicionar variáveis
- Como conectar variáveis
- Como editar valores
- Como dados fluem
- Casos de uso práticos

**Tempo estimado:** 30-45 minutos

---

## 🔍 Busca Rápida

### Por Tópico

**Compilação:**
- Erros de compilação → [TROUBLESHOOTING.md](#problemas-de-compilação)
- Namespaces faltando → [TROUBLESHOOTING.md](#problemas-de-compilação)
- Packages faltando → [INTEGRATION_GUIDE.md](./INTEGRATION_GUIDE.md#passo-1)

**Interface:**
- Variável não aparece → [TROUBLESHOOTING.md](#variável-não-aparece-no-inventário)
- Duplo clique não funciona → [TROUBLESHOOTING.md](#duplo-clique-não-abre-o-editor)
- Visual errado → [CUSTOMIZATION_GUIDE.md](#personalizar-aparência)

**Propagação:**
- Dados não fluem → [TROUBLESHOOTING.md](#variáveis-não-propagam-pelos-galhos)
- Loop infinito → [TROUBLESHOOTING.md](#loop-infinito-na-propagação)
- Lentidão → [TROUBLESHOOTING.md](#propagação-lenta)

**Customização:**
- Mudar cores → [CUSTOMIZATION_GUIDE.md](#mudar-cores-da-variável)
- Adicionar tipos → [CUSTOMIZATION_GUIDE.md](#adicionar-novos-tipos)
- Criar validações → [CUSTOMIZATION_GUIDE.md](#criar-validações)

---

## 📊 Estatísticas do Projeto

| Métrica | Valor |
|---------|-------|
| **Arquivos de código** | 5 arquivos C# |
| **Linhas de código** | ~800 linhas |
| **Arquivos de documentação** | 8 arquivos MD |
| **Páginas de documentação** | ~50 páginas |
| **Testes unitários** | 21 testes |
| **Cobertura estimada** | 85% |
| **Tempo de integração** | 1-2 horas |
| **Tempo de aprendizado** | 30-45 minutos |

---

## 🎓 Níveis de Conhecimento

### Nível 1: Iniciante
**Você está aqui se:**
- É sua primeira vez vendo o projeto
- Não sabe o que são variáveis de dados
- Quer apenas usar o sistema

**Leia:**
1. [README.md](./README.md) - Seções: "O Que Foi Criado" e "Como Funciona"
2. [VARIABLES_GUIDE.md](./VARIABLES_GUIDE.md) - Seções: "Como Usar"
3. [VISUAL_EXAMPLE.md](./VISUAL_EXAMPLE.md) - Todos os exemplos

---

### Nível 2: Intermediário
**Você está aqui se:**
- Já usou o sistema básico
- Quer integrar ao projeto
- Sabe C# e WPF

**Leia:**
1. [INTEGRATION_GUIDE.md](./INTEGRATION_GUIDE.md) - Completo
2. [ARCHITECTURE.md](./ARCHITECTURE.md) - Diagrama de componentes
3. [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - Problemas de integração

**Faça:**
- Copie os arquivos
- Siga o passo a passo
- Execute os testes
- Teste no seu projeto

---

### Nível 3: Avançado
**Você está aqui se:**
- Domina o sistema
- Quer customizar ou estender
- Vai criar novas funcionalidades

**Leia:**
1. [CUSTOMIZATION_GUIDE.md](./CUSTOMIZATION_GUIDE.md) - Completo
2. [ARCHITECTURE.md](./ARCHITECTURE.md) - Completo
3. Código-fonte - Com atenção aos comentários

**Faça:**
- Crie novos tipos de variáveis
- Implemente validações customizadas
- Adicione novos recursos
- Contribua com melhorias

---

### Nível 4: Expert
**Você está aqui se:**
- Conhece tudo do sistema
- Vai modificar a arquitetura
- Vai criar extensões complexas

**Leia:**
- Todo o código-fonte
- Todos os testes
- Toda a documentação
- Código do projeto original RPA Mechanics

**Faça:**
- Refatore para melhorias
- Otimize performance
- Crie plugins complexos
- Documente suas mudanças

---

## 🚀 Atalhos Úteis

### Comandos Rápidos

```bash
# Compilar
dotnet build

# Executar
dotnet run --project RPA_ULTRA_CORE

# Testes
dotnet test

# Limpar e recompilar
dotnet clean && dotnet build

# Ver estrutura do projeto
tree -L 3

# Buscar em arquivos
grep -r "VariableShape" .

# Contar linhas de código
find . -name "*.cs" -exec wc -l {} + | sort -n
```

---

## 📞 Suporte

### Antes de Pedir Ajuda

1. ✅ Consultou [TROUBLESHOOTING.md](./TROUBLESHOOTING.md)?
2. ✅ Verificou o [Checklist de Verificação](./TROUBLESHOOTING.md#checklist-de-verificação)?
3. ✅ Executou os testes unitários?
4. ✅ Leu a documentação relevante?

### Como Reportar Problemas

Ao criar uma issue, inclua:

```markdown
**Ambiente:**
- OS: Windows/Linux/Mac
- .NET Version: 8.0
- Projeto: RPA Mechanics

**Problema:**
[Descrição clara do problema]

**Passos para Reproduzir:**
1. ...
2. ...
3. ...

**Comportamento Esperado:**
[O que deveria acontecer]

**Comportamento Atual:**
[O que está acontecendo]

**Logs:**
```
[Cole aqui os logs relevantes]
```

**Screenshots:**
[Se aplicável]
```

---

## 🗂️ Glossário

| Termo | Significado |
|-------|-------------|
| **Variable** | Objeto que armazena nome e valor |
| **Node** | Ponto de conexão entre shapes |
| **Shape** | Elemento visual no canvas |
| **Canvas** | Área de desenho principal |
| **Propagation** | Fluxo de dados entre variáveis |
| **MEF** | Managed Extensibility Framework (sistema de plugins) |
| **SkiaSharp** | Biblioteca de renderização 2D |
| **WPF** | Windows Presentation Foundation |
| **MVVM** | Model-View-ViewModel (padrão arquitetural) |
| **Inventory** | Painel com itens disponíveis |
| **Hotbar** | Barra de atalhos (slots 1-7) |

---

## 📅 Histórico de Versões

| Versão | Data | Mudanças |
|--------|------|----------|
| 1.0.0 | Out 2025 | Release inicial |

---

## 📜 Licença

Este sistema segue a mesma licença do projeto RPA Mechanics.

---

## 🙏 Agradecimentos

Desenvolvido para o projeto [RPA Mechanics](https://github.com/SergioSilva77/rpa-mechanics) por SergioSilva77.

---

## 📌 Links Rápidos

### Documentação
- [README.md](./README.md) - Visão geral
- [INTEGRATION_GUIDE.md](./INTEGRATION_GUIDE.md) - Como integrar
- [VARIABLES_GUIDE.md](./VARIABLES_GUIDE.md) - Como usar
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Arquitetura
- [TROUBLESHOOTING.md](./TROUBLESHOOTING.md) - Problemas
- [CUSTOMIZATION_GUIDE.md](./CUSTOMIZATION_GUIDE.md) - Customização
- [VISUAL_EXAMPLE.md](./VISUAL_EXAMPLE.md) - Exemplos

### Código
- [VariableShape.cs](./Models/Geometry/VariableShape.cs) - Classe principal
- [VariableEditorDialog.cs](./Views/VariableEditorDialog.cs) - Editor
- [DataPlugin.cs](./Plugins/Data/DataPlugin.cs) - Plugin
- [VariableSystemTests.cs](./Tests/VariableSystemTests.cs) - Testes

### Projeto Original
- [GitHub - RPA Mechanics](https://github.com/SergioSilva77/rpa-mechanics)

---

**💡 Dica:** Marque esta página nos favoritos para fácil acesso!

---

Última atualização: Outubro 2025
