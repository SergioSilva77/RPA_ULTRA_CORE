using System.Windows;

namespace RPA_ULTRA_CORE.Views
{
    public partial class DashboardView : Window
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            DragMove();
        }
    }
}
