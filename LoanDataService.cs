using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoanSimulator
{
    public class LoanDataService
    {
        private readonly string _dataFilePath;

        public LoanDataService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appFolder = Path.Combine(appDataPath, "LoanSimulator");
            Directory.CreateDirectory(appFolder);
            _dataFilePath = Path.Combine(appFolder, "loans.json");
        }

        public async Task SaveLoansAsync(IEnumerable<Loan> loans)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(loans, options);
                await File.WriteAllTextAsync(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save loans: {ex.Message}", ex);
            }
        }

        public async Task SaveLoansAsync(IEnumerable<Loan> loans, string filePath)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(loans, options);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save loans to {filePath}: {ex.Message}", ex);
            }
        }

        public async Task<List<Loan>> LoadLoansAsync()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    return new List<Loan>();
                }

                var json = await File.ReadAllTextAsync(_dataFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<Loan>();
                }

                var loans = JsonSerializer.Deserialize<List<Loan>>(json);
                return loans ?? new List<Loan>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load loans: {ex.Message}", ex);
            }
        }

        public async Task<List<Loan>> LoadLoansAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return new List<Loan>();
                }

                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<Loan>();
                }

                var loans = JsonSerializer.Deserialize<List<Loan>>(json);
                return loans ?? new List<Loan>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load loans from {filePath}: {ex.Message}", ex);
            }
        }

        public bool DataFileExists()
        {
            return File.Exists(_dataFilePath);
        }

        public string GetDataFilePath()
        {
            return _dataFilePath;
        }
    }
}