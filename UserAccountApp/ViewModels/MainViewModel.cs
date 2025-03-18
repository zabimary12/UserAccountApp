using System.Windows.Input;
using System.Windows;
using UserAccountApp.Commands;
using UserAccountApp.Interfaces;
using UserAccountApp.Factories;

namespace UserAccountApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentView;
        private readonly CompanyInfoViewModel _companyInfoViewModel;
        private readonly EmployeesViewModel _employeesViewModel;
        private readonly SalaryReportViewModel _salaryReportViewModel;

        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand ShowCompanyInfoCommand { get; }
        public ICommand ShowEmployeesCommand { get; }
        public ICommand ShowSalaryReportCommand { get; }

        public MainViewModel(
            CompanyInfoViewModel companyInfoViewModel,
            EmployeesViewModel employeesViewModel,
            SalaryReportViewModel salaryReportViewModel)
        {
            _companyInfoViewModel = companyInfoViewModel;
            _employeesViewModel = employeesViewModel;
            _salaryReportViewModel = salaryReportViewModel;

            ShowCompanyInfoCommand = new RelayCommand(ShowCompanyInfo);
            ShowEmployeesCommand = new RelayCommand(ShowEmployees);
            ShowSalaryReportCommand = new RelayCommand(ShowSalaryReport);

            // За замовчуванням показуємо інформацію про компанію
            CurrentView = _companyInfoViewModel;
        }

        private void ShowCompanyInfo()
        {
            CurrentView = _companyInfoViewModel;
        }

        private void ShowEmployees()
        {
            CurrentView = _employeesViewModel;
        }

        private void ShowSalaryReport()
        {
            CurrentView = _salaryReportViewModel;
        }
    }
} 