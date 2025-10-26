using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RPA_ULTRA_CORE.Models.Geometry;

namespace RPA_ULTRA_CORE.Views
{
    /// <summary>
    /// Janela de diálogo para editar nome e valor de variáveis
    /// </summary>
    public partial class VariableEditorDialog : Window
    {
        public string VariableName { get; set; }
        public string VariableValue { get; set; }

        private readonly VariableShape _variableShape;
        private TextBox txtName;
        private TextBox txtValue;

        public VariableEditorDialog(VariableShape variableShape)
        {
            _variableShape = variableShape;
            VariableName = variableShape.VariableName;
            VariableValue = variableShape.VariableValue;

            InitializeComponent();
            DataContext = this;

            // Define o foco no campo de nome
            Loaded += (s, e) => txtName?.Focus();
        }

        private void InitializeComponent()
        {
            // Configuração da janela
            Title = "Edit Variable";
            Width = 400;
            Height = 250;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            ResizeMode = ResizeMode.NoResize;
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));
            WindowStyle = WindowStyle.SingleBorderWindow;

            // Grid principal
            var mainGrid = new Grid
            {
                Margin = new Thickness(20)
            };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Label Nome
            var lblName = new TextBlock
            {
                Text = "Variable Name:",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(lblName, 0);
            mainGrid.Children.Add(lblName);

            // TextBox Nome
            txtName = new TextBox
            {
                Name = "txtName",
                FontSize = 14,
                Padding = new Thickness(8, 6, 8, 6),
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                BorderThickness = new Thickness(1),
                CaretBrush = Brushes.White
            };
            txtName.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("VariableName")
            {
                Mode = System.Windows.Data.BindingMode.TwoWay,
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });
            Grid.SetRow(txtName, 2);
            mainGrid.Children.Add(txtName);

            // Label Valor
            var lblValue = new TextBlock
            {
                Text = "Variable Value:",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetRow(lblValue, 4);
            mainGrid.Children.Add(lblValue);

            // TextBox Valor (multiline)
            txtValue = new TextBox
            {
                Name = "txtValue",
                FontSize = 14,
                Padding = new Thickness(8, 6, 8, 6),
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 45)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(70, 70, 70)),
                BorderThickness = new Thickness(1),
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                CaretBrush = Brushes.White
            };
            txtValue.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding("VariableValue")
            {
                Mode = System.Windows.Data.BindingMode.TwoWay,
                UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged
            });
            Grid.SetRow(txtValue, 5);
            mainGrid.Children.Add(txtValue);

            // StackPanel para botões
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 15, 0, 0)
            };
            Grid.SetRow(buttonPanel, 6);

            // Botão Cancelar
            var btnCancel = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 32,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FontSize = 13
            };
            btnCancel.Click += BtnCancel_Click;
            buttonPanel.Children.Add(btnCancel);

            // Botão OK
            var btnOk = new Button
            {
                Content = "OK",
                Width = 80,
                Height = 32,
                Background = new SolidColorBrush(Color.FromRgb(30, 144, 255)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(50, 164, 255)),
                BorderThickness = new Thickness(1),
                Cursor = Cursors.Hand,
                FontWeight = FontWeights.SemiBold,
                FontSize = 13
            };
            btnOk.Click += BtnOk_Click;
            buttonPanel.Children.Add(btnOk);

            mainGrid.Children.Add(buttonPanel);

            // Define o conteúdo da janela
            Content = mainGrid;

            // Estilo de hover para botões
            btnOk.MouseEnter += (s, e) => btnOk.Background = new SolidColorBrush(Color.FromRgb(50, 164, 255));
            btnOk.MouseLeave += (s, e) => btnOk.Background = new SolidColorBrush(Color.FromRgb(30, 144, 255));
            btnCancel.MouseEnter += (s, e) => btnCancel.Background = new SolidColorBrush(Color.FromRgb(70, 70, 70));
            btnCancel.MouseLeave += (s, e) => btnCancel.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));

            // Atalho Enter para OK
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    BtnOk_Click(this, null);
                }
                else if (e.Key == Key.Escape)
                {
                    BtnCancel_Click(this, null);
                }
            };
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(VariableName))
            {
                MessageBox.Show("Variable name cannot be empty!", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName?.Focus();
                return;
            }

            // Atualiza a variável
            _variableShape.VariableName = VariableName.Trim();
            _variableShape.VariableValue = VariableValue ?? "";

            // Propaga a variável pelos galhos
            _variableShape.PropagateVariable();

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}