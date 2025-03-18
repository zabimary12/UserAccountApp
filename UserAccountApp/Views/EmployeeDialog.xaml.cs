using System;
using System.Windows;
using UserAccountApp.ViewModels;

namespace UserAccountApp.Views
{
    public partial class EmployeeDialog : Window
    {
        private readonly EmployeeDialogViewModel _viewModel;

        public EmployeeDialog(EmployeeDialogViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            _viewModel.RequestClose += ViewModel_RequestClose;
        }

        private void ViewModel_RequestClose(object sender, bool dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel.RequestClose -= ViewModel_RequestClose;
        }
    }
} 