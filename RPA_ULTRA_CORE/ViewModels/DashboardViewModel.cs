using System.Windows.Input;
using System.Windows.Threading;
using RPA_ULTRA_CORE.Helpers;

namespace RPA_ULTRA_CORE.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly Random _random = new Random();
        private readonly DispatcherTimer _updateTimer;

        private double _cpuUsage;
        private double _memoryUsage;
        private double _diskUsage;
        private int _completedTasks;
        private int _pendingTasks;
        private int _averageSpeed;
        private int _errors;
        private double[] _weeklyData;
        private string[] _weeklyLabels;

        public double CpuUsage
        {
            get => _cpuUsage;
            set => SetProperty(ref _cpuUsage, value);
        }

        public double MemoryUsage
        {
            get => _memoryUsage;
            set => SetProperty(ref _memoryUsage, value);
        }

        public double DiskUsage
        {
            get => _diskUsage;
            set => SetProperty(ref _diskUsage, value);
        }

        public int CompletedTasks
        {
            get => _completedTasks;
            set => SetProperty(ref _completedTasks, value);
        }

        public int PendingTasks
        {
            get => _pendingTasks;
            set => SetProperty(ref _pendingTasks, value);
        }

        public int AverageSpeed
        {
            get => _averageSpeed;
            set => SetProperty(ref _averageSpeed, value);
        }

        public int Errors
        {
            get => _errors;
            set => SetProperty(ref _errors, value);
        }

        public double[] WeeklyData
        {
            get => _weeklyData;
            set => SetProperty(ref _weeklyData, value);
        }

        public string[] WeeklyLabels
        {
            get => _weeklyLabels;
            set => SetProperty(ref _weeklyLabels, value);
        }

        public ICommand SimulateDataCommand { get; }

        public DashboardViewModel()
        {
            SimulateDataCommand = new RelayCommand(ExecuteSimulateData);

            // Initialize data
            _weeklyLabels = new[] { "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb", "Dom" };
            _weeklyData = new double[] { 45, 67, 89, 78, 92, 56, 73 };

            // Set initial values
            CpuUsage = 0.65;
            MemoryUsage = 0.78;
            DiskUsage = 0.43;
            CompletedTasks = 1247;
            PendingTasks = 89;
            AverageSpeed = 342;
            Errors = 12;

            // Auto-update timer
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Simulate realistic data changes
            CpuUsage = Math.Clamp(_cpuUsage + (_random.NextDouble() - 0.5) * 0.1, 0.0, 1.0);
            MemoryUsage = Math.Clamp(_memoryUsage + (_random.NextDouble() - 0.5) * 0.05, 0.0, 1.0);
            DiskUsage = Math.Clamp(_diskUsage + (_random.NextDouble() - 0.5) * 0.02, 0.0, 1.0);

            // Occasionally increase completed tasks
            if (_random.NextDouble() > 0.7)
            {
                CompletedTasks += _random.Next(1, 5);
                PendingTasks = Math.Max(0, PendingTasks - _random.Next(0, 3));
            }

            // Random speed fluctuation
            AverageSpeed = Math.Max(100, Math.Min(1000, _averageSpeed + _random.Next(-50, 51)));

            // Errors stay mostly stable
            if (_random.NextDouble() > 0.95)
            {
                Errors += _random.Next(-2, 3);
                Errors = Math.Max(0, Errors);
            }
        }

        private void ExecuteSimulateData(object? parameter)
        {
            // Generate new random data
            var newWeeklyData = new double[7];
            for (int i = 0; i < 7; i++)
            {
                newWeeklyData[i] = _random.Next(30, 100);
            }
            WeeklyData = newWeeklyData;

            // Random changes to metrics
            CpuUsage = _random.NextDouble();
            MemoryUsage = _random.NextDouble();
            DiskUsage = _random.NextDouble();

            CompletedTasks = _random.Next(500, 2000);
            PendingTasks = _random.Next(10, 200);
            AverageSpeed = _random.Next(150, 800);
            Errors = _random.Next(0, 50);
        }
    }
}
