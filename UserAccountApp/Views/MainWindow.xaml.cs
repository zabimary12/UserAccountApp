using System.Windows;
using UserAccountApp.ViewModels;
using System.Diagnostics;

namespace UserAccountApp.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 