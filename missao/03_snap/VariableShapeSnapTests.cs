using Xunit;
using RPA_ULTRA_CORE.Models.Geometry;
using RPA_ULTRA_CORE.Services;
using SkiaSharp;
using System.Linq;

namespace RPA_ULTRA_CORE.Tests
{
    /// <summary>
    /// Testes unitários para o sistema de snap do VariableShape
    /// </summary>
    public class VariableShapeSnapTests
    {
        private const float RADIUS = 25f;
        private const float TOLERANCE = 10f;

        [Fact]
        public void VariableShape_ImplementsIAnchorProvider()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(100, 100));

            // Act & Assert
            Assert.IsAssignableFrom<IAnchorProvider>(variable);
        }

        [Fact]
        public void GetAnchorPoints_ReturnsCenterAnchor()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var mousePos = new SKPoint(100, 100);

            // Act
            var anchors = variable.GetAnchorPoints(mousePos).ToList();

            // Assert
            Assert.NotEmpty(anchors);
            var centerAnchor = anchors.FirstOrDefault(a => a.Type == AnchorType.Center);
            Assert.NotNull(centerAnchor);
            Assert.Equal(center, centerAnchor.Position);
            Assert.Equal("C", centerAnchor.Symbol);
            Assert.NotNull(centerAnchor.Node); // Deve ter CenterNode
        }

        [Fact]
        public void GetAnchorPoints_Returns8PerimeterAnchors()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var mousePos = new SKPoint(150, 150);

            // Act
            var anchors = variable.GetAnchorPoints(mousePos).ToList();

            // Assert
            var perimeterAnchors = anchors.Where(a => a.Type == AnchorType.Perimeter).ToList();
            Assert.True(perimeterAnchors.Count >= 8, "Deve ter pelo menos 8 âncoras de perímetro");
            
            // Verifica se todos têm símbolo correto
            Assert.All(perimeterAnchors, a => Assert.Equal("●", a.Symbol));
        }

        [Fact]
        public void GetAnchorPoints_PerimeterAnchors_AtCorrectDistances()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var mousePos = new SKPoint(150, 150);

            // Act
            var anchors = variable.GetAnchorPoints(mousePos).ToList();
            var perimeterAnchors = anchors.Where(a => a.Type == AnchorType.Perimeter).ToList();

            // Assert
            foreach (var anchor in perimeterAnchors)
            {
                var distance = SKPoint.Distance(anchor.Position, center);
                Assert.True(
                    Math.Abs(distance - RADIUS) < 0.1f, 
                    $"Âncora deve estar a {RADIUS}px do centro, mas está a {distance}px"
                );
            }
        }

        [Theory]
        [InlineData(0, 25, 0)]      // 0° - Direita
        [InlineData(45, 17.68, 17.68)]  // 45° - Diagonal direita-baixo
        [InlineData(90, 0, 25)]     // 90° - Baixo
        [InlineData(180, -25, 0)]   // 180° - Esquerda
        [InlineData(270, 0, -25)]   // 270° - Cima
        public void GetAnchorPoints_PerimeterAt_SpecificAngles(int angleDegrees, float expectedOffsetX, float expectedOffsetY)
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var mousePos = new SKPoint(150, 150);

            // Act
            var anchors = variable.GetAnchorPoints(mousePos).ToList();
            var perimeterAnchors = anchors.Where(a => a.Type == AnchorType.Perimeter).ToList();

            // Assert
            var expectedPoint = new SKPoint(center.X + expectedOffsetX, center.Y + expectedOffsetY);
            var foundAnchor = perimeterAnchors.FirstOrDefault(a => 
                Math.Abs(a.Position.X - expectedPoint.X) < 1f &&
                Math.Abs(a.Position.Y - expectedPoint.Y) < 1f
            );

            Assert.NotNull(foundAnchor);
        }

        [Fact]
        public void GetAnchorPoints_ReturnsDynamicPerimeterAnchor()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var mousePos = new SKPoint(115, 110); // Posição arbitrária

            // Act
            var anchors = variable.GetAnchorPoints(mousePos).ToList();

            // Assert
            var dynamicAnchor = anchors.FirstOrDefault(a => 
                a.Type == AnchorType.Perimeter && 
                a.Priority == 1
            );

            Assert.NotNull(dynamicAnchor);
            
            // Verifica se está no perímetro
            var distance = SKPoint.Distance(dynamicAnchor.Position, center);
            Assert.True(Math.Abs(distance - RADIUS) < 0.1f);
        }

        [Fact]
        public void IsNearAnchor_ReturnsTrueWhenNearCenter()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var nearPoint = new SKPoint(105, 105); // 5px de distância

            // Act
            var isNear = variable.IsNearAnchor(nearPoint, TOLERANCE);

            // Assert
            Assert.True(isNear);
        }

        [Fact]
        public void IsNearAnchor_ReturnsTrueWhenNearPerimeter()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var nearPerimeter = new SKPoint(122, 100); // ~22px do centro (3px da borda)

            // Act
            var isNear = variable.IsNearAnchor(nearPerimeter, TOLERANCE);

            // Assert
            Assert.True(isNear);
        }

        [Fact]
        public void IsNearAnchor_ReturnsFalseWhenFarAway()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var farPoint = new SKPoint(200, 200); // Muito longe

            // Act
            var isNear = variable.IsNearAnchor(farPoint, TOLERANCE);

            // Assert
            Assert.False(isNear);
        }

        [Fact]
        public void GetClosestAnchor_ReturnsCenterWhenVeryClose()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var veryClosePoint = new SKPoint(102, 102); // 2px do centro

            // Act
            var anchor = variable.GetClosestAnchor(veryClosePoint);

            // Assert
            Assert.Equal(AnchorType.Center, anchor.Type);
            Assert.Equal("C", anchor.Symbol);
            Assert.NotNull(anchor.Node);
            Assert.Equal(center, anchor.Position);
        }

        [Fact]
        public void GetClosestAnchor_ReturnsPerimeterWhenNearEdge()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var nearEdge = new SKPoint(120, 100); // No lado direito

            // Act
            var anchor = variable.GetClosestAnchor(nearEdge);

            // Assert
            Assert.Equal(AnchorType.Perimeter, anchor.Type);
            Assert.Equal("●", anchor.Symbol);
            
            // Verifica se está no perímetro direito (~125, 100)
            Assert.True(anchor.Position.X > center.X);
            Assert.True(Math.Abs(anchor.Position.Y - center.Y) < 1f);
        }

        [Theory]
        [InlineData(100, 100, true)]   // Centro exato
        [InlineData(105, 105, true)]   // Dentro da zona centro (30%)
        [InlineData(110, 110, false)]  // Fora da zona centro
        [InlineData(120, 100, false)]  // Perto do perímetro
        public void GetClosestAnchor_ReturnsCorrectTypeBasedOnDistance(float x, float y, bool shouldBeCenter)
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var testPoint = new SKPoint(x, y);

            // Act
            var anchor = variable.GetClosestAnchor(testPoint);

            // Assert
            if (shouldBeCenter)
            {
                Assert.Equal(AnchorType.Center, anchor.Type);
            }
            else
            {
                Assert.Equal(AnchorType.Perimeter, anchor.Type);
            }
        }

        [Fact]
        public void GetClosestAnchor_CalculatesCorrectPerimeterPoint()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var mousePos = new SKPoint(120, 110);

            // Act
            var anchor = variable.GetClosestAnchor(mousePos);

            // Assert
            if (anchor.Type == AnchorType.Perimeter)
            {
                // Calcula onde deveria estar
                var direction = mousePos - center;
                var length = direction.Length;
                var normalized = new SKPoint(direction.X / length, direction.Y / length);
                var expectedPos = new SKPoint(
                    center.X + normalized.X * RADIUS,
                    center.Y + normalized.Y * RADIUS
                );

                // Permite pequena margem de erro
                Assert.True(Math.Abs(anchor.Position.X - expectedPos.X) < 1f);
                Assert.True(Math.Abs(anchor.Position.Y - expectedPos.Y) < 1f);
            }
        }

        [Fact]
        public void Snap_WorksWithMultipleVariables()
        {
            // Arrange
            var var1 = new VariableShape(new SKPoint(100, 100));
            var var2 = new VariableShape(new SKPoint(200, 100));
            var mousePos = new SKPoint(195, 100); // Perto de var2

            // Act
            var anchors1 = var1.GetAnchorPoints(mousePos).ToList();
            var anchors2 = var2.GetAnchorPoints(mousePos).ToList();

            var closest1 = var1.IsNearAnchor(mousePos, TOLERANCE);
            var closest2 = var2.IsNearAnchor(mousePos, TOLERANCE);

            // Assert
            Assert.False(closest1); // Longe de var1
            Assert.True(closest2);  // Perto de var2
        }

        [Fact]
        public void Snap_PrioritizesCenterOverPerimeter()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var mousePos = new SKPoint(106, 106); // Dentro da zona centro

            // Act
            var anchors = variable.GetAnchorPoints(mousePos).ToList();
            var centerAnchor = anchors.FirstOrDefault(a => a.Type == AnchorType.Center);
            var closestAnchor = variable.GetClosestAnchor(mousePos);

            // Assert
            Assert.Equal(AnchorType.Center, closestAnchor.Type);
            Assert.NotNull(centerAnchor);
        }

        [Fact]
        public void AnchorPoint_HasCorrectProperties()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(100, 100));
            var mousePos = new SKPoint(120, 100);

            // Act
            var anchors = variable.GetAnchorPoints(mousePos).ToList();

            // Assert
            Assert.All(anchors, anchor =>
            {
                Assert.NotNull(anchor.Position);
                Assert.NotNull(anchor.Symbol);
                Assert.NotNull(anchor.Shape);
                Assert.Equal(variable, anchor.Shape);
            });
        }

        [Fact]
        public void DynamicAnchor_UpdatesWithMousePosition()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            
            // Act
            var anchors1 = variable.GetAnchorPoints(new SKPoint(120, 100)).ToList();
            var anchors2 = variable.GetAnchorPoints(new SKPoint(100, 120)).ToList();

            var dynamic1 = anchors1.FirstOrDefault(a => a.Priority == 1);
            var dynamic2 = anchors2.FirstOrDefault(a => a.Priority == 1);

            // Assert
            Assert.NotNull(dynamic1);
            Assert.NotNull(dynamic2);
            Assert.NotEqual(dynamic1.Position, dynamic2.Position);
        }

        [Fact]
        public void VariableShape_SnapDoesNotAffectOtherProperties()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(100, 100))
            {
                VariableName = "testVar",
                VariableValue = "testValue"
            };

            // Act
            var anchors = variable.GetAnchorPoints(new SKPoint(120, 100));
            var closest = variable.GetClosestAnchor(new SKPoint(120, 100));

            // Assert
            Assert.Equal("testVar", variable.VariableName);
            Assert.Equal("testValue", variable.VariableValue);
            Assert.Equal(new SKPoint(100, 100), variable.Position);
        }

        [Fact]
        public void Integration_SnapWithLineConnection()
        {
            // Arrange
            var var1 = new VariableShape(new SKPoint(100, 100));
            var var2 = new VariableShape(new SKPoint(200, 100));
            var lineStartPos = new SKPoint(100, 100); // Centro de var1
            var lineEndPos = new SKPoint(197, 100);   // Perto de var2

            // Act
            var startAnchor = var1.GetClosestAnchor(lineStartPos);
            var endAnchor = var2.GetClosestAnchor(lineEndPos);

            // Assert
            Assert.Equal(AnchorType.Center, startAnchor.Type);
            Assert.NotNull(startAnchor.Node);
            Assert.True(var2.IsNearAnchor(lineEndPos, TOLERANCE));
        }

        [Fact]
        public void PerformanceTest_GetAnchorPoints_IsEfficient()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(100, 100));
            var mousePos = new SKPoint(120, 120);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            for (int i = 0; i < 1000; i++)
            {
                var anchors = variable.GetAnchorPoints(mousePos).ToList();
            }
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 100, 
                $"1000 chamadas devem executar em menos de 100ms, mas levou {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public void EdgeCase_MouseAtExactCenter()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);

            // Act
            var anchor = variable.GetClosestAnchor(center);

            // Assert
            Assert.Equal(AnchorType.Center, anchor.Type);
            Assert.Equal(center, anchor.Position);
        }

        [Fact]
        public void EdgeCase_MouseVeryFarAway()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var farAway = new SKPoint(10000, 10000);

            // Act
            var isNear = variable.IsNearAnchor(farAway, TOLERANCE);
            var anchor = variable.GetClosestAnchor(farAway);

            // Assert
            Assert.False(isNear);
            Assert.NotNull(anchor); // Sempre retorna algo (fallback)
        }

        [Fact]
        public void EdgeCase_ZeroTolerance()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);
            var nearPoint = new SKPoint(101, 101);

            // Act
            var isNear = variable.IsNearAnchor(nearPoint, 0f);

            // Assert
            Assert.False(isNear); // Com tolerância zero, nada faz snap
        }
    }
}
