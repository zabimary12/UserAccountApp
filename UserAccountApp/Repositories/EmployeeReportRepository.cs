using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UserAccountApp.Data;
using UserAccountApp.Interfaces;
using UserAccountApp.Model;
using ClosedXML.Excel;

namespace UserAccountApp.Repositories
{
    public class EmployeeReportRepository : IEmployeeReportRepository
    {
        private readonly IDatabase _database;

        public EmployeeReportRepository(IDatabase database)
        {
            _database = database;
        }

        public async Task<IEnumerable<DepartmentSalaryReport>> GetDepartmentSalaryReportAsync(int? departmentId = null)
        {
            var reports = new List<DepartmentSalaryReport>();
            var query = @"
                SELECT 
                    d.Id as DepartmentId,
                    d.Name as DepartmentName,
                    COUNT(e.Id) as EmployeeCount,
                    SUM(e.Salary) as TotalSalary
                FROM Departments d
                LEFT JOIN Employees e ON d.Id = e.DepartmentId
                WHERE (@DepartmentId IS NULL OR d.Id = @DepartmentId)
                GROUP BY d.Id, d.Name
                ORDER BY d.Name";

            using (var connection = _database.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DepartmentId", (object)departmentId ?? DBNull.Value);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reports.Add(new DepartmentSalaryReport
                            {
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                                EmployeeCount = reader.GetInt32(reader.GetOrdinal("EmployeeCount")),
                                TotalSalary = reader.GetDecimal(reader.GetOrdinal("TotalSalary"))
                            });
                        }
                    }
                }
            }

            return reports;
        }

        public async Task ExportToExcelAsync(IEnumerable<DepartmentSalaryReport> reportData, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Зарплатна звітність");

                // Заголовки
                worksheet.Cell(1, 1).Value = "Відділ";
                worksheet.Cell(1, 2).Value = "Кількість працівників";
                worksheet.Cell(1, 3).Value = "Загальна сума";
                worksheet.Cell(1, 4).Value = "Середня зарплата";

                // Стилізація заголовків
                var headerRange = worksheet.Range(1, 1, 1, 4);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Дані
                int row = 2;
                foreach (var report in reportData)
                {
                    worksheet.Cell(row, 1).Value = report.DepartmentName;
                    worksheet.Cell(row, 2).Value = report.EmployeeCount;
                    worksheet.Cell(row, 3).Value = report.TotalSalary;
                    worksheet.Cell(row, 4).Value = report.AverageSalary;
                    row++;
                }

                // Форматування
                worksheet.Columns().AdjustToContents();
                worksheet.Range(2, 3, row - 1, 4).Style.NumberFormat.Format = "#,##0.00";

                // Збереження
                await Task.Run(() => workbook.SaveAs(filePath));
            }
        }
    }
} 