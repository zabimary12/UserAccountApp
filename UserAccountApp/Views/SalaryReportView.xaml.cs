using System.Windows.Controls;
using UserAccountApp.ViewModels;

namespace UserAccountApp.Views
{
    public partial class SalaryReportView : UserControl
    {
        private bool _isInitialized;

        public SalaryReportView()
        {
            InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!_isInitialized && DataContext is SalaryReportViewModel viewModel)
            {
                await viewModel.InitializeAsync();
                _isInitialized = true;
            }
        }
    }
} 