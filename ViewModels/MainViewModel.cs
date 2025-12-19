namespace MyPOS99.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private string _title = "MyPOS99 - Point of Sale System";

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public MainViewModel()
        {
            // Initialize your view model here
        }
    }
}
