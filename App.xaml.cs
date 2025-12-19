using System.Configuration;
using System.Data;
using System.Windows;
using MyPOS99.Models;

namespace MyPOS99
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public User? CurrentUser { get; set; }
    }
}
