using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace LoanSimulator
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Loan _currentLoan = new Loan();
        private readonly LoanDataService _dataService;
        private int _selectedTimeRange = 30;

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

        public decimal TotalMinimumPayment
        {
            get
            {
                return Loans.Sum(loan => loan.MinimumPayment);
            }
        }

        public ICommand AddLoanCommand { get; }
        public ICommand SaveLoansCommand { get; }
        public ICommand SaveAsLoansCommand { get; }
        public ICommand LoadLoansCommand { get; }
        public ICommand ClearAllLoansCommand { get; }
        public ICommand RefreshChartCommand { get; }
        public ICommand ShowAllLoansCommand { get; }
        public ICommand HideAllLoansCommand { get; }
        public ICommand AnalyzeLoansCommand { get; }

        // Chart properties
        public ObservableCollection<ISeries> ChartSeries { get; } = new ObservableCollection<ISeries>();
        public ObservableCollection<LoanVisibilityItem> LoanVisibilityItems { get; } = new ObservableCollection<LoanVisibilityItem>();
        public Axis[] XAxes { get; set; }
        public Axis[] YAxes { get; set; }

        private bool _showTotalBalance = true;
        public bool ShowTotalBalance
        {
            get => _showTotalBalance;
            set
            {
                _showTotalBalance = value;
                OnPropertyChanged(nameof(ShowTotalBalance));
                RefreshChart();
            }
        }

        // AI Advisor properties
        private readonly LoanAdvisor _loanAdvisor = new LoanAdvisor();
        private string _selectedGoalType = "MinimizeTotalInterest";
        private decimal _extraBudget = 0;
        private DateTime? _targetDate;
        private string _overallStrategy = "Add loans and set your financial goals to get AI-powered recommendations.";
        private ObservableCollection<LoanAdvisor.PaymentRecommendation> _paymentRecommendations = new();
        private ObservableCollection<string> _keyInsights = new();
        private ObservableCollection<string> _warnings = new();
        private bool _isAnalyzing = false;

        public string SelectedGoalType
        {
            get => _selectedGoalType;
            set
            {
                _selectedGoalType = value;
                OnPropertyChanged(nameof(SelectedGoalType));
            }
        }

        public decimal ExtraBudget
        {
            get => _extraBudget;
            set
            {
                _extraBudget = value;
                OnPropertyChanged(nameof(ExtraBudget));
            }
        }

        public DateTime? TargetDate
        {
            get => _targetDate;
            set
            {
                _targetDate = value;
                OnPropertyChanged(nameof(TargetDate));
            }
        }

        public string OverallStrategy
        {
            get => _overallStrategy;
            set
            {
                _overallStrategy = value;
                OnPropertyChanged(nameof(OverallStrategy));
            }
        }

        public ObservableCollection<LoanAdvisor.PaymentRecommendation> PaymentRecommendations
        {
            get => _paymentRecommendations;
            set
            {
                _paymentRecommendations = value;
                OnPropertyChanged(nameof(PaymentRecommendations));
            }
        }

        public ObservableCollection<string> KeyInsights
        {
            get => _keyInsights;
            set
            {
                _keyInsights = value;
                OnPropertyChanged(nameof(KeyInsights));
            }
        }

        public ObservableCollection<string> Warnings
        {
            get => _warnings;
            set
            {
                _warnings = value;
                OnPropertyChanged(nameof(Warnings));
            }
        }

        public bool IsAnalyzing
        {
            get => _isAnalyzing;
            set
            {
                _isAnalyzing = value;
                OnPropertyChanged(nameof(IsAnalyzing));
            }
        }

        public decimal AverageInterestRate
        {
            get
            {
                if (Loans.Count == 0) return 0;
                return Loans.Average(l => l.InterestRate);
            }
        }

        public int SelectedTimeRange
        {
            get => _selectedTimeRange;
            set
            {
                _selectedTimeRange = value;
                OnPropertyChanged(nameof(SelectedTimeRange));
                RefreshChart();
            }
        }

        public MainViewModel()
        {
            _dataService = new LoanDataService();
            AddLoanCommand = new RelayCommand(AddLoan, CanAddLoan);
            SaveLoansCommand = new RelayCommand(async () => await SaveLoansAsync());
            SaveAsLoansCommand = new RelayCommand(async () => await SaveAsLoansAsync());
            LoadLoansCommand = new RelayCommand(async () => await LoadLoansAsync());
            ClearAllLoansCommand = new RelayCommand(ClearAllLoans, CanClearLoans);
            RefreshChartCommand = new RelayCommand(RefreshChart);
            ShowAllLoansCommand = new RelayCommand(ShowAllLoans);
            HideAllLoansCommand = new RelayCommand(HideAllLoans);
            AnalyzeLoansCommand = new RelayCommand(async () => await AnalyzeLoans(), () => Loans.Count > 0);

            // Initialize chart axes
            XAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Years",
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 }
                }
            };

            YAxes = new Axis[]
            {
                new Axis
                {
                    Name = "Balance ($)",
                    NamePaint = new SolidColorPaint(SKColors.Black),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray),
                    TextSize = 12,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 1 },
                    Labeler = value => value.ToString("C0")
                }
            };

            // Subscribe to collection changes to update totals and chart
            Loans.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(TotalBalance));
                OnPropertyChanged(nameof(TotalMonthlyPayment));
                OnPropertyChanged(nameof(TotalMonthlyInterest));
                OnPropertyChanged(nameof(TotalMinimumPayment));
                OnPropertyChanged(nameof(AverageInterestRate));

                // Subscribe to property changes of new items
                if (e.NewItems != null)
                {
                    foreach (Loan loan in e.NewItems)
                    {
                        loan.PropertyChanged += Loan_PropertyChanged;
                        
                        // Add to visibility collection
                        var visibilityItem = new LoanVisibilityItem(loan);
                        visibilityItem.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(LoanVisibilityItem.IsVisible))
                            {
                                RefreshChart();
                            }
                        };
                        LoanVisibilityItems.Add(visibilityItem);
                    }
                }

                // Unsubscribe from property changes of removed items
                if (e.OldItems != null)
                {
                    foreach (Loan loan in e.OldItems)
                    {
                        loan.PropertyChanged -= Loan_PropertyChanged;
                        
                        // Remove from visibility collection
                        var visibilityItem = LoanVisibilityItems.FirstOrDefault(vi => vi.Loan == loan);
                        if (visibilityItem != null)
                        {
                            LoanVisibilityItems.Remove(visibilityItem);
                        }
                    }
                }

                // Refresh chart when loans change
                RefreshChart();
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
                // Ensure visibility items are properly synchronized
                RebuildVisibilityItems();

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
                // Ensure visibility items are properly synchronized
                RebuildVisibilityItems();
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
                RebuildVisibilityItems(); // Ensure visibility items are properly cleared
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
                e.PropertyName == nameof(Loan.MonthlyInterest) ||
                e.PropertyName == nameof(Loan.MinimumPayment))
            {
                OnPropertyChanged(nameof(TotalBalance));
                OnPropertyChanged(nameof(TotalMonthlyPayment));
                OnPropertyChanged(nameof(TotalMonthlyInterest));
                OnPropertyChanged(nameof(TotalMinimumPayment));

                // Refresh chart when loan properties change
                RefreshChart();
            }
        }

        private void RefreshChart()
        {
            ChartSeries.Clear();
            
            if (Loans.Count == 0)
                return;

            var colors = new SKColor[]
            {
                SKColors.Blue, SKColors.Red, SKColors.Green, SKColors.Orange, 
                SKColors.Purple, SKColors.Brown, SKColors.Pink, SKColors.Gray
            };

            // Determine if we should show monthly detail (for periods less than 10 years)
            var showMonthlyDetail = SelectedTimeRange < 10;
            var monthsToProject = SelectedTimeRange * 12;
            
            // For detailed view, show every month; for overview, show every 6 months
            var stepSize = showMonthlyDetail ? 1 : 6;
            var timePoints = new List<double>();
            var dataPointCount = monthsToProject / stepSize + 1;
            
            // Generate time points
            for (int i = 0; i <= monthsToProject; i += stepSize)
            {
                timePoints.Add(i / 12.0); // Convert months to years
            }

            // Initialize total balance tracking
            var totalBalanceSeries = new List<double>();
            for (int i = 0; i < timePoints.Count; i++)
            {
                totalBalanceSeries.Add(0);
            }

            // Create a series for each visible loan
            var colorIndex = 0;
            for (int loanIndex = 0; loanIndex < Loans.Count; loanIndex++)
            {
                var loan = Loans[loanIndex];
                var visibilityItem = LoanVisibilityItems.FirstOrDefault(vi => vi.Loan == loan);
                
                // Skip if loan is not visible
                if (visibilityItem?.IsVisible != true)
                    continue;
                
                var loanBalances = new List<double>();
                var currentBalance = (double)loan.CurrentBalance;
                var monthlyPayment = (double)loan.MonthlyPayment;
                var monthlyInterestRate = (double)loan.InterestRate / 100.0 / 12.0;

                // Calculate balances for each time point
                var actualCurrentBalance = currentBalance;
                for (int pointIndex = 0; pointIndex < timePoints.Count; pointIndex++)
                {
                    var monthsElapsed = pointIndex * stepSize;
                    
                    // Reset to original balance and calculate forward to this time point
                    var balanceAtThisPoint = currentBalance;
                    for (int month = 0; month < monthsElapsed; month++)
                    {
                        if (balanceAtThisPoint <= 0)
                        {
                            balanceAtThisPoint = 0;
                            break;
                        }
                        
                        // Calculate this month's payment effect
                        var interestCharge = balanceAtThisPoint * monthlyInterestRate;
                        var principalPayment = Math.Max(0, monthlyPayment - interestCharge);
                        balanceAtThisPoint = Math.Max(0, balanceAtThisPoint - principalPayment);
                    }
                    
                    loanBalances.Add(balanceAtThisPoint);
                    
                    // Add to total balance if showing total
                    if (ShowTotalBalance)
                    {
                        totalBalanceSeries[pointIndex] += balanceAtThisPoint;
                    }
                }

                // Create series for this loan with appropriate styling
                var loanSeries = new LineSeries<double>
                {
                    Values = loanBalances,
                    Name = loan.Name,
                    Stroke = new SolidColorPaint(colors[colorIndex % colors.Length]) { StrokeThickness = 2 },
                    Fill = null,
                    // Keep geometry properties for legend colors but hide the actual dots
                    GeometryStroke = new SolidColorPaint(colors[colorIndex % colors.Length]) { StrokeThickness = 2 },
                    GeometryFill = new SolidColorPaint(colors[colorIndex % colors.Length]),
                    GeometrySize = 0 // Hide dots but keep color properties for legend
                };

                ChartSeries.Add(loanSeries);
                colorIndex++;
            }

            // Add total balance series if enabled and there are multiple visible loans
            if (ShowTotalBalance && ChartSeries.Count > 1)
            {
                var totalSeries = new LineSeries<double>
                {
                    Values = totalBalanceSeries,
                    Name = "Total Balance",
                    Stroke = new SolidColorPaint(SKColors.Black) { StrokeThickness = 3 },
                    Fill = null,
                    // Keep geometry properties for legend but hide dots
                    GeometryStroke = new SolidColorPaint(SKColors.Black) { StrokeThickness = 3 },
                    GeometryFill = new SolidColorPaint(SKColors.Black),
                    GeometrySize = 0 // Hide dots but keep color properties for legend
                };

                ChartSeries.Add(totalSeries);
            }

            // Update X-axis with proper range and labels
            XAxes[0].MinLimit = 0;
            XAxes[0].MaxLimit = SelectedTimeRange;
            
            // Adjust X-axis labeling based on time range
            if (showMonthlyDetail)
            {
                // For detailed view, show month labels more frequently
                XAxes[0].Name = $"Time (Years) - Monthly Detail";
            }
            else
            {
                XAxes[0].Name = "Time (Years)";
            }
        }

        private void ShowAllLoans()
        {
            foreach (var visibilityItem in LoanVisibilityItems)
            {
                visibilityItem.IsVisible = true;
            }
        }

        private void HideAllLoans()
        {
            foreach (var visibilityItem in LoanVisibilityItems)
            {
                visibilityItem.IsVisible = false;
            }
        }

        private async Task AnalyzeLoans()
        {
            if (Loans.Count == 0) return;

            IsAnalyzing = true;
            try
            {
                var goalType = SelectedGoalType switch
                {
                    "MinimizeTotalInterest" => LoanAdvisor.GoalType.MinimizeTotalInterest,
                    "PayOffFastest" => LoanAdvisor.GoalType.PayoffAllLoans,
                    "TargetDate" => LoanAdvisor.GoalType.PayoffAllLoans,
                    "ReducePayments" => LoanAdvisor.GoalType.ReduceMonthlyPayments,
                    "FreeUpCash" => LoanAdvisor.GoalType.FreeUpCashFlow,
                    "Consolidate" => LoanAdvisor.GoalType.DebtConsolidation,
                    _ => LoanAdvisor.GoalType.MinimizeTotalInterest
                };

                var goals = new List<LoanAdvisor.FinancialGoal>
                {
                    new LoanAdvisor.FinancialGoal
                    {
                        Type = goalType,
                        TargetAmount = ExtraBudget,
                        TargetDate = TargetDate ?? DateTime.Now.AddYears(5),
                        Priority = 1,
                        Description = $"Primary goal: {SelectedGoalType}"
                    }
                };

                var result = await _loanAdvisor.AnalyzeLoansAsync(
                    Loans.ToList(),
                    goals,
                    ExtraBudget
                );

                OverallStrategy = result.OverallStrategy;
                PaymentRecommendations = new ObservableCollection<LoanAdvisor.PaymentRecommendation>(result.Recommendations);
                KeyInsights = new ObservableCollection<string>(result.KeyInsights);
                Warnings = new ObservableCollection<string>(result.Warnings);
            }
            catch (Exception ex)
            {
                OverallStrategy = $"Error analyzing loans: {ex.Message}. Please ensure Ollama is running locally with a model available.";
                PaymentRecommendations.Clear();
                KeyInsights.Clear();
                Warnings.Clear();
            }
            finally
            {
                IsAnalyzing = false;
            }
        }

        private void RebuildVisibilityItems()
        {
            LoanVisibilityItems.Clear();
            foreach (var loan in Loans)
            {
                var visibilityItem = new LoanVisibilityItem(loan);
                visibilityItem.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(LoanVisibilityItem.IsVisible))
                    {
                        RefreshChart();
                    }
                };
                LoanVisibilityItems.Add(visibilityItem);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}