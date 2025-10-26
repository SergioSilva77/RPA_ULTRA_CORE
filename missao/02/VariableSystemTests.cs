using Xunit;
using RPA_ULTRA_CORE.Models.Geometry;
using RPA_ULTRA_CORE.ViewModels;
using SkiaSharp;
using System.Linq;

namespace RPA_ULTRA_CORE.Tests
{
    /// <summary>
    /// Testes unitários para o sistema de variáveis
    /// </summary>
    public class VariableSystemTests
    {
        [Fact]
        public void VariableShape_Creation_ShouldSetDefaultValues()
        {
            // Arrange & Act
            var position = new SKPoint(100, 100);
            var variable = new VariableShape(position);

            // Assert
            Assert.Equal("Variable", variable.VariableName);
            Assert.Equal("", variable.VariableValue);
            Assert.Equal(position, variable.Position);
            Assert.NotNull(variable.CenterNode);
            Assert.Empty(variable.IncomingVariables);
        }

        [Fact]
        public void VariableShape_SetProperties_ShouldUpdateCorrectly()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(0, 0));

            // Act
            variable.VariableName = "testVar";
            variable.VariableValue = "testValue";

            // Assert
            Assert.Equal("testVar", variable.VariableName);
            Assert.Equal("testValue", variable.VariableValue);
        }

        [Fact]
        public void VariableShape_HitTest_ShouldDetectClickInside()
        {
            // Arrange
            var center = new SKPoint(100, 100);
            var variable = new VariableShape(center);

            // Act
            var hitInside = variable.HitTest(new SKPoint(105, 105));
            var hitOutside = variable.HitTest(new SKPoint(200, 200));

            // Assert
            Assert.True(hitInside);
            Assert.False(hitOutside);
        }

        [Fact]
        public void VariableShape_Move_ShouldUpdatePosition()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(100, 100));
            var deltaX = 50f;
            var deltaY = 30f;

            // Act
            variable.Move(deltaX, deltaY);

            // Assert
            Assert.Equal(150, variable.Position.X);
            Assert.Equal(130, variable.Position.Y);
            Assert.Equal(150, variable.CenterNode.X);
            Assert.Equal(130, variable.CenterNode.Y);
        }

        [Fact]
        public void VariableShape_GetAllVariables_ShouldIncludeOwnAndIncoming()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(0, 0))
            {
                VariableName = "myVar",
                VariableValue = "myValue"
            };
            variable.IncomingVariables["otherVar"] = "otherValue";

            // Act
            var allVars = variable.GetAllVariables();

            // Assert
            Assert.Equal(2, allVars.Count);
            Assert.Equal("myValue", allVars["myVar"]);
            Assert.Equal("otherValue", allVars["otherVar"]);
        }

        [Fact]
        public void VariableShape_Clone_ShouldCreateCopy()
        {
            // Arrange
            var original = new VariableShape(new SKPoint(100, 100))
            {
                VariableName = "original",
                VariableValue = "value"
            };

            // Act
            var clone = (VariableShape)original.Clone();

            // Assert
            Assert.Equal(original.VariableName, clone.VariableName);
            Assert.Equal(original.VariableValue, clone.VariableValue);
            Assert.Equal(original.Position, clone.Position);
            Assert.NotSame(original, clone);
        }

        [Fact]
        public void VariableShape_PropagateVariable_WithoutConnections_ShouldNotThrow()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(0, 0))
            {
                VariableName = "test",
                VariableValue = "value"
            };

            // Act & Assert
            var exception = Record.Exception(() => variable.PropagateVariable());
            Assert.Null(exception);
        }

        [Fact]
        public void VariableInventoryItem_Properties_ShouldBeCorrect()
        {
            // Arrange
            var item = new RPA_ULTRA_CORE.Inventory.Items.VariableInventoryItem();

            // Assert
            Assert.Equal("item.variable", item.Id);
            Assert.Equal("Variable", item.Name);
            Assert.NotNull(item.Description);
        }

        [Fact]
        public void VariableInventoryItem_CreateShape_ShouldReturnVariableShape()
        {
            // Arrange
            var item = new RPA_ULTRA_CORE.Inventory.Items.VariableInventoryItem();
            var position = new SKPoint(50, 50);

            // Act
            var shape = item.CreateShape(position);

            // Assert
            Assert.IsType<VariableShape>(shape);
            Assert.Equal(position, ((VariableShape)shape).Position);
        }

        [Fact]
        public void DataPlugin_Properties_ShouldBeCorrect()
        {
            // Arrange
            var plugin = new RPA_ULTRA_CORE.Plugins.Data.DataPlugin();

            // Assert
            Assert.Equal("plugin.data", plugin.Id);
            Assert.Equal("Data Management", plugin.Name);
            Assert.Equal("1.0.0", plugin.Version);
        }

        [Fact]
        public void DataPlugin_GetSections_ShouldReturnDataSection()
        {
            // Arrange
            var plugin = new RPA_ULTRA_CORE.Plugins.Data.DataPlugin();

            // Act
            var sections = plugin.GetSections().ToList();

            // Assert
            Assert.Single(sections);
            Assert.Equal("section.data", sections[0].Id);
            Assert.Equal("Data", sections[0].Name);
        }

        [Fact]
        public void DataInventorySection_GetItems_ShouldReturnVariableItem()
        {
            // Arrange
            var section = new RPA_ULTRA_CORE.Plugins.Data.DataInventorySection();

            // Act
            var items = section.GetItems().ToList();

            // Assert
            Assert.Single(items);
            Assert.Equal("item.variable", items[0].Id);
        }

        [Theory]
        [InlineData("", "value", false)] // Nome vazio
        [InlineData("name", "", true)]   // Valor vazio é válido
        [InlineData("name", "value", true)] // Normal
        [InlineData("var123", "123", true)] // Números
        [InlineData("my_var", "test", true)] // Underscore
        public void VariableShape_Validation_Scenarios(string name, string value, bool isValid)
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(0, 0))
            {
                VariableName = name,
                VariableValue = value
            };

            // Act
            var hasName = !string.IsNullOrWhiteSpace(variable.VariableName);

            // Assert
            Assert.Equal(isValid, hasName);
        }

        [Fact]
        public void VariableShape_MultipleIncomingVariables_ShouldMergeCorrectly()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(0, 0))
            {
                VariableName = "result",
                VariableValue = "final"
            };

            // Act
            variable.IncomingVariables["var1"] = "value1";
            variable.IncomingVariables["var2"] = "value2";
            variable.IncomingVariables["var3"] = "value3";

            var allVars = variable.GetAllVariables();

            // Assert
            Assert.Equal(4, allVars.Count); // 3 incoming + 1 own
            Assert.Equal("value1", allVars["var1"]);
            Assert.Equal("value2", allVars["var2"]);
            Assert.Equal("value3", allVars["var3"]);
            Assert.Equal("final", allVars["result"]);
        }

        [Fact]
        public void SketchViewModelExtensions_GetVariablesDebugInfo_WithNoVariables()
        {
            // Arrange
            var viewModel = new SketchViewModel();

            // Act
            var info = viewModel.GetVariablesDebugInfo();

            // Assert
            Assert.Contains("No variables", info);
        }

        [Fact]
        public void VariableShape_ComplexValue_ShouldStoreCorrectly()
        {
            // Arrange
            var variable = new VariableShape(new SKPoint(0, 0));
            var complexValue = @"{
                ""name"": ""João"",
                ""age"": 30,
                ""address"": {
                    ""street"": ""Rua X"",
                    ""number"": 123
                }
            }";

            // Act
            variable.VariableValue = complexValue;

            // Assert
            Assert.Equal(complexValue, variable.VariableValue);
        }

        [Fact]
        public void VariableShape_EmptyNameAndValue_ShouldBeValid()
        {
            // Arrange & Act
            var variable = new VariableShape(new SKPoint(0, 0));

            // Assert (não deve lançar exceção)
            Assert.NotNull(variable);
            Assert.Equal("Variable", variable.VariableName);
            Assert.Equal("", variable.VariableValue);
        }
    }
}
