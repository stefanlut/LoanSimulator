using System.ComponentModel;

namespace LoanSimulator
{
    public class Loan : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private decimal _currentBalance;
        private decimal _interestRate;
        private decimal _monthlyPayment;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public decimal CurrentBalance
        {
            get => _currentBalance;
            set
            {
                _currentBalance = value;
                OnPropertyChanged(nameof(CurrentBalance));
                OnPropertyChanged(nameof(EstimatedPayoffDate));
                OnPropertyChanged(nameof(MonthlyInterest));
            }
        }

        public decimal InterestRate
        {
            get => _interestRate;
            set
            {
                _interestRate = value;
                OnPropertyChanged(nameof(InterestRate));
                OnPropertyChanged(nameof(EstimatedPayoffDate));
                OnPropertyChanged(nameof(MonthlyInterest));
            }
        }

        public decimal MonthlyPayment
        {
            get => _monthlyPayment;
            set
            {
                _monthlyPayment = value;
                OnPropertyChanged(nameof(MonthlyPayment));
                OnPropertyChanged(nameof(EstimatedPayoffDate));
            }
        }

        private int _paymentDay = 1;

        public int PaymentDay
        {
            get => _paymentDay;
            set
            {
                if (value >= 1 && value <= 28) // Limit to 28 to avoid month-end issues
                {
                    _paymentDay = value;
                    OnPropertyChanged(nameof(PaymentDay));
                    OnPropertyChanged(nameof(EstimatedPayoffDate));
                }
            }
        }

        public DateTime? EstimatedPayoffDate
        {
            get
            {
                return CalculatePayoffDate();
            }
        }

        public decimal MonthlyInterest
        {
            get
            {
                if (CurrentBalance <= 0 || InterestRate < 0)
                    return 0;
                
                decimal monthlyInterestRate = InterestRate / 100 / 12;
                return CurrentBalance * monthlyInterestRate;
            }
        }

        private DateTime? CalculatePayoffDate()
        {
            if (CurrentBalance <= 0 || MonthlyPayment <= 0 || InterestRate < 0)
                return null;

            decimal monthlyInterestRate = InterestRate / 100 / 12;
            
            // If monthly payment is less than or equal to monthly interest, loan will never be paid off
            decimal monthlyInterest = CurrentBalance * monthlyInterestRate;
            if (MonthlyPayment <= monthlyInterest)
                return null;

            decimal balance = CurrentBalance;
            DateTime currentDate = DateTime.Today;
            
            // Set to the next payment date
            DateTime paymentDate = new DateTime(currentDate.Year, currentDate.Month, Math.Min(PaymentDay, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)));
            if (paymentDate <= currentDate)
            {
                paymentDate = paymentDate.AddMonths(1);
                paymentDate = new DateTime(paymentDate.Year, paymentDate.Month, Math.Min(PaymentDay, DateTime.DaysInMonth(paymentDate.Year, paymentDate.Month)));
            }

            int monthsToPayoff = 0;
            const int maxMonths = 1200; // Prevent infinite loops (100 years max)

            while (balance > 0 && monthsToPayoff < maxMonths)
            {
                monthlyInterest = balance * monthlyInterestRate;
                decimal principalPayment = MonthlyPayment - monthlyInterest;
                
                if (principalPayment <= 0)
                    return null; // Loan will never be paid off
                
                balance -= principalPayment;
                monthsToPayoff++;
                
                if (balance <= 0)
                    break;
                
                paymentDate = paymentDate.AddMonths(1);
                paymentDate = new DateTime(paymentDate.Year, paymentDate.Month, Math.Min(PaymentDay, DateTime.DaysInMonth(paymentDate.Year, paymentDate.Month)));
            }

            return monthsToPayoff >= maxMonths ? null : paymentDate;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}