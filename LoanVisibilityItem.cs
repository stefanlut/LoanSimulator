using System.ComponentModel;

namespace LoanSimulator
{
    public class LoanVisibilityItem : INotifyPropertyChanged
    {
        private bool _isVisible = true;
        
        public Loan Loan { get; set; }
        public string LoanName => Loan.Name;
        
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        public LoanVisibilityItem(Loan loan)
        {
            Loan = loan;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
