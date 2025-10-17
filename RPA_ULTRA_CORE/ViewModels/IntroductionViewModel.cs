using System.Windows;
using System.Windows.Input;
using RPA_ULTRA_CORE.Helpers;
using RPA_ULTRA_CORE.Views;

namespace RPA_ULTRA_CORE.ViewModels
{
    public class IntroductionViewModel : ViewModelBase
    {
        public ICommand NewProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand OpenDashboardCommand { get; }
        public ICommand OpenSketchEditorCommand { get; }

        public IntroductionViewModel()
        {
            NewProjectCommand = new RelayCommand(ExecuteNewProject);
            OpenProjectCommand = new RelayCommand(ExecuteOpenProject);
            SettingsCommand = new RelayCommand(ExecuteSettings);
            OpenDashboardCommand = new RelayCommand(ExecuteOpenDashboard);
            OpenSketchEditorCommand = new RelayCommand(ExecuteOpenSketchEditor);
        }

        private void ExecuteNewProject(object? parameter)
        {
            MessageBox.Show(
                "Funcionalidade 'Novo Projeto' será implementada em breve!",
                "RPA Ultra Core",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void ExecuteOpenProject(object? parameter)
        {
            MessageBox.Show(
                "Funcionalidade 'Abrir Projeto' será implementada em breve!",
                "RPA Ultra Core",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void ExecuteSettings(object? parameter)
        {
            MessageBox.Show(
                "Funcionalidade 'Configurações' será implementada em breve!",
                "RPA Ultra Core",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void ExecuteOpenDashboard(object? parameter)
        {
            var dashboard = new DashboardView();
            dashboard.Show();
        }

        private void ExecuteOpenSketchEditor(object? parameter)
        {
            var sketchEditor = new SketchView();
            sketchEditor.Show();
        }
    }
}
