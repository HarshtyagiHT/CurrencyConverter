using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.Models;
using CurrencyConverter.Models.Data;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CurrencyConverter.Controllers
{
    public class CurrencyConverterController : Controller
    {
        private readonly AppDbContext _dbContext;
        private Dictionary<string, double> _exchangeRates;
        private readonly HttpClient _httpClient;

        public CurrencyConverterController(AppDbContext dbContext, IHttpClientFactory httpClientFactory)
        {
            _dbContext = dbContext;
            _httpClient = httpClientFactory.CreateClient();
        }

        private Dictionary<string, double> GetExchangeRatesFromDatabase()
        {
            try
            {
                var exchangeRates = _dbContext.ExchangeRates.ToDictionary(r => r.CurrencyCode.ToUpper(), r => r.Rate);
                return exchangeRates;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving exchange rates from the database.", ex);
            }
        }

        private double GetExchangeRateFromDatabase(string currencyCode)
        {
            if (_exchangeRates.TryGetValue(currencyCode.ToUpper(), out var rate))
            {
                return rate;
            }

            throw new ArgumentException("Invalid currency code provided.");
        }

        // GET: CurrencyConverterController1
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConvertCurrencyFromDatabase(CurrencyConverterModel model)
        {
            try
            {
                // Data Validation
                if (!ModelState.IsValid)
                {
                    return Json(new { error = "Invalid input. Please check your data." });
                }

                _exchangeRates = GetExchangeRatesFromDatabase();
                double exchangeRateFrom = GetExchangeRateFromDatabase(model.CurrencyFrom);
                double exchangeRateTo = GetExchangeRateFromDatabase(model.CurrencyTo);

                // Perform the conversion
                double convertedAmount = model.Amount * Convert.ToDouble(exchangeRateTo / exchangeRateFrom);

                return Json(new { convertedAmount });
            }
            catch (ArgumentException ex)
            {
                // Handle invalid currency code
                return Json(new { error = ex.Message });
            }
            
            catch (Exception ex)
            {
                // Log and handle other exceptions
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]

        public async Task<IActionResult> ConvertCurrencyFromApi(CurrencyConverterModel model)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Make the API call
                    string apiUrl = "https://v6.exchangerate-api.com/v6/46946c7e1fc2e9041bc53af5/latest/USD";
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Read and parse the JSON response
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        ExchangeRatesApiResponse apiResponse = JsonConvert.DeserializeObject<ExchangeRatesApiResponse>(jsonResponse);
                        if (apiResponse != null && apiResponse.conversion_rates != null)
                        {
                            // Extract the conversion rates
                            var conversionRates = apiResponse.conversion_rates;

                            // Check if the currencies exist in the conversion rates
                            if (conversionRates.GetType().GetProperty(model.CurrencyFrom.ToUpper()) != null &&
                                conversionRates.GetType().GetProperty(model.CurrencyTo.ToUpper()) != null)
                            {
                                // Retrieve the exchange rates
                                double exchangeRateFrom = Convert.ToDouble(conversionRates.GetType().GetProperty(model.CurrencyFrom.ToUpper()).GetValue(conversionRates));
                                double exchangeRateTo = Convert.ToDouble(conversionRates.GetType().GetProperty(model.CurrencyTo.ToUpper()).GetValue(conversionRates));

                                // Perform the conversion
                                if (exchangeRateFrom != 0)
                                {
                                    double convertedAmount = model.Amount * (exchangeRateTo / exchangeRateFrom);
                                    return Json(new { convertedAmount });
                                }
                                else
                                {
                                    return Json(new { error = "Exchange rate for the selected currency is zero." });
                                }
                            }
                            else
                            {
                                return Json(new { error = "Selected currencies not found in conversion rates." });
                            }
                        }
                        else
                        {
                            return Json(new { error = "Invalid API response or conversion rates are missing." });
                        }


                    }

                    // Handle API call failure
                    return Json(new { error = "Failed to fetch exchange rates from the API." });
                }
            }
            catch (Exception ex)
            {
                // Log and handle exceptions
                return Json(new { error = ex.Message });
            }
        }

    }
}
