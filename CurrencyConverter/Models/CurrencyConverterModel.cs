namespace CurrencyConverter.Models
{
    public class CurrencyConverterModel
    {
        public string CurrencyFrom { get; set; }
        public string CurrencyTo { get; set; }
        public double Amount { get; set; }
        public double ConvertedAmount { get; set; }
    }
}
