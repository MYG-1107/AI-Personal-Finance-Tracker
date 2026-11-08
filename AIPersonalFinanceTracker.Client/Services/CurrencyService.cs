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
        var currencies = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
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
            .OrderBy(currency => currency.Code)
            .ToList();

        if (currencies.Count == 0)
            currencies.Add(new CurrencyOption { Code = "USD", Symbol = "$", Name = "USD - United States Dollar" });

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
