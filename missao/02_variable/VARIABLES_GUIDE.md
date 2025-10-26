# Sistema de Variáveis - RPA Mechanics

## Visão Geral

O sistema de variáveis permite criar, armazenar e propagar dados através das conexões (linhas) no canvas. As variáveis fluem pelos galhos conectados, permitindo criar fluxos de dados complexos.

## Como Usar

### 1. Adicionar Variável ao Canvas

1. Pressione **E** para abrir o inventário
2. Navegue até a seção **Data** (ícone de banco de dados)
3. Localize o item **Variable** (ícone com "V")
4. Arraste o item para o canvas na posição desejada
5. Um círculo azul com o ícone "V" será criado

### 2. Editar Variável

Para definir o nome e valor da variável:

1. **Dê um duplo clique** no círculo da variável
2. Uma janela de diálogo será aberta
3. Digite o **nome da variável** (ex: "userName", "idade", "totalVendas")
4. Digite o **valor da variável** (pode ser texto, número, JSON, etc.)
5. Clique em **OK** ou pressione **Ctrl+Enter**

**Atalhos na janela de edição:**
- **Ctrl+Enter**: Salvar e fechar
- **Esc**: Cancelar

### 3. Conectar Variáveis

As variáveis se conectam através de linhas:

1. Segure **SHIFT** para entrar no modo de desenho
2. Clique no nó central (ponto) de uma variável
3. Arraste até outra variável ou forma
4. Solte para criar a conexão

### 4. Propagação de Dados

Quando variáveis estão conectadas:

- Os dados fluem automaticamente pela linha
- A variável de destino recebe todas as variáveis das conexões anteriores
- É possível ter múltiplas variáveis conectadas em cadeia
- Cada nó acumula todas as variáveis recebidas

**Exemplo de fluxo:**

```
Variable A (name: "user", value: "João")
    |
    v (linha de conexão)
Variable B (name: "age", value: "25")
    |
    v
Variable C (recebe: user="João", age="25")
```

## Funcionalidades

### Múltiplas Entradas

Uma variável pode receber dados de múltiplas fontes:

```
Variable A --> 
              Variable C
Variable B -->
```

Variable C terá acesso aos dados de A e B.

### Valores Complexos

O campo de valor suporta:
- Texto simples: `"João Silva"`
- Números: `42`, `3.14`
- JSON: `{"name": "João", "age": 30}`
- Multilinhas (use Enter na caixa de texto)

### Visualização

- **Círculo azul**: Indica uma variável
- **Nome embaixo**: Mostra o nome da variável (quando definido)
- **Borda laranja**: Variável selecionada
- **Linhas conectadas**: Mostram o fluxo de dados

## Teclas de Atalho

- **E**: Abrir/fechar inventário
- **SHIFT + Click**: Iniciar linha
- **DELETE**: Remover variável selecionada
- **Duplo Click**: Editar variável
- **1-7**: Acesso rápido à hotbar

## Exemplos de Uso

### Exemplo 1: Dados de Usuário

```
Variável "nome" = "Maria"
    |
Variável "sobrenome" = "Silva"
    |
Variável "nomeCompleto" = concatena as anteriores
```

### Exemplo 2: Cálculos

```
Variável "preço" = "100"
    |
Variável "quantidade" = "3"
    |
Variável "total" = executa cálculo (em desenvolvimento)
```

### Exemplo 3: Configurações

```
Variável "apiUrl" = "https://api.exemplo.com"
    |
Variável "token" = "abc123xyz"
    |
Requisição HTTP (usa as variáveis)
```

## Boas Práticas

1. **Nomes descritivos**: Use nomes claros como "emailUsuario" em vez de "e"
2. **Organização visual**: Mantenha variáveis relacionadas próximas
3. **Fluxo lógico**: Conecte da esquerda para direita ou de cima para baixo
4. **Documentação**: Use nomes que explicam o propósito da variável

## Debugging

Para visualizar as variáveis e seus valores:
- As variáveis mostram seu nome abaixo do círculo
- Variáveis conectadas propagam automaticamente ao serem editadas
- (Em breve: painel de debug para ver todos os valores)

## Limitações Atuais

- Não há validação de tipo (texto vs número)
- Não há expressões matemáticas automáticas
- A propagação é imediata, sem ordem específica
- Não há variáveis globais (escopo de canvas)

## Próximas Funcionalidades

- [ ] Painel de debug com lista de variáveis
- [ ] Expressões e fórmulas
- [ ] Validação de tipos
- [ ] Variáveis globais
- [ ] Exportar/importar valores
- [ ] Templates de variáveis
- [ ] Histórico de valores

## Contribuindo

Para adicionar novos recursos ao sistema de variáveis:

1. Edite `Models/Geometry/VariableShape.cs` para lógica
2. Modifique `Views/VariableEditorDialog.cs` para UI
3. Atualize `Plugins/Data/DataPlugin.cs` para novos itens
4. Documente as mudanças neste arquivo

## Suporte

Em caso de problemas:
1. Verifique se o plugin Data está carregado
2. Teste a conexão entre variáveis com SHIFT + Click
3. Confirme que o duplo clique está funcionando
4. Abra uma issue no GitHub com detalhes
