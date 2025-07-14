using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;

namespace LoanSimulator
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Loan _currentLoan = new Loan();
        private readonly LoanDataService _dataService;

        public ObservableCollection<Loan> Loans { get; } = new ObservableCollection<Loan>();

        public Loan CurrentLoan
        {
            get => _currentLoan;
            set
            {
                _currentLoan = value;
                OnPropertyChanged(nameof(CurrentLoan));
            }
        }

        public decimal TotalBalance
        {
            get
            {
                return Loans.Sum(loan => loan.CurrentBalance);
            }
        }

        public decimal TotalMonthlyPayment
        {
            get
            {
                return Loans.Sum(loan => loan.MonthlyPayment);
            }
        }

        public decimal TotalMonthlyInterest
        {
            get
            {
                return Loans.Sum(loan => loan.MonthlyInterest);
            }
        }

        public ICommand AddLoanCommand { get; }
        public ICommand SaveLoansCommand { get; }
        public ICommand SaveAsLoansCommand { get; }
        public ICommand LoadLoansCommand { get; }
        public ICommand ClearAllLoansCommand { get; }

        public MainViewModel()
        {
            _dataService = new LoanDataService();
            AddLoanCommand = new RelayCommand(AddLoan, CanAddLoan);
            SaveLoansCommand = new RelayCommand(async () => await SaveLoansAsync());
            SaveAsLoansCommand = new RelayCommand(async () => await SaveAsLoansAsync());
            LoadLoansCommand = new RelayCommand(async () => await LoadLoansAsync());
            ClearAllLoansCommand = new RelayCommand(ClearAllLoans, CanClearLoans);
            
            // Subscribe to collection changes to update totals
            Loans.CollectionChanged += (s, e) => 
            {
                OnPropertyChanged(nameof(TotalBalance));
                OnPropertyChanged(nameof(TotalMonthlyPayment));
                OnPropertyChanged(nameof(TotalMonthlyInterest));
                
                // Subscribe to property changes of new items
                if (e.NewItems != null)
                {
                    foreach (Loan loan in e.NewItems)
                    {
                        loan.PropertyChanged += Loan_PropertyChanged;
                    }
                }
                
                // Unsubscribe from property changes of removed items
                if (e.OldItems != null)
                {
                    foreach (Loan loan in e.OldItems)
                    {
                        loan.PropertyChanged -= Loan_PropertyChanged;
                    }
                }
            };
            
            // Load loans on startup (silent)
            _ = LoadLoansSilentAsync();
        }

        private async void AddLoan()
        {
            var newLoan = new Loan
            {
                Name = CurrentLoan.Name,
                CurrentBalance = CurrentLoan.CurrentBalance,
                InterestRate = CurrentLoan.InterestRate,
                MonthlyPayment = CurrentLoan.MonthlyPayment,
                PaymentDay = CurrentLoan.PaymentDay
            };

            Loans.Add(newLoan);

            // Reset current loan for next entry
            CurrentLoan = new Loan();
            
            // Auto-save after adding a loan (silent save)
            await SaveLoansSilentAsync();
        }

        private bool CanAddLoan()
        {
            return !string.IsNullOrWhiteSpace(CurrentLoan.Name) &&
                   CurrentLoan.CurrentBalance > 0 &&
                   CurrentLoan.InterestRate >= 0 &&
                   CurrentLoan.MonthlyPayment > 0 &&
                   CurrentLoan.PaymentDay >= 1 &&
                   CurrentLoan.PaymentDay <= 28;
        }

        private async Task SaveLoansAsync()
        {
            if (Loans.Count == 0)
            {
                System.Windows.MessageBox.Show("No loans to save. Please add some loans first.", 
                    "No Data", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            try
            {
                await _dataService.SaveLoansAsync(Loans);
                System.Windows.MessageBox.Show($"Successfully saved {Loans.Count} loan(s).", 
                    "Save Complete", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving loans: {ex.Message}", 
                    "Save Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task SaveAsLoansAsync()
        {
            if (Loans.Count == 0)
            {
                System.Windows.MessageBox.Show("No loans to save. Please add some loans first.", 
                    "No Data", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            // Show save file dialog
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save Loans to File",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json",
                FileName = "my_loans.json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                // User cancelled the dialog
                return;
            }

            try
            {
                await _dataService.SaveLoansAsync(Loans, saveFileDialog.FileName);
                System.Windows.MessageBox.Show($"Successfully saved {Loans.Count} loan(s) to:\n{saveFileDialog.FileName}", 
                    "Save Complete", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving loans to file:\n{saveFileDialog.FileName}\n\nError: {ex.Message}", 
                    "Save Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task LoadLoansAsync()
        {
            // Show file dialog to select file to load
            var openFileDialog = new OpenFileDialog
            {
                Title = "Load Loans from File",
                Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                DefaultExt = ".json",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            };

            // Set default file name to the standard data file if it exists
            var defaultDataFile = _dataService.GetDataFilePath();
            if (File.Exists(defaultDataFile))
            {
                openFileDialog.InitialDirectory = Path.GetDirectoryName(defaultDataFile);
                openFileDialog.FileName = Path.GetFileName(defaultDataFile);
            }

            if (openFileDialog.ShowDialog() != true)
            {
                // User cancelled the dialog
                return;
            }

            try
            {
                var loadedLoans = await _dataService.LoadLoansAsync(openFileDialog.FileName);
                Loans.Clear();
                foreach (var loan in loadedLoans)
                {
                    Loans.Add(loan);
                }

                if (loadedLoans.Count > 0)
                {
                    System.Windows.MessageBox.Show($"Successfully loaded {loadedLoans.Count} loan(s) from:\n{openFileDialog.FileName}", 
                        "Load Complete", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show($"No loans found in the selected file:\n{openFileDialog.FileName}", 
                        "No Data", 
                        System.Windows.MessageBoxButton.OK, 
                        System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading loans from file:\n{openFileDialog.FileName}\n\nError: {ex.Message}", 
                    "Load Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        private async Task LoadLoansSilentAsync()
        {
            try
            {
                var loadedLoans = await _dataService.LoadLoansAsync();
                Loans.Clear();
                foreach (var loan in loadedLoans)
                {
                    Loans.Add(loan);
                }
            }
            catch (Exception ex)
            {
                // Silent load for startup - only log errors
                System.Diagnostics.Debug.WriteLine($"Error loading loans on startup: {ex.Message}");
            }
        }

        private async Task SaveLoansSilentAsync()
        {
            try
            {
                await _dataService.SaveLoansAsync(Loans);
            }
            catch (Exception ex)
            {
                // Silent save for auto-save functionality - only log errors
                System.Diagnostics.Debug.WriteLine($"Error auto-saving loans: {ex.Message}");
            }
        }

        private void ClearAllLoans()
        {
            if (Loans.Count == 0)
            {
                System.Windows.MessageBox.Show("No loans to clear.", 
                    "No Data", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
                return;
            }

            var result = System.Windows.MessageBox.Show($"Are you sure you want to clear all {Loans.Count} loan(s)?\n\nThis action cannot be undone.", 
                "Confirm Clear All", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                Loans.Clear();
                CommandManager.InvalidateRequerySuggested();
                System.Windows.MessageBox.Show("All loans have been cleared.", 
                    "Clear Complete", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Information);
            }
        }

        private bool CanClearLoans()
        {
            return Loans.Count > 0;
        }

        private void Loan_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Update totals when any loan property changes
            if (e.PropertyName == nameof(Loan.CurrentBalance) || 
                e.PropertyName == nameof(Loan.MonthlyPayment) ||
                e.PropertyName == nameof(Loan.MonthlyInterest))
            {
                OnPropertyChanged(nameof(TotalBalance));
                OnPropertyChanged(nameof(TotalMonthlyPayment));
                OnPropertyChanged(nameof(TotalMonthlyInterest));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}