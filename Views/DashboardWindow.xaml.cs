using System.Windows;
using MyPOS99.ViewModels;
using MyPOS99.Services;
using MyPOS99.Data;

namespace MyPOS99.Views
{
    public partial class DashboardWindow : Window
    {
        private readonly DashboardViewModel _viewModel;

        public DashboardWindow()
        {
            InitializeComponent();

            // Initialize services
            var dbService = new DatabaseService();
            var authService = new AuthenticationService(dbService);

            // Set up ViewModel with MVVM binding
            _viewModel = new DashboardViewModel(dbService, authService);
            DataContext = _viewModel;
        }
    }
}
