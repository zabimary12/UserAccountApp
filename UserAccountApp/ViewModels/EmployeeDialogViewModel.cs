using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UserAccountApp.Commands;
using UserAccountApp.Model;
using UserAccountApp.Interfaces;
using System.Threading.Tasks;

namespace UserAccountApp.ViewModels
{
    public class EmployeeDialogViewModel : BaseViewModel
    {
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPositionRepository _positionRepository;
        private Employee _employee;
        private ObservableCollection<Department> _departments;
        private ObservableCollection<Position> _positions;
        private bool _isNewEmployee;

        public Employee Employee
        {
            get => _employee;
            set => SetProperty(ref _employee, value);
        }

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public ObservableCollection<Position> Positions
        {
            get => _positions;
            set => SetProperty(ref _positions, value);
        }

        public bool IsNewEmployee
        {
            get => _isNewEmployee;
            set => SetProperty(ref _isNewEmployee, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<bool> RequestClose;

        public EmployeeDialogViewModel(Employee employee, IDepartmentRepository departmentRepository, IPositionRepository positionRepository)
        {
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;

            IsNewEmployee = employee == null;
            Employee = employee ?? new Employee
            {
                HireDate = DateTime.Today
            };

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            LoadDataAsync();
        }

        private async void LoadDataAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            var positions = await _positionRepository.GetAllPositionsAsync();

            Departments = new ObservableCollection<Department>(departments);
            Positions = new ObservableCollection<Position>(positions);
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Employee.FirstName) &&
                   !string.IsNullOrWhiteSpace(Employee.LastName) &&
                   Employee.Department != null &&
                   Employee.Position != null &&
                   Employee.Salary > 0;
        }

        private void Save()
        {
            RequestClose?.Invoke(this, true);
        }

        private void Cancel()
        {
            RequestClose?.Invoke(this, false);
        }
    }
} 