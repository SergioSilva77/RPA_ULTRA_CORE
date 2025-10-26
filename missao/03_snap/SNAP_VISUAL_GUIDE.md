# 🎨 Visualização do Sistema de Snap - VariableShape

## 📍 Mapa de Pontos de Snap

```
                    VISTA SUPERIOR DO VARIABLE SHAPE
                    
                    
                       ● (0°, perímetro)
                        │
                        │
           ● (315°)     │      ● (45°)
                 ╲      │      ╱
                  ╲     │     ╱
                   ╲    │    ╱
                    ╲   │   ╱
       ● (270°) ─────┬──C──┬───── ● (90°)
                    ╱   │   ╲
                   ╱    │    ╲
                  ╱     │     ╲
                 ╱      │      ╲
           ● (225°)     │      ● (135°)
                        │
                        │
                       ● (180°)


    Legenda:
    C  = Centro (CenterNode) - PRIORIDADE MÁXIMA
    ● = Pontos do Perímetro (8 fixos + 1 dinâmico)
```

---

## 🎯 Zonas de Snap

```
    ┌──────────────────────────────────────┐
    │         ZONA EXTERNA                 │
    │   (Sem snap - distância > RADIUS)   │
    │                                       │
    │    ┌────────────────────────────┐   │
    │    │   ZONA DE PERÍMETRO        │   │
    │    │  (Snap nos pontos ●)       │   │
    │    │                             │   │
    │    │     ┌─────────────┐        │   │
    │    │     │ ZONA CENTRO │        │   │
    │    │     │ (Snap em C) │        │   │
    │    │     │      C      │        │   │
    │    │     │     (V)     │        │   │
    │    │     └─────────────┘        │   │
    │    │     ↑             ↑        │   │
    │    │   30% RADIUS   RADIUS      │   │
    │    └────────────────────────────┘   │
    │                                       │
    └──────────────────────────────────────┘
    

    Cálculos:
    - RADIUS = 25px (constante)
    - Zona Centro: 0 até 7.5px (30% de 25)
    - Zona Perímetro: 7.5px até 25px
    - Tolerância: ±10px da borda
```

---

## 🔄 Fluxo de Detecção de Snap

```
┌─────────────────────────────────────────────────────────────┐
│                    1. MOUSE SE MOVE                         │
│                  mousePosition = (x, y)                     │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         2. SNAP SERVICE VERIFICA TODAS AS SHAPES            │
│              foreach (shape in canvas)                      │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         3. FILTRA SHAPES QUE SÃO IAnchorProvider            │
│         var providers = shapes.OfType<IAnchorProvider>()    │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│    4. VARIABLESHAPE DETECTADA - IMPLEMENTA IAnchorProvider │
│              ✅ VariableShape encontrada!                   │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         5. CHAMA GetAnchorPoints(mousePosition)             │
│              Retorna lista de AnchorPoint                   │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         6. CALCULA DISTÂNCIAS                               │
│         foreach (anchor in anchorPoints)                    │
│             distance = Distance(mouse, anchor.Position)     │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         7. VERIFICA SE ESTÁ DENTRO DA TOLERÂNCIA            │
│         if (distance <= tolerance) → SNAP!                  │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         8. SELECIONA ÂNCORA MAIS PRÓXIMA                    │
│         bestAnchor = anchors.OrderBy(distance).First()      │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         9. DESENHA INDICADOR VISUAL                         │
│         - Círculo ciano ao redor do ponto                   │
│         - Símbolo do tipo (C ou ●)                          │
└──────────────────────┬──────────────────────────────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│         10. LINHA FAZ SNAP NO PONTO                         │
│         linePreview.End = bestAnchor.Position               │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎬 Sequência Animada de Conexão

### Frame 1: Começando a Desenhar
```
    ⚪ var1          ~~ linha preview
   (V)              
                    
                    
                    
                    
                    ⚪ var2
                   (V)
```

### Frame 2: Aproximando do Alvo
```
    ⚪ var1
   (V)
     │
     │
     └──────────── ~~ linha preview
                    
                    
                    ⚪ var2
                   (V)
```

### Frame 3: Entrando na Zona de Snap
```
    ⚪ var1
   (V)
     │
     │
     └────────────────┐
                      │
                      ▼
                    ⚫ ← Indicador ciano pisca
                    ⚪ var2
                   (V)
```

### Frame 4: Snap Ativado (Centro)
```
    ⚪ var1
   (V)
     │
     │
     └────────────────┐
                      │
                      ▼C  ← Símbolo "C" aparece
                    ⚪ var2
                   (V)
```

### Frame 5: Conexão Finalizada
```
    ⚪ var1
   (V)
     │
     │ linha conectada
     │
     ▼
    ⚪ var2  ← Dados fluem: var1 → var2
   (V)
```

---

## 🎨 Indicadores Visuais por Tipo

### Tipo 1: Snap no Centro
```
       ⚫ Círculo ciano (raio 8px)
      ╱C╲ Letra "C" branca
     ╱   ╲
    ╱  V  ╲
   ╱       ╲
  ╱_________╲
  
  Características:
  - Cor: SKColors.Cyan
  - Tamanho: 8px raio
  - Símbolo: "C" (Center)
  - Fonte: 12px bold
```

### Tipo 2: Snap no Perímetro
```
       ⚫●  ← Círculo ciano + símbolo
      ╱   ╲
     ╱  V  ╲
    ╱       ╲
   ╱_________╲
  
  Características:
  - Cor: SKColors.Cyan
  - Tamanho: 6px raio
  - Símbolo: "●" (Perimeter)
  - Fonte: 10px
```

---

## 📐 Cálculos Matemáticos

### 1. Cálculo do Ponto Dinâmico no Perímetro

```csharp
// Entrada: mousePosition, _position (centro), RADIUS
// Saída: Ponto no perímetro mais próximo do mouse

// Passo 1: Calcular vetor direção
var direction = mousePosition - _position;
// direction = (mouseX - centerX, mouseY - centerY)

// Passo 2: Calcular comprimento do vetor
var length = direction.Length;
// length = √((mouseX - centerX)² + (mouseY - centerY)²)

// Passo 3: Normalizar o vetor (tornar unitário)
var normalized = direction / length;
// normalized = (dirX/length, dirY/length)

// Passo 4: Multiplicar pela distância do raio
var perimeterPoint = _position + (normalized * RADIUS);
// perimeterPoint = (centerX + normX * 25, centerY + normY * 25)
```

### 2. Cálculo dos 8 Pontos Fixos

```csharp
// Para cada ângulo: 0°, 45°, 90°, 135°, 180°, 225°, 270°, 315°

var angle = 45; // graus
var radians = angle * Math.PI / 180.0; // converter para radianos

var x = centerX + RADIUS * Math.Cos(radians);
var y = centerY + RADIUS * Math.Sin(radians);

// Exemplo para 45°:
// radians = 45 * π / 180 = 0.785398
// cos(0.785398) = 0.707107
// sin(0.785398) = 0.707107
// x = centerX + 25 * 0.707107 = centerX + 17.68
// y = centerY + 25 * 0.707107 = centerY + 17.68
```

### 3. Verificação de Distância para Snap

```csharp
// Verifica se mouse está próximo o suficiente

var distanceToCenter = Distance(mousePosition, _position);
// distanceToCenter = √((mouseX - centerX)² + (mouseY - centerY)²)

var distanceFromEdge = Math.Abs(distanceToCenter - RADIUS);
// distanceFromEdge = |distanceToCenter - 25|

var isNearPerimeter = distanceFromEdge <= tolerance;
// Se tolerance = 10px:
// Snap ativado se distância da borda ≤ 10px
// Ou seja, snap entre raio 15px e 35px
```

---

## 🔢 Tabela de Prioridades

| Prioridade | Tipo | Condição | Raio Efetivo |
|------------|------|----------|--------------|
| **1** | Centro | `distance ≤ 30% * RADIUS` | 0 - 7.5px |
| **2** | Perímetro Dinâmico | Mouse próximo à borda | 15 - 35px |
| **3** | Perímetro Fixo (8 pontos) | Ângulos específicos | 15 - 35px |
| **4** | Fallback | Se nada funcionar | Retorna centro |

---

## 🎯 Exemplos de Snap por Posição do Mouse

```
CASO 1: Mouse no centro
──────────────────────────
Mouse: (centerX, centerY)
Distance: 0px
Snap em: CENTRO (C)
Node: CenterNode


CASO 2: Mouse a 5px do centro
──────────────────────────────
Mouse: (centerX + 5, centerY)
Distance: 5px
Snap em: CENTRO (C)
Node: CenterNode
(dentro da zona de 30%)


CASO 3: Mouse a 20px do centro
──────────────────────────────
Mouse: (centerX + 20, centerY)
Distance: 20px
Snap em: PERÍMETRO (●)
Node: Será criado
(dentro do raio)


CASO 4: Mouse a 25px do centro
──────────────────────────────
Mouse: (centerX + 25, centerY)
Distance: 25px (exatamente na borda)
Snap em: PERÍMETRO (●)
Node: Será criado
(snap perfeito na borda)


CASO 5: Mouse a 30px do centro
──────────────────────────────
Mouse: (centerX + 30, centerY)
Distance: 30px
Snap em: PERÍMETRO (●)
Node: Será criado
(dentro da tolerância de 10px)


CASO 6: Mouse a 40px do centro
──────────────────────────────
Mouse: (centerX + 40, centerY)
Distance: 40px
Snap em: NENHUM
(fora da tolerância)
```

---

## 🧪 Casos de Teste Visuais

### Teste A: Snap em 0° (Direita)
```
                              Mouse aqui ➜ ●
    ⚪ var1                                  
   (V)                        ⚪ var2        
     │                       (V)            
     │                                      
     └──────────────────────▶C              
                            
Resultado: Snap no perímetro direito (0°)
```

### Teste B: Snap em 90° (Baixo)
```
    ⚪ var1
   (V)
     │
     │
     │
     ▼C
    ⚪ var2
   (V)
     ●  ← Mouse aqui
     
Resultado: Snap no perímetro inferior (90°)
```

### Teste C: Snap em 45° (Diagonal)
```
    ⚪ var1
   (V)
     │
     │    ⚪ var2
     │   (V)●  ← Mouse aqui (diagonal)
     └───▶C
     
Resultado: Snap no perímetro diagonal (45°)
```

### Teste D: Múltiplas Conexões
```
    ⚪ var1
   (V)
     │ ╲
     │  ╲───────▶●
     │          ⚪ var2
     │         (V)
     │          │
     └──────────C
                │
                ▼
              ⚪ var3
             (V)
             
Resultado: var1 conecta em dois pontos de var2
```

---

## 📊 Performance: Comparação de Métodos

### Método 1: Apenas 8 Pontos Fixos
```
✅ Pros:
- Simples de calcular
- Performance constante
- Previsível

❌ Contras:
- Limitado a 8 direções
- Snap "pulado" entre pontos
- Menos intuitivo
```

### Método 2: Apenas Snap Dinâmico
```
✅ Pros:
- Snap suave em qualquer direção
- Intuitivo para o usuário
- Flexível

❌ Contras:
- Mais cálculos por frame
- Pode ser impreciso
```

### Método 3: Híbrido (Implementado) ⭐
```
✅ Pros:
- Melhor dos dois mundos
- Snap preciso nos 8 eixos principais
- Snap suave nas diagonais
- Performance otimizada com cache

❌ Contras:
- Código mais complexo
- Necessita gerenciar prioridades
```

---

## 🎓 Conceitos Avançados

### 1. Sistema de Prioridade Dinâmica

```csharp
public class AnchorPoint
{
    public int Priority { get; set; } = 0;
    
    // Quanto MAIOR o número, MAIOR a prioridade
    // Priority = 0: Padrão
    // Priority = 1: Alta (snap dinâmico)
    // Priority = 2: Máxima (centro quando muito próximo)
}
```

### 2. Snap Magnético

```csharp
// Aumenta a "força" do snap quando próximo
var magnetStrength = 1.0f;
if (distance < RADIUS * 0.5f)
{
    magnetStrength = 2.0f; // Dobra a área de atração
}

var effectiveTolerance = tolerance * magnetStrength;
```

### 3. Snap com Interpolação

```csharp
// Suaviza a transição do snap
var snapFactor = 1.0f - (distance / tolerance);
var interpolatedPoint = SKPoint.Lerp(
    currentPosition, 
    anchorPosition, 
    snapFactor
);
```

---

## 🔮 Melhorias Futuras

### 1. Snap Contextual
```csharp
// Snap diferente dependendo do estado
if (variableShape.IncomingVariables.Count > 0)
{
    // Variável já tem entrada → priorizar saídas no perímetro
    return perimeterAnchors;
}
else
{
    // Variável vazia → priorizar entrada no centro
    return centerAnchor;
}
```

### 2. Snap com Ângulo Preferencial
```csharp
// Favorece snap horizontal/vertical
var angle = Math.Atan2(direction.Y, direction.X);
var isCardinal = angle % (Math.PI / 2) < 0.1; // ±5.7°

if (isCardinal)
{
    snapTolerance *= 1.5f; // Aumenta tolerância
}
```

### 3. Indicador de Direção de Fluxo
```csharp
// Mostra seta indicando direção dos dados
if (anchorPoint.Node?.ConnectedLines.Count > 0)
{
    DrawDataFlowArrow(canvas, anchorPoint);
}
```

---

## ✅ Checklist de Implementação

- [x] Interface `IAnchorProvider` implementada
- [x] Método `GetAnchorPoints()` criado
- [x] Âncora do centro configurada
- [x] 8 âncoras do perímetro configuradas
- [x] Snap dinâmico implementado
- [x] Método `IsNearAnchor()` implementado
- [x] Método `GetClosestAnchor()` implementado
- [x] Sistema de prioridades configurado
- [x] Indicadores visuais compatíveis
- [x] Documentação completa

---

**Arquivo**: SNAP_VISUAL_GUIDE.md  
**Versão**: 1.0  
**Status**: ✅ Completo
