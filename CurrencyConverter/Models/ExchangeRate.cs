using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Models
{
    public class ExchangeRate
    {
        [Key] // Add this annotation to specify the primary key
        public int CurrencyID { get; set; }
        public string CurrencyCode { get; set; }
        public double Rate { get; set; }
    }
}
