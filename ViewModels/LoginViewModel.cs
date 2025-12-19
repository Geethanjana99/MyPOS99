using System.Windows;
using System.Windows.Input;
using MyPOS99.Services;

namespace MyPOS99.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthenticationService _authService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public LoginViewModel(AuthenticationService authService)
        {
            _authService = authService;
            LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
            ExitCommand = new RelayCommand(Exit);
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand ExitCommand { get; }

        public event EventHandler<bool>? LoginCompleted;

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) && 
                   !string.IsNullOrWhiteSpace(Password) && 
                   !IsLoading;
        }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var (success, message, user) = await _authService.LoginAsync(Username, Password);

                if (success && user != null)
                {
                    LoginCompleted?.Invoke(this, true);
                }
                else
                {
                    ErrorMessage = message;
                    Password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }

        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}
