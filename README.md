# Loan Simulator with AI-Powered Analysis

A comprehensive Windows Presentation Foundation (WPF) application for managing personal loans with AI-powered recommendations, interactive visualizations, and intelligent debt strategy optimization using local Ollama integration.

## 🚀 Key Features Overview

### 📊 **Comprehensive Loan Management**
- Multi-loan portfolio tracking with detailed financial metrics
- Smart loan editing with real-time validation
- Minimum payment tracking for better debt strategy planning
- Automated calculations for interest, payoff dates, and total obligations

### 🤖 **AI-Powered Loan Analysis** *(NEW)*
- **Local AI Integration**: Uses Ollama for privacy-focused loan analysis
- **Intelligent Recommendations**: Get personalized debt payoff strategies
- **Goal-Based Planning**: Optimize for different financial objectives
- **Smart Payment Allocation**: AI suggests optimal payment distributions
- **Risk Assessment**: Identifies potential financial risks and warnings

### 📈 **Interactive Data Visualization** *(NEW)*
- **Dynamic Charts**: Track loan balance progression over time
- **Customizable Views**: Show/hide individual loans or total balance
- **Multiple Time Ranges**: View projections from 5 to 30 years
- **Real-time Updates**: Charts automatically refresh when loan data changes

### 🎯 **Strategic Financial Planning** *(NEW)*
- **Goal Selection**: Choose from multiple debt payoff strategies
- **Budget Integration**: Factor in extra monthly budget and lump sums
- **Timeline Planning**: Set target dates for debt freedom
- **Comprehensive Analysis**: Get detailed insights and recommendations

## 📋 Detailed Features

### 💰 Loan Management Tab
- **Add Multiple Loans**: Track various loans with comprehensive details
- **Minimum Payment Tracking**: Distinguish between current and minimum required payments
- **Edit Existing Loans**: Real-time editing with automatic recalculation
- **Delete & Clear**: Remove individual loans or clear entire portfolio
- **Smart Totals**: Live calculation of total balances, payments, and minimum obligations

### 📈 Visualization Tab *(NEW)*
- **Interactive Charts**: Powered by LiveCharts for smooth, responsive visualizations
- **Loan Balance Projections**: See how each loan balance decreases over time
- **Visibility Controls**: Toggle individual loans or total balance line
- **Time Range Selection**: View 5, 10, 15, 20, or 30-year projections
- **Real-time Updates**: Charts automatically refresh when loan data changes

### 🤖 AI Advisor Tab *(NEW)*
- **Local AI Integration**: Uses Ollama for completely private loan analysis
- **Financial Goal Selection**:
  - Minimize Total Interest
  - Pay Off Fastest
  - Reduce Monthly Payments
  - Free Up Cash Flow
  - Debt Consolidation
- **Budget Planning**: Input extra monthly budget and available lump sums
- **Portfolio Overview**: Real-time summary of total debt, payments, and average rates
- **AI Recommendations**: Detailed payment strategies with reasoning
- **Key Insights**: Important observations about your debt portfolio
- **Risk Warnings**: Potential issues and recommendations for safer debt management

## 🛠 Technical Implementation

### Built With
- **Framework**: .NET 9.0
- **UI Technology**: Windows Presentation Foundation (WPF)
- **Architecture**: Model-View-ViewModel (MVVM) with comprehensive data binding
- **Visualization**: LiveCharts2 for interactive charting
- **AI Integration**: OllamaSharp 2.0.1 for local AI model communication
- **Data Storage**: JSON serialization with auto-save functionality
- **Language**: C#

### AI Integration Details
- **Local Processing**: All AI analysis happens locally using Ollama
- **Privacy First**: No loan data sent to external services
- **Fallback Support**: Rule-based recommendations when AI is unavailable
- **Structured Analysis**: JSON-based prompt engineering for consistent responses
- **Error Handling**: Robust error handling with graceful degradation

## 🚀 Getting Started

### Prerequisites
- **Windows 10 or later**
- **.NET 9.0 Runtime**
- **Ollama** (for AI features) - [Download from ollama.ai](https://ollama.ai)
  - Install any compatible model (e.g., `ollama pull llama2`, `ollama pull codellama`)
  - Ensure Ollama service is running on localhost:11434

### Installation
1. Download the latest release from the releases page
2. Extract the files to your desired location
3. Install Ollama for AI-powered analysis (optional but recommended)
4. Run `LoanSimulator.exe`

### Building from Source
```bash
# Clone the repository
git clone <repository-url>
cd LoanSimulator

# Restore dependencies (includes OllamaSharp)
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

## 📱 Usage Guide

### Adding Loans
1. Navigate to the **"Loan Management"** tab
2. Fill in the loan details:
   - **Loan Name**: Descriptive identifier (e.g., "Credit Card", "Car Loan")
   - **Current Balance**: Remaining amount owed
   - **Interest Rate**: Annual percentage rate (e.g., 4.25 for 4.25%)
   - **Monthly Payment**: Your current regular payment amount
   - **Minimum Payment**: Required minimum payment amount
   - **Payment Day**: Day of month when payment is due (1-28)
3. Click **"Add Loan"** to add it to your portfolio

### Visualizing Loan Progress *(NEW)*
1. Switch to the **"Loan Progress Visualization"** tab
2. Configure chart settings:
   - Select time range (5-30 years)
   - Choose which loans to display
   - Toggle total balance line visibility
3. View interactive loan balance projections over time
4. Use **"Refresh Chart"** to update after making loan changes

### Getting AI-Powered Recommendations *(NEW)*
1. Go to the **"AI Advisor"** tab
2. Set your financial goals:
   - Choose primary goal (minimize interest, pay off fastest, etc.)
   - Enter extra monthly budget available
   - Set target completion date (optional)
3. Click **"Analyze Loans with AI"**
4. Review comprehensive recommendations:
   - Overall strategy explanation
   - Specific payment recommendations per loan
   - Key insights about your debt portfolio
   - Important warnings and risk assessments

### File Operations
- **Auto-Save**: All changes automatically saved to default location
- **Save As**: Export portfolio to custom location
- **Load**: Import existing loan portfolios
- **Clear All**: Reset entire portfolio (with confirmation)

## 🔧 AI Setup Instructions

### Installing Ollama
1. Download Ollama from [ollama.ai](https://ollama.ai)
2. Install the application
3. Pull a compatible language model:
   ```bash
   ollama pull llama2
   # or
   ollama pull codellama
   # or any other supported model
   ```
4. Verify Ollama is running:
   ```bash
   ollama list
   ```

### AI Features Without Ollama
If Ollama is not available, the application will:
- Fall back to rule-based analysis
- Provide basic debt strategy recommendations
- Continue to function with all other features intact

## 📊 Understanding the Interface

### Loan Management Tab
**Loan Grid Columns:**
- **Loan Name**: Your custom identifier
- **Current Balance**: Remaining debt amount
- **Interest Rate**: Annual percentage rate
- **Monthly Payment**: Current payment amount
- **Minimum Payment**: Required minimum payment *(NEW)*
- **Monthly Interest**: Calculated interest charge
- **Payment Day**: Payment due date
- **Estimated Payoff**: Calculated completion date

**Totals Section:**
- **Total Balance**: Sum of all remaining balances
- **Total Monthly Payment**: Sum of current payments
- **Total Minimum Payment**: Sum of minimum required payments *(NEW)*
- **Total Monthly Interest**: Sum of all interest charges

### Visualization Tab *(NEW)*
- **Time Range Controls**: Select projection period
- **Loan Visibility**: Show/hide individual loans
- **Interactive Chart**: Hover for detailed data points
- **Real-time Updates**: Automatic refresh on data changes

### AI Advisor Tab *(NEW)*
- **Goal Selection**: Choose debt payoff strategy
- **Budget Input**: Enter available extra funds
- **Portfolio Overview**: Current debt summary
- **AI Analysis**: Comprehensive recommendations
- **Payment Strategies**: Loan-specific advice
- **Insights & Warnings**: Important observations

## 🏗 Technical Details

### Project Architecture
```
LoanSimulator/
├── App.xaml                    # Application definition and startup
├── App.xaml.cs                 # Application code-behind
├── MainWindow.xaml             # Main UI with tabbed interface
├── MainWindow.xaml.cs          # Main window code-behind
├── MainViewModel.cs            # Primary view model (MVVM pattern)
├── Loan.cs                     # Loan data model with property binding
├── LoanDataService.cs          # JSON persistence and file operations
├── LoanAdvisor.cs              # AI integration and analysis engine (NEW)
├── Converters.cs               # WPF data binding converters (NEW)
├── RelayCommand.cs             # Command pattern implementation
└── LoanSimulator.csproj        # Project configuration with dependencies
```

### Dependencies
- **LiveChartsCore.SkiaSharpView.WPF**: Interactive charting and visualization
- **OllamaSharp**: Local AI model integration (2.0.1)
- **System.Text.Json**: High-performance JSON serialization
- **.NET 9.0**: Latest framework with performance improvements

### Data Storage
**Default Location:**
```
%LOCALAPPDATA%\LoanSimulator\loans.json
```

**JSON Structure:**
```json
{
  "loans": [
    {
      "name": "Credit Card",
      "currentBalance": 5000.00,
      "interestRate": 18.99,
      "monthlyPayment": 200.00,
      "minimumPayment": 150.00,
      "paymentDay": 15
    }
  ],
  "lastModified": "2025-07-14T10:30:00Z"
}
```

## 🧮 Financial Calculations

### Interest Calculations
```csharp
// Monthly Interest
monthlyInterest = (currentBalance × annualInterestRate) ÷ 12

// Payoff Estimation
while (balance > 0) {
    monthlyInterest = balance × (annualRate / 12)
    principalPayment = monthlyPayment - monthlyInterest
    balance -= principalPayment
    monthsToPayoff++
}
```

### AI Analysis Integration
The AI advisor uses sophisticated prompt engineering to analyze your loan portfolio:

1. **Data Preparation**: Loans converted to structured format for AI analysis
2. **Goal-Based Prompting**: Different prompts based on selected financial goals
3. **JSON Response Parsing**: Structured AI responses for consistent recommendations
4. **Fallback Logic**: Rule-based analysis when AI is unavailable
5. **Risk Assessment**: Automated warnings for potential financial risks

## 🔄 Version History

### v2.0.0 - AI-Powered Analysis *(Current)*
- **🤖 AI Integration**: Local Ollama integration for intelligent loan analysis
- **📈 Visualization**: Interactive charts with LiveCharts2
- **💡 Smart Recommendations**: Goal-based debt payoff strategies
- **📊 Enhanced UI**: Three-tab interface with comprehensive features
- **🎯 Minimum Payment Tracking**: Better distinction between current and minimum payments
- **⚡ Performance Improvements**: Optimized calculations and real-time updates

### v1.0.0 - Foundation
- Basic loan management functionality
- Auto-save and file operations
- Financial calculations and totals
- Editable loan grid interface
- Payment day scheduling

## 🤝 Contributing

We welcome contributions to make the Loan Simulator even better! Here's how you can help:

### Development Setup
1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/yourusername/LoanSimulator.git
   cd LoanSimulator
   ```
3. **Install dependencies**:
   ```bash
   dotnet restore
   ```
4. **Set up Ollama** for AI testing:
   ```bash
   ollama pull llama2
   ```

### Making Changes
1. **Create a feature branch**:
   ```bash
   git checkout -b feature/amazing-new-feature
   ```
2. **Make your changes** following the existing code style
3. **Test thoroughly**:
   - Test with and without Ollama running
   - Verify UI responsiveness across different screen sizes
   - Test loan calculations with various scenarios
4. **Commit your changes**:
   ```bash
   git commit -m 'Add amazing new feature: detailed description'
   ```
5. **Push to your fork**:
   ```bash
   git push origin feature/amazing-new-feature
   ```
6. **Open a Pull Request** with detailed description

### Areas for Contribution
- **🎨 UI/UX Improvements**: Better layouts, themes, accessibility
- **📊 Additional Chart Types**: New visualization options
- **🤖 AI Enhancements**: Better prompts, additional AI providers
- **💱 Currency Support**: Multi-currency loan tracking
- **📱 Responsive Design**: Better support for different screen sizes
- **🔧 Performance**: Optimization and code improvements
- **📚 Documentation**: Improved guides and examples

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for complete details.

**Key Points:**
- ✅ Free for personal and commercial use
- ✅ Modify and distribute as needed
- ✅ Include original license in distributions
- ❌ No warranty or liability from authors

## 🆘 Support & Troubleshooting

### Common Issues

**AI Analysis Not Working:**
- Ensure Ollama is installed and running
- Verify a model is pulled: `ollama list`
- Check Ollama is accessible at `localhost:11434`
- Application will fall back to rule-based analysis if AI unavailable

**Charts Not Displaying:**
- Verify loans have been added to the portfolio
- Check that at least one loan is visible in chart settings
- Try refreshing the chart manually

**Data Not Saving:**
- Check write permissions to `%LOCALAPPDATA%\LoanSimulator\`
- Verify disk space availability
- Try "Save As" to a different location

### Getting Help
1. **Check existing issues** in the GitHub repository
2. **Search documentation** for similar problems
3. **Create a new issue** with:
   - Detailed problem description
   - Steps to reproduce
   - System information (Windows version, .NET version)
   - Error messages or screenshots
   - Whether Ollama is installed/running

### Feature Requests
We love hearing about new ideas! When requesting features:
- Describe the use case and benefit
- Provide examples of how it would work
- Consider if it fits the application's scope
- Check if similar requests already exist

## 🎯 Roadmap

### Planned Features
- **📱 Mobile Companion**: Web-based mobile interface
- **☁️ Cloud Sync**: Optional cloud backup and sync
- **📈 Advanced Analytics**: Debt-to-income ratios, payment optimization
- **💳 Bank Integration**: Direct import from financial institutions
- **🎨 Themes**: Dark mode and customizable themes
- **📊 Export Options**: PDF reports, Excel export
- **🔔 Notifications**: Payment reminders and milestone alerts

---

**💡 Remember**: This application provides estimates and recommendations for educational purposes. Always consult with qualified financial advisors for important financial decisions.

**🔒 Privacy**: All AI analysis happens locally on your machine. No loan data is transmitted to external services when using the recommended Ollama setup.
