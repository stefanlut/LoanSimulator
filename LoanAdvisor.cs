using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System.Text.Json;

namespace LoanSimulator
{
    public class LoanAdvisor
    {
        private readonly OllamaApiClient _ollamaClient;
        private const string DEFAULT_MODEL = "llama3.2"; // You can change this to your preferred model
        
        public LoanAdvisor()
        {
            _ollamaClient = new OllamaApiClient("http://localhost:11434");
        }

        public class FinancialGoal
        {
            public GoalType Type { get; set; }
            public decimal TargetAmount { get; set; }
            public DateTime TargetDate { get; set; }
            public int Priority { get; set; } // 1-5, where 1 is highest priority
            public string Description { get; set; } = string.Empty;
        }

        public enum GoalType
        {
            PayoffAllLoans,
            PayoffSpecificLoan,
            ReduceMonthlyPayments,
            MinimizeTotalInterest,
            FreeUpCashFlow,
            DebtConsolidation
        }

        public class PaymentRecommendation
        {
            public string LoanName { get; set; } = string.Empty;
            public decimal RecommendedPayment { get; set; }
            public decimal ExtraPayment { get; set; }
            public string Reasoning { get; set; } = string.Empty;
            public decimal InterestSavings { get; set; }
            public TimeSpan TimeReduction { get; set; }
        }

        public class AnalysisResult
        {
            public List<PaymentRecommendation> Recommendations { get; set; } = new();
            public string OverallStrategy { get; set; } = string.Empty;
            public decimal TotalMonthlySavings { get; set; }
            public decimal TotalInterestSavings { get; set; }
            public TimeSpan OverallTimeReduction { get; set; }
            public List<string> KeyInsights { get; set; } = new();
            public List<string> Warnings { get; set; } = new();
            public bool IsAiPowered { get; set; } = false;
        }

        public async Task<AnalysisResult> AnalyzeLoansAsync(IEnumerable<Loan> loans, List<FinancialGoal> goals, decimal availableBudget = 0)
        {
            var result = new AnalysisResult();
            var loanList = loans.ToList();

            if (!loanList.Any())
            {
                result.OverallStrategy = "No loans to analyze. Consider this a good financial position!";
                return result;
            }

            try
            {
                // Try AI analysis first
                result = await AnalyzeWithAI(loanList, goals, availableBudget);
                result.IsAiPowered = true;
            }
            catch (Exception ex)
            {
                // Fallback to rule-based analysis
                Console.WriteLine($"AI analysis failed: {ex.Message}. Falling back to rule-based analysis.");
                result = AnalyzeWithRules(loanList, goals, availableBudget);
                result.IsAiPowered = false;
                result.Warnings.Add("AI analysis unavailable - using simplified rule-based recommendations. Ensure Ollama is running for better analysis.");
            }

            return result;
        }

        private async Task<AnalysisResult> AnalyzeWithAI(List<Loan> loans, List<FinancialGoal> goals, decimal availableBudget)
        {
            // Prepare loan data for the AI
            var loanData = loans.Select(l => new
            {
                Name = l.Name,
                Balance = l.CurrentBalance,
                InterestRate = l.InterestRate,
                MonthlyPayment = l.MonthlyPayment,
                MinimumPayment = l.MinimumPayment,
                EstimatedMonthsRemaining = l.MonthlyPayment > 0 ? (int)Math.Ceiling(l.CurrentBalance / l.MonthlyPayment) : 999
            }).ToList();

            var goalData = goals.Select(g => new
            {
                Type = g.Type.ToString(),
                Description = g.Description,
                Priority = g.Priority,
                TargetAmount = g.TargetAmount,
                TargetDate = g.TargetDate.ToString("yyyy-MM-dd")
            }).ToList();

            // Create a comprehensive prompt for the AI
            var prompt = $@"You are a financial advisor AI specializing in loan payment optimization. 

LOAN PORTFOLIO:
{JsonSerializer.Serialize(loanData, new JsonSerializerOptions { WriteIndented = true })}

FINANCIAL GOALS:
{JsonSerializer.Serialize(goalData, new JsonSerializerOptions { WriteIndented = true })}

AVAILABLE EXTRA BUDGET: ${availableBudget:F2}/month

ANALYSIS REQUEST:
Please analyze this loan portfolio and provide personalized recommendations. Consider:
1. Interest rate optimization (avalanche vs snowball methods)
2. Psychological factors and momentum building
3. Cash flow optimization
4. Risk factors and warnings
5. The specific financial goals provided

Please respond with a JSON object in this exact format:
{{
  ""overallStrategy"": ""A clear, actionable overall strategy explanation"",
  ""recommendations"": [
    {{
      ""loanName"": ""Name of the loan"",
      ""recommendedPayment"": 0.00,
      ""extraPayment"": 0.00,
      ""reasoning"": ""Clear explanation of why this payment amount"",
      ""estimatedInterestSavings"": 0.00,
      ""estimatedMonthsReduced"": 0
    }}
  ],
  ""keyInsights"": [
    ""Important insight 1"",
    ""Important insight 2""
  ],
  ""warnings"": [
    ""Warning about high interest rates, if any"",
    ""Other important warnings""
  ],
  ""totalInterestSavings"": 0.00,
  ""totalMonthlySavings"": 0.00
}}

IMPORTANT: Return ONLY valid JSON, no additional text or formatting.";

            try
            {
                var request = new GenerateCompletionRequest
                {
                    Model = DEFAULT_MODEL,
                    Prompt = prompt,
                    Stream = false
                };

                var response = await _ollamaClient.GetCompletion(request);
                var aiResponse = response.Response ?? "";

                // Parse the AI response
                return ParseAIResponse(aiResponse, loans);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get AI analysis: {ex.Message}", ex);
            }
        }

        private AnalysisResult ParseAIResponse(string aiResponse, List<Loan> loans)
        {
            try
            {
                // Clean up the response - remove any markdown formatting or extra text
                var jsonStart = aiResponse.IndexOf('{');
                var jsonEnd = aiResponse.LastIndexOf('}');
                
                if (jsonStart == -1 || jsonEnd == -1 || jsonEnd <= jsonStart)
                {
                    throw new Exception("No valid JSON found in AI response");
                }

                var jsonContent = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                
                using var doc = JsonDocument.Parse(jsonContent);
                var root = doc.RootElement;

                var result = new AnalysisResult
                {
                    OverallStrategy = root.GetProperty("overallStrategy").GetString() ?? "Strategy not provided",
                    TotalInterestSavings = root.TryGetProperty("totalInterestSavings", out var totalSavings) ? totalSavings.GetDecimal() : 0,
                    TotalMonthlySavings = root.TryGetProperty("totalMonthlySavings", out var monthlySavings) ? monthlySavings.GetDecimal() : 0
                };

                // Parse recommendations
                if (root.TryGetProperty("recommendations", out var recommendations))
                {
                    foreach (var rec in recommendations.EnumerateArray())
                    {
                        var recommendation = new PaymentRecommendation
                        {
                            LoanName = rec.GetProperty("loanName").GetString() ?? "",
                            RecommendedPayment = rec.GetProperty("recommendedPayment").GetDecimal(),
                            ExtraPayment = rec.GetProperty("extraPayment").GetDecimal(),
                            Reasoning = rec.GetProperty("reasoning").GetString() ?? "",
                            InterestSavings = rec.TryGetProperty("estimatedInterestSavings", out var intSavings) ? intSavings.GetDecimal() : 0,
                            TimeReduction = rec.TryGetProperty("estimatedMonthsReduced", out var monthsReduced) 
                                ? TimeSpan.FromDays(monthsReduced.GetInt32() * 30.44) 
                                : TimeSpan.Zero
                        };
                        
                        result.Recommendations.Add(recommendation);
                    }
                }

                // Parse insights
                if (root.TryGetProperty("keyInsights", out var insights))
                {
                    foreach (var insight in insights.EnumerateArray())
                    {
                        result.KeyInsights.Add(insight.GetString() ?? "");
                    }
                }

                // Parse warnings
                if (root.TryGetProperty("warnings", out var warnings))
                {
                    foreach (var warning in warnings.EnumerateArray())
                    {
                        result.Warnings.Add(warning.GetString() ?? "");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse AI response: {ex.Message}. Raw response: {aiResponse.Substring(0, Math.Min(200, aiResponse.Length))}...", ex);
            }
        }

        private AnalysisResult AnalyzeWithRules(List<Loan> loans, List<FinancialGoal> goals, decimal availableBudget)
        {
            var result = new AnalysisResult();
            
            // Determine primary goal
            var primaryGoal = goals.OrderBy(g => g.Priority).FirstOrDefault();
            var goalType = primaryGoal?.Type ?? GoalType.MinimizeTotalInterest;

            // Apply rule-based analysis based on goal type
            switch (goalType)
            {
                case GoalType.MinimizeTotalInterest:
                    result = AnalyzeForMinimumInterest(loans, availableBudget);
                    break;
                case GoalType.PayoffAllLoans:
                    result = AnalyzeForFastestPayoff(loans, availableBudget);
                    break;
                case GoalType.ReduceMonthlyPayments:
                    result = AnalyzeForReducedPayments(loans, availableBudget);
                    break;
                case GoalType.FreeUpCashFlow:
                    result = AnalyzeForCashFlow(loans, availableBudget);
                    break;
                case GoalType.DebtConsolidation:
                    result = AnalyzeForConsolidation(loans, availableBudget);
                    break;
                default:
                    result = AnalyzeForMinimumInterest(loans, availableBudget);
                    break;
            }

            // Add common insights
            var totalBalance = loans.Sum(l => l.CurrentBalance);
            var totalMinPayment = loans.Sum(l => l.MinimumPayment);
            var averageRate = loans.Average(l => l.InterestRate);

            result.KeyInsights.Add($"Total debt: {totalBalance:C}");
            result.KeyInsights.Add($"Total minimum payments: {totalMinPayment:C}/month");
            result.KeyInsights.Add($"Average interest rate: {averageRate:P2}");

            if (availableBudget > 0)
            {
                result.KeyInsights.Add($"Extra budget available: {availableBudget:C}/month");
            }

            // Add warnings for high-risk situations
            result.Warnings.AddRange(GenerateWarnings(loans, availableBudget));

            return result;
        }

        // Synchronous wrapper for backward compatibility
        public AnalysisResult AnalyzeLoans(IEnumerable<Loan> loans, List<FinancialGoal> goals, decimal availableBudget = 0)
        {
            return AnalyzeLoansAsync(loans, goals, availableBudget).GetAwaiter().GetResult();
        }

        #region Rule-based Analysis Methods

        private AnalysisResult AnalyzeForMinimumInterest(List<Loan> loans, decimal extraBudget)
        {
            var result = new AnalysisResult();
            result.OverallStrategy = "Focus on paying off highest interest rate loans first (Avalanche Method) to minimize total interest paid.";

            // Sort by interest rate descending
            var sortedLoans = loans.OrderByDescending(l => l.InterestRate).ToList();
            
            foreach (var loan in sortedLoans)
            {
                var extraPayment = loan == sortedLoans.First() ? extraBudget : 0;
                
                result.Recommendations.Add(new PaymentRecommendation
                {
                    LoanName = loan.Name,
                    RecommendedPayment = loan.MonthlyPayment + extraPayment,
                    ExtraPayment = extraPayment,
                    Reasoning = loan == sortedLoans.First() && extraPayment > 0 
                        ? $"Highest interest rate ({loan.InterestRate:P2}) - apply all extra budget here"
                        : $"Interest rate: {loan.InterestRate:P2} - minimum payment only",
                    InterestSavings = CalculateInterestSavings(loan, extraPayment),
                    TimeReduction = CalculateTimeReduction(loan, extraPayment)
                });
            }

            return result;
        }

        private AnalysisResult AnalyzeForFastestPayoff(List<Loan> loans, decimal extraBudget)
        {
            var result = new AnalysisResult();
            result.OverallStrategy = "Focus on paying off smallest balances first (Snowball Method) to build momentum and free up cash flow quickly.";

            // Sort by balance ascending
            var sortedLoans = loans.OrderBy(l => l.CurrentBalance).ToList();
            
            foreach (var loan in sortedLoans)
            {
                var extraPayment = loan == sortedLoans.First() ? extraBudget : 0;
                
                result.Recommendations.Add(new PaymentRecommendation
                {
                    LoanName = loan.Name,
                    RecommendedPayment = loan.MonthlyPayment + extraPayment,
                    ExtraPayment = extraPayment,
                    Reasoning = loan == sortedLoans.First() && extraPayment > 0 
                        ? $"Smallest balance ({loan.CurrentBalance:C}) - focus here for quick wins"
                        : $"Balance: {loan.CurrentBalance:C} - minimum payment until smaller loans are paid off",
                    InterestSavings = CalculateInterestSavings(loan, extraPayment),
                    TimeReduction = CalculateTimeReduction(loan, extraPayment)
                });
            }

            return result;
        }

        private AnalysisResult AnalyzeForReducedPayments(List<Loan> loans, decimal extraBudget)
        {
            var result = new AnalysisResult();
            result.OverallStrategy = "Consider refinancing high-interest loans to reduce monthly payment burden.";

            foreach (var loan in loans)
            {
                var reasoning = loan.InterestRate > 0.07m 
                    ? $"High interest rate ({loan.InterestRate:P2}) - consider refinancing"
                    : $"Reasonable rate ({loan.InterestRate:P2}) - maintain current payment";

                result.Recommendations.Add(new PaymentRecommendation
                {
                    LoanName = loan.Name,
                    RecommendedPayment = loan.MonthlyPayment,
                    ExtraPayment = 0,
                    Reasoning = reasoning,
                    InterestSavings = 0,
                    TimeReduction = TimeSpan.Zero
                });
            }

            if (extraBudget > 0)
            {
                result.KeyInsights.Add($"Consider using extra budget ({extraBudget:C}) for emergency fund instead of extra payments");
            }

            return result;
        }

        private AnalysisResult AnalyzeForCashFlow(List<Loan> loans, decimal extraBudget)
        {
            var result = new AnalysisResult();
            result.OverallStrategy = "Focus on paying off loans with shortest remaining terms to free up monthly cash flow soonest.";

            foreach (var loan in loans)
            {
                var monthsRemaining = CalculateMonthsRemaining(loan);
                
                result.Recommendations.Add(new PaymentRecommendation
                {
                    LoanName = loan.Name,
                    RecommendedPayment = loan.MonthlyPayment,
                    ExtraPayment = 0,
                    Reasoning = $"Approximately {monthsRemaining} months remaining at current payment",
                    InterestSavings = 0,
                    TimeReduction = TimeSpan.Zero
                });
            }

            return result;
        }

        private AnalysisResult AnalyzeForConsolidation(List<Loan> loans, decimal extraBudget)
        {
            var result = new AnalysisResult();
            
            if (loans.Count > 1)
            {
                var averageRate = loans.Average(l => l.InterestRate);
                var totalBalance = loans.Sum(l => l.CurrentBalance);
                
                result.OverallStrategy = $"Consider consolidating {loans.Count} loans (total: {totalBalance:C}) if you can get a rate lower than {averageRate:P2}.";
                
                result.KeyInsights.Add("Consolidation benefits: Simplified payments, potentially lower rate");
                result.KeyInsights.Add("Consolidation risks: May lose benefits like loan forgiveness programs");
            }
            else
            {
                result.OverallStrategy = "Only one loan exists - consolidation not applicable. Focus on extra payments.";
            }

            foreach (var loan in loans)
            {
                result.Recommendations.Add(new PaymentRecommendation
                {
                    LoanName = loan.Name,
                    RecommendedPayment = loan.MonthlyPayment,
                    ExtraPayment = 0,
                    Reasoning = loans.Count > 1 ? "Consider for consolidation" : "Single loan - maintain payments",
                    InterestSavings = 0,
                    TimeReduction = TimeSpan.Zero
                });
            }

            return result;
        }

        private decimal CalculateInterestSavings(Loan loan, decimal extraPayment)
        {
            if (extraPayment <= 0) return 0;

            // Simplified calculation - more accurate calculation would require amortization schedule
            var monthsWithExtra = loan.CurrentBalance / (loan.MonthlyPayment + extraPayment);
            var monthsWithoutExtra = loan.CurrentBalance / loan.MonthlyPayment;
            var timeSaved = monthsWithoutExtra - monthsWithExtra;
            
            return timeSaved * loan.MonthlyPayment * (loan.InterestRate / 12);
        }

        private TimeSpan CalculateTimeReduction(Loan loan, decimal extraPayment)
        {
            if (extraPayment <= 0) return TimeSpan.Zero;

            var monthsWithExtra = loan.CurrentBalance / (loan.MonthlyPayment + extraPayment);
            var monthsWithoutExtra = loan.CurrentBalance / loan.MonthlyPayment;
            var monthsSaved = monthsWithoutExtra - monthsWithExtra;
            
            return TimeSpan.FromDays((double)monthsSaved * 30.44); // Average days per month
        }

        private int CalculateMonthsRemaining(Loan loan)
        {
            // Simplified calculation
            return (int)Math.Ceiling(loan.CurrentBalance / loan.MonthlyPayment);
        }

        private List<string> GenerateWarnings(List<Loan> loans, decimal extraBudget)
        {
            var warnings = new List<string>();

            // High interest rate warning
            var highInterestLoans = loans.Where(l => l.InterestRate > 0.15m).ToList();
            if (highInterestLoans.Any())
            {
                warnings.Add($"{highInterestLoans.Count} loan(s) have very high interest rates (>15%) - prioritize these!");
            }

            // Low payment warning
            foreach (var loan in loans)
            {
                var interestOnly = loan.CurrentBalance * loan.InterestRate / 12;
                if (loan.MonthlyPayment < interestOnly * 1.1m) // Less than 110% of interest
                {
                    warnings.Add($"{loan.Name}: Payment barely covers interest - consider increasing payment");
                }
            }

            // Budget strain warning
            var totalMinPayments = loans.Sum(l => l.MinimumPayment);
            if (extraBudget > totalMinPayments * 0.5m)
            {
                warnings.Add("Extra budget is very high relative to minimum payments - ensure emergency fund is adequate first");
            }

            return warnings;
        }

        #endregion
    }
}