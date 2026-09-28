namespace AIPersonalFinanceTracker.Client.Services;

public class CurrencyService
{
    public event Action? OnCurrencyChanged;
    public string SelectedCode { get; private set; } = "USD";
    public string SelectedSymbol { get; private set; } = "$";

    public class CurrencyOption
    {
        public string Code { get; set; } = "";
        public string Symbol { get; set; } = "";
        public string Name { get; set; } = "";
    }

    public List<CurrencyOption> Currencies { get; } = new()
    {
        new CurrencyOption { Code = "USD", Symbol = "$", Name = "USD ($) - United States Dollar" },
        new CurrencyOption { Code = "EUR", Symbol = "EUR", Name = "EUR (EUR) - Euro" },
        new CurrencyOption { Code = "GBP", Symbol = "GBP", Name = "GBP (GBP) - British Pound" },
        new CurrencyOption { Code = "INR", Symbol = "INR", Name = "INR (INR) - Indian Rupee" },
        new CurrencyOption { Code = "JPY", Symbol = "JPY", Name = "JPY (JPY) - Japanese Yen" },
        new CurrencyOption { Code = "CAD", Symbol = "CAD$", Name = "CAD (CAD$) - Canadian Dollar" },
        new CurrencyOption { Code = "AUD", Symbol = "AUD$", Name = "AUD (AUD$) - Australian Dollar" },
        new CurrencyOption { Code = "NOK", Symbol = "NOK", Name = "NOK (kr) - Norwegian Krone" },
        new CurrencyOption { Code = "SEK", Symbol = "SEK", Name = "SEK (kr) - Swedish Krona" },
        new CurrencyOption { Code = "CHF", Symbol = "CHF", Name = "CHF - Swiss Franc" },
        new CurrencyOption { Code = "BRL", Symbol = "R$", Name = "BRL (R$) - Brazilian Real" },
        new CurrencyOption { Code = "AED", Symbol = "AED", Name = "AED - UAE Dirham" }
    };

    public void SetCurrency(string code)
    {
        var match = Currencies.FirstOrDefault(c => c.Code == code);
        if (match != null)
        {
            SelectedCode = match.Code;
            SelectedSymbol = match.Symbol;
            OnCurrencyChanged?.Invoke();
        }
    }

    public string Format(decimal amount)
    {
        return $"{SelectedSymbol} {Math.Abs(amount):N2}";
    }
}
