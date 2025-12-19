using System.Windows;
using System.Windows.Controls;
using MyPOS99.ViewModels;
using MyPOS99.Services;
using MyPOS99.Data;

namespace MyPOS99.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;
        private readonly AuthenticationService _authService;

        public LoginWindow()
        {
            InitializeComponent();

            // Initialize services
            var dbService = new DatabaseService();
            _authService = new AuthenticationService(dbService);

            // Set up ViewModel
            _viewModel = new LoginViewModel(_authService);
            _viewModel.LoginCompleted += OnLoginCompleted;
            DataContext = _viewModel;

            // Focus username on load
            Loaded += (s, e) => UsernameTextBox.Focus();

            // Handle Enter key
            UsernameTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                    PasswordBox.Focus();
            };

            PasswordBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter && _viewModel.LoginCommand.CanExecute(null))
                    _viewModel.LoginCommand.Execute(null);
            };
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = PasswordBox.Password;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnLoginCompleted(object? sender, bool success)
        {
            if (success && _authService.CurrentUser != null)
            {
                // Store current user in App
                ((App)Application.Current).CurrentUser = _authService.CurrentUser;

                // Open Dashboard
                var dashboard = new DashboardWindow();
                dashboard.Show();
                this.Close();
            }
        }
    }
}
