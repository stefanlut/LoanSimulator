# Loan Simulator

A Windows Presentation Foundation (WPF) application for managing and tracking personal loans with automatic calculations and data persistence.

## Features

### 📊 Loan Management
- **Add Multiple Loans**: Track various loans with customizable details
- **Edit Existing Loans**: Double-click any loan in the grid to modify its properties
- **Delete Loans**: Remove unwanted loans with a simple delete key press
- **Clear All Loans**: Bulk deletion with confirmation dialog

### 💰 Financial Calculations
- **Automatic Interest Calculation**: Monthly interest computed based on current balance and annual rate
- **Payoff Date Estimation**: Calculates when each loan will be paid off based on payment schedule
- **Real-time Totals**: Live updates of total balance, monthly payments, and monthly interest

### 💾 Data Persistence
- **Auto-Save**: Changes are automatically saved to your local data file
- **Custom File Operations**: Save to or load from any location on your computer
- **JSON Format**: Human-readable data storage format

### 🎯 User-Friendly Interface
- **Input Validation**: Ensures all loan data is valid before adding
- **Smart Formatting**: Currency and percentage values displayed properly
- **Confirmation Dialogs**: Prevents accidental data loss
- **Payment Day Scheduling**: Track when monthly payments are due (1-28th of month)

## Getting Started

### Prerequisites
- Windows 10 or later
- .NET 9.0 Runtime

### Installation
1. Download the latest release from the releases page
2. Extract the files to your desired location
3. Run `LoanSimulator.exe`

### Building from Source
```bash
# Clone the repository
git clone <repository-url>
cd LoanSimulator

# Build the project
dotnet build

# Run the application
dotnet run
```

## Usage

### Adding a New Loan
1. Fill in the loan details in the form:
   - **Loan Name**: A descriptive name for the loan
   - **Current Balance**: The remaining amount owed
   - **Interest Rate**: Annual percentage rate (e.g., 4.25 for 4.25%)
   - **Monthly Payment**: Your regular payment amount
   - **Payment Day**: Day of the month when payment is due (1-28)

2. Click **Add Loan** to add it to your portfolio

### Editing Loans
- **Edit Values**: Double-click any cell in the loan grid to modify values
- **Delete Loan**: Select a row and press the Delete key
- **Read-Only Fields**: Monthly Interest and Estimated Payoff Date are automatically calculated

### File Operations
- **Auto-Save**: Changes are saved automatically to your default data file
- **Save As**: Use the "Save As" button to save to a custom location
- **Load**: Use the "Load" button to import loans from a file
- **Clear All**: Remove all loans with confirmation

### Understanding the Display

#### Loan Grid Columns
- **Loan Name**: Your custom identifier for the loan
- **Current Balance**: Remaining amount owed
- **Interest Rate**: Annual percentage rate
- **Monthly Payment**: Your regular payment amount
- **Monthly Interest**: Calculated monthly interest charge
- **Payment Day**: Day of month when payment is due
- **Estimated Payoff**: Calculated completion date

#### Totals Section
- **Total Balance**: Sum of all loan balances
- **Total Monthly Payment**: Sum of all monthly payments
- **Total Monthly Interest**: Sum of all monthly interest charges

## Technical Details

### Built With
- **Framework**: .NET 9.0
- **UI Technology**: Windows Presentation Foundation (WPF)
- **Architecture**: Model-View-ViewModel (MVVM)
- **Data Storage**: JSON serialization
- **Language**: C#

### Project Structure
```
LoanSimulator/
├── App.xaml                 # Application definition
├── App.xaml.cs              # Application code-behind
├── MainWindow.xaml          # Main UI layout
├── MainWindow.xaml.cs       # Main window code-behind
├── MainViewModel.cs         # Main view model (MVVM)
├── Loan.cs                  # Loan data model
├── LoanDataService.cs       # Data persistence service
├── RelayCommand.cs          # Command implementation
└── LoanSimulator.csproj     # Project file
```

### Data Storage
Loan data is automatically saved to:
```
%LOCALAPPDATA%\LoanSimulator\loans.json
```

The JSON format stores:
- Loan name and financial details
- Payment scheduling information
- All data needed for calculations

## Calculations

### Monthly Interest
```
Monthly Interest = (Current Balance × Annual Interest Rate) ÷ 12
```

### Estimated Payoff Date
The payoff calculation uses compound interest formulas to determine when the loan will be completely paid off based on:
- Current balance
- Monthly payment amount
- Annual interest rate
- Payment day of month

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

If you encounter any issues or have suggestions for improvements, please:
1. Check the existing issues in the repository
2. Create a new issue with detailed information about the problem
3. Include steps to reproduce any bugs

## Version History

### v1.0.0
- Initial release
- Basic loan management functionality
- Auto-save and file operations
- Financial calculations and totals
- Editable loan grid
- Payment day scheduling

---

**Note**: This application is designed for personal loan tracking and estimation purposes. Always consult with financial professionals for important financial decisions.
