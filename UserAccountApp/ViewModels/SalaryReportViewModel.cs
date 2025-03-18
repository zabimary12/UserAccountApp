using System.Collections.ObjectModel;
using System.Windows.Input;
using UserAccountApp.Commands;
using UserAccountApp.Model;
using UserAccountApp.Interfaces;
using UserAccountApp.Factories;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.Win32;
using System;

namespace UserAccountApp.ViewModels
{
    public class SalaryReportViewModel : BaseViewModel
    {
        private readonly IEmployeeReportRepository _reportRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private ObservableCollection<DepartmentSalaryReport> _reportData;
        private ObservableCollection<Department> _departments;
        private Department _selectedDepartment;
        private bool _isLoading;
        private bool _isInitialized;
        private string _searchText;
        private ObservableCollection<DepartmentSalaryReport> _filteredReportData;
        private string _sortField;
        private bool _sortAscending = true;

        public ObservableCollection<DepartmentSalaryReport> ReportData
        {
            get => _reportData;
            set => SetProperty(ref _reportData, value);
        }

        public ObservableCollection<DepartmentSalaryReport> FilteredReportData
        {
            get => _filteredReportData;
            set => SetProperty(ref _filteredReportData, value);
        }

        public ObservableCollection<Department> Departments
        {
            get => _departments;
            set => SetProperty(ref _departments, value);
        }

        public Department SelectedDepartment
        {
            get => _selectedDepartment;
            set
            {
                if (SetProperty(ref _selectedDepartment, value))
                {
                    ApplyFilters();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilters();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public int TotalEmployees => FilteredReportData?.Sum(r => r.EmployeeCount) ?? 0;
        public decimal TotalAmount => FilteredReportData?.Sum(r => r.TotalSalary) ?? 0;

        public ICommand LoadReportCommand { get; }
        public ICommand ExportToTxtCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand SortCommand { get; }

        public SalaryReportViewModel(RepositoryFactory repositoryFactory)
        {
            _reportRepository = repositoryFactory.CreateEmployeeReportRepository();
            _departmentRepository = repositoryFactory.CreateDepartmentRepository();

            LoadReportCommand = new RelayCommand(async () => await LoadReportAsync());
            ExportToTxtCommand = new RelayCommand(async () => await ExportToTxt());
            ExportToCsvCommand = new RelayCommand(async () => await ExportToCsv());
            SortCommand = new RelayCommand<string>(SortData);

            // Ініціалізуємо колекції
            ReportData = new ObservableCollection<DepartmentSalaryReport>();
            FilteredReportData = new ObservableCollection<DepartmentSalaryReport>();
            Departments = new ObservableCollection<Department>();
        }

        private void SortData(string field)
        {
            if (_sortField == field)
            {
                _sortAscending = !_sortAscending;
            }
            else
            {
                _sortField = field;
                _sortAscending = true;
            }

            ApplyFilters();
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            try
            {
                IsLoading = true;
                await LoadDepartmentsAsync();
                _isInitialized = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            var allDepartments = new ObservableCollection<Department>(departments);
            allDepartments.Insert(0, new Department { Id = 0, Name = "Всі" });
            Departments = allDepartments;
            SelectedDepartment = allDepartments[0];
        }

        private async Task LoadReportAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                var report = await _reportRepository.GetDepartmentSalaryReportAsync(
                    SelectedDepartment?.Id == 0 ? null : SelectedDepartment?.Id);
                ReportData = new ObservableCollection<DepartmentSalaryReport>(report);
                ApplyFilters();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ApplyFilters()
        {
            if (ReportData == null) return;

            var filtered = ReportData.AsQueryable();

            if (SelectedDepartment?.Id > 0)
            {
                filtered = filtered.Where(r => r.DepartmentId == SelectedDepartment.Id);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(r => r.DepartmentName.ToLower().Contains(searchLower));
            }

            // Застосовуємо сортування
            if (!string.IsNullOrEmpty(_sortField))
            {
                filtered = _sortAscending
                    ? filtered.OrderBy(r => GetPropertyValue(r, _sortField))
                    : filtered.OrderByDescending(r => GetPropertyValue(r, _sortField));
            }

            FilteredReportData = new ObservableCollection<DepartmentSalaryReport>(filtered);
            OnPropertyChanged(nameof(TotalEmployees));
            OnPropertyChanged(nameof(TotalAmount));
        }

        private object GetPropertyValue(DepartmentSalaryReport report, string propertyName)
        {
            return propertyName switch
            {
                "DepartmentName" => report.DepartmentName,
                "EmployeeCount" => report.EmployeeCount,
                "TotalSalary" => report.TotalSalary,
                "AverageSalary" => report.AverageSalary,
                _ => null
            };
        }

        private async Task ExportToTxt()
        {
            if (FilteredReportData == null || !FilteredReportData.Any())
            {
                MessageBox.Show("Немає даних для експорту", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Текстові файли (*.txt)|*.txt",
                Title = "Зберегти звіт як TXT",
                DefaultExt = "txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Звіт про зарплати по відділах");
                    sb.AppendLine("=============================");
                    sb.AppendLine($"Дата: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    sb.AppendLine($"Відділ: {(SelectedDepartment != null ? SelectedDepartment.Name : "Всі відділи")}");
                    sb.AppendLine($"Пошук: {(string.IsNullOrEmpty(SearchText) ? "Не вказано" : SearchText)}");
                    sb.AppendLine($"Всього працівників: {TotalEmployees}");
                    sb.AppendLine($"Загальна сума: {TotalAmount:N2}");
                    sb.AppendLine("=============================");
                    sb.AppendLine();

                    // Заголовки таблиці
                    sb.AppendLine("Відділ".PadRight(30) + "Кількість".PadRight(15) + "Загальна сума".PadRight(20) + "Середня зарплата");
                    sb.AppendLine(new string('-', 85));

                    // Дані
                    foreach (var report in FilteredReportData)
                    {
                        sb.AppendLine($"{report.DepartmentName.PadRight(30)}" +
                                    $"{report.EmployeeCount.ToString().PadRight(15)}" +
                                    $"{report.TotalSalary:N2}".PadRight(20) +
                                    $"{report.AverageSalary:N2}");
                    }

                    await File.WriteAllTextAsync(dialog.FileName, sb.ToString());
                    MessageBox.Show("Звіт успішно збережено", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при збереженні файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task ExportToCsv()
        {
            if (FilteredReportData == null || !FilteredReportData.Any())
            {
                MessageBox.Show("Немає даних для експорту", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "CSV файли (*.csv)|*.csv",
                Title = "Зберегти звіт як CSV",
                DefaultExt = "csv"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    var sb = new StringBuilder();
                    // Заголовки
                    sb.AppendLine("Відділ,Кількість працівників,Загальна сума,Середня зарплата");

                    // Дані
                    foreach (var report in FilteredReportData)
                    {
                        sb.AppendLine($"{EscapeCsvValue(report.DepartmentName)}," +
                                    $"{report.EmployeeCount}," +
                                    $"{report.TotalSalary:N2}," +
                                    $"{report.AverageSalary:N2}");
                    }

                    await File.WriteAllTextAsync(dialog.FileName, sb.ToString());
                    MessageBox.Show("Звіт успішно збережено", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при збереженні файлу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string EscapeCsvValue(string value)
        {
            if (value.Contains(",") || value.Contains("\""))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }
            return value;
        }
    }
} 