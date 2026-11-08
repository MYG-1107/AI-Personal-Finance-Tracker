using System.Globalization;

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

    public List<CurrencyOption> Currencies { get; } = BuildCurrencies();

    private static List<CurrencyOption> BuildCurrencies()
    {
        var currencies = new List<CurrencyOption>
        {
            new() { Code = "USD", Symbol = "$", Name = "USD - United States Dollar" },
            new() { Code = "EUR", Symbol = "€", Name = "EUR - Euro" },
            new() { Code = "GBP", Symbol = "£", Name = "GBP - British Pound" },
            new() { Code = "INR", Symbol = "₹", Name = "INR - Indian Rupee" },
            new() { Code = "CAD", Symbol = "CA$", Name = "CAD - Canadian Dollar" },
            new() { Code = "AUD", Symbol = "A$", Name = "AUD - Australian Dollar" },
            new() { Code = "JPY", Symbol = "¥", Name = "JPY - Japanese Yen" },
            new() { Code = "CNY", Symbol = "CN¥", Name = "CNY - Chinese Yuan" },
            new() { Code = "CHF", Symbol = "CHF", Name = "CHF - Swiss Franc" },
            new() { Code = "BRL", Symbol = "R$", Name = "BRL - Brazilian Real" },
            new() { Code = "ZAR", Symbol = "R", Name = "ZAR - South African Rand" },
            new() { Code = "AED", Symbol = "د.إ", Name = "AED - UAE Dirham" },
            new() { Code = "SAR", Symbol = "﷼", Name = "SAR - Saudi Riyal" },
            new() { Code = "SGD", Symbol = "S$", Name = "SGD - Singapore Dollar" },
            new() { Code = "HKD", Symbol = "HK$", Name = "HKD - Hong Kong Dollar" },
            new() { Code = "NZD", Symbol = "NZ$", Name = "NZD - New Zealand Dollar" },
            new() { Code = "MXN", Symbol = "MX$", Name = "MXN - Mexican Peso" },
            new() { Code = "KRW", Symbol = "₩", Name = "KRW - South Korean Won" },
            new() { Code = "THB", Symbol = "฿", Name = "THB - Thai Baht" },
            new() { Code = "IDR", Symbol = "Rp", Name = "IDR - Indonesian Rupiah" },
            new() { Code = "MYR", Symbol = "RM", Name = "MYR - Malaysian Ringgit" },
            new() { Code = "TRY", Symbol = "₺", Name = "TRY - Turkish Lira" },
            new() { Code = "NOK", Symbol = "kr", Name = "NOK - Norwegian Krone" },
            new() { Code = "SEK", Symbol = "kr", Name = "SEK - Swedish Krona" },
            new() { Code = "DKK", Symbol = "kr", Name = "DKK - Danish Krone" },
            new() { Code = "PLN", Symbol = "zł", Name = "PLN - Polish Zloty" },
            new() { Code = "CZK", Symbol = "Kč", Name = "CZK - Czech Koruna" },
            new() { Code = "ILS", Symbol = "₪", Name = "ILS - Israeli New Shekel" },
            new() { Code = "PHP", Symbol = "₱", Name = "PHP - Philippine Peso" },
            new() { Code = "VND", Symbol = "₫", Name = "VND - Vietnamese Dong" }
        };

        IEnumerable<CurrencyOption> cultureCurrencies = Array.Empty<CurrencyOption>();
        try
        {
            cultureCurrencies = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Select(culture =>
                {
                    try
                    {
                        var region = new RegionInfo(culture.Name);
                        return new CurrencyOption
                        {
                            Code = region.ISOCurrencySymbol,
                            Symbol = region.CurrencySymbol,
                            Name = $"{region.ISOCurrencySymbol} - {region.CurrencyEnglishName}"
                        };
                    }
                    catch (ArgumentException)
                    {
                        return null;
                    }
                })
                .Where(currency => currency is not null)
                .GroupBy(currency => currency!.Code)
                .Select(group => group.First()!)
                .ToList();
        }
        catch (CultureNotFoundException)
        {
            // The client uses invariant globalization, so the built-in catalog is authoritative.
        }

        foreach (var currency in cultureCurrencies)
        {
            if (currencies.All(existing => existing.Code != currency.Code))
                currencies.Add(currency);
        }

        return currencies;
    }

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
