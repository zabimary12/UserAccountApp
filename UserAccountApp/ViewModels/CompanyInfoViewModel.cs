using System;
using System.Windows.Input;
using UserAccountApp.Commands;
using UserAccountApp.Model;
using UserAccountApp.Interfaces;
using System.Threading.Tasks;

namespace UserAccountApp.ViewModels
{
    public class CompanyInfoViewModel : BaseViewModel
    {
        private readonly IDatabase _database;
        private Company _company;
        private Company _originalCompany;
        private bool _isEditMode;
        private int _rightClickCount;
        private DateTime _lastRightClickTime;

        public Company Company
        {
            get => _company;
            set => SetProperty(ref _company, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            private set
            {
                if (SetProperty(ref _isEditMode, value))
                {
                    OnPropertyChanged(nameof(IsReadOnly));
                    OnPropertyChanged(nameof(ShowButtons));
                }
            }
        }

        public bool IsReadOnly => !IsEditMode;
        public bool ShowButtons => IsEditMode;

        public ICommand SaveChangesCommand { get; }
        public ICommand HandleRightClickCommand { get; }

        public CompanyInfoViewModel(IDatabase database)
        {
            _database = database;
            SaveChangesCommand = new RelayCommand(SaveChanges, CanSaveChanges);
            HandleRightClickCommand = new RelayCommand(HandleRightClick);
            _rightClickCount = 0;
            _lastRightClickTime = DateTime.MinValue;
            LoadCompanyData();
        }

        private void HandleRightClick()
        {
            var now = DateTime.Now;
            if ((now - _lastRightClickTime).TotalSeconds > 1)
            {
                _rightClickCount = 1;
            }
            else
            {
                _rightClickCount++;
                if (_rightClickCount == 2)
                {
                    IsEditMode = true;
                    _rightClickCount = 0;
                }
            }
            _lastRightClickTime = now;
        }

        private async void LoadCompanyData()
        {
            using (var connection = _database.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT TOP 1 Id, Name, Addres, PhoneNumber FROM Companies";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Company = new Company
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1),
                                Addres = !reader.IsDBNull(2) ? reader.GetString(2) : null,
                                PhoneNumber = !reader.IsDBNull(3) ? reader.GetString(3) : null
                            };
                            _originalCompany = new Company
                            {
                                Id = Company.Id,
                                Name = Company.Name,
                                Addres = Company.Addres,
                                PhoneNumber = Company.PhoneNumber
                            };
                        }
                    }
                }
            }
        }

        private bool CanSaveChanges()
        {
            return IsEditMode && Company != null && !string.IsNullOrWhiteSpace(Company.Name);
        }

        private async void SaveChanges()
        {
            using (var connection = _database.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        UPDATE Companies 
                        SET Name = @Name, 
                            Addres = @Address, 
                            PhoneNumber = @PhoneNumber 
                        WHERE Id = @Id";

                    command.Parameters.AddWithValue("@Id", Company.Id);
                    command.Parameters.AddWithValue("@Name", Company.Name);
                    command.Parameters.AddWithValue("@Address", (object)Company.Addres ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PhoneNumber", (object)Company.PhoneNumber ?? DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }

            _originalCompany = new Company
            {
                Id = Company.Id,
                Name = Company.Name,
                Addres = Company.Addres,
                PhoneNumber = Company.PhoneNumber
            };

            IsEditMode = false;
        }
    }
} 