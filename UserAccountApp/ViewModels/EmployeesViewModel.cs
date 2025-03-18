using System.Collections.ObjectModel;
using System.Windows.Input;
using UserAccountApp.Commands;
using UserAccountApp.Model;
using UserAccountApp.Interfaces;
using UserAccountApp.Factories;
using System.Threading.Tasks;
using System.Windows;
using UserAccountApp.Views;

namespace UserAccountApp.ViewModels
{
    public class EmployeesViewModel : BaseViewModel
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPositionRepository _positionRepository;
        private ObservableCollection<Employee> _employees;
        private ObservableCollection<Department> _departments;
        private ObservableCollection<Position> _positions;
        private Department _selectedDepartment;
        private Position _selectedPosition;
        private string _nameFilter;
        private Employee _selectedEmployee;

        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            set => SetProperty(ref _employees, value);
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

        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set => SetProperty(ref _selectedDepartment, value);
        }

        public Position SelectedPosition
        {
            get => _selectedPosition;
            set => SetProperty(ref _selectedPosition, value);
        }

        public string NameFilter
        {
            get => _nameFilter;
            set => SetProperty(ref _nameFilter, value);
        }

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set => SetProperty(ref _selectedEmployee, value);
        }

        public ICommand AddEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }
        public ICommand SearchCommand { get; }

        public EmployeesViewModel(RepositoryFactory repositoryFactory)
        {
            _employeeRepository = repositoryFactory.CreateEmployeeRepository();
            _departmentRepository = repositoryFactory.CreateDepartmentRepository();
            _positionRepository = repositoryFactory.CreatePositionRepository();

            AddEmployeeCommand = new RelayCommand(AddEmployee);
            EditEmployeeCommand = new RelayCommand(EditEmployee, CanEditEmployee);
            DeleteEmployeeCommand = new RelayCommand(DeleteEmployee, CanDeleteEmployee);
            SearchCommand = new RelayCommand(Search);

            LoadInitialDataAsync();
        }

        private async void LoadInitialDataAsync()
        {
            await LoadDepartmentsAsync();
            await LoadPositionsAsync();
            await LoadEmployeesAsync();
        }

        private async Task LoadDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            var allDepartments = new ObservableCollection<Department>(departments);
            allDepartments.Insert(0, new Department { Id = 0, Name = "Всі" });
            Departments = allDepartments;
            SelectedDepartment = allDepartments[0];
        }

        private async Task LoadPositionsAsync()
        {
            var positions = await _positionRepository.GetAllPositionsAsync();
            var allPositions = new ObservableCollection<Position>(positions);
            allPositions.Insert(0, new Position { Id = 0, Name = "Всі" });
            Positions = allPositions;
            SelectedPosition = allPositions[0];
        }

        private async Task LoadEmployeesAsync()
        {
            var employees = await _employeeRepository.GetEmployeesAsync(
                SelectedDepartment?.Id == 0 ? null : SelectedDepartment?.Name,
                SelectedPosition?.Id == 0 ? null : SelectedPosition?.Name,
                NameFilter);
            Employees = new ObservableCollection<Employee>(employees);
        }

        private async void Search()
        {
            await LoadEmployeesAsync();
        }

        private async void AddEmployee()
        {
            var viewModel = new EmployeeDialogViewModel(
                null,
                _departmentRepository,
                _positionRepository);

            var dialog = new EmployeeDialog(viewModel);

            if (dialog.ShowDialog() == true)
            {
                await _employeeRepository.AddAsync(viewModel.Employee);
                await LoadEmployeesAsync();
            }
        }

        private async void EditEmployee()
        {
            if (SelectedEmployee == null) return;

            var viewModel = new EmployeeDialogViewModel(
                SelectedEmployee,
                _departmentRepository,
                _positionRepository);

            var dialog = new EmployeeDialog(viewModel);

            if (dialog.ShowDialog() == true)
            {
                await _employeeRepository.UpdateAsync(viewModel.Employee);
                await LoadEmployeesAsync();
            }
        }

        private bool CanEditEmployee()
        {
            return SelectedEmployee != null;
        }

        private async void DeleteEmployee()
        {
            if (SelectedEmployee == null) return;

            var result = MessageBox.Show(
                "Ви впевнені, що хочете видалити цього працівника?",
                "Підтвердження видалення",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _employeeRepository.DeleteAsync(SelectedEmployee.Id);
                await LoadEmployeesAsync();
            }
        }

        private bool CanDeleteEmployee()
        {
            return SelectedEmployee != null;
        }
    }
} 