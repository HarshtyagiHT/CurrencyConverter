using CurrencyConverter.Controllers;
using CurrencyConverter.Models;
using CurrencyConverter.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace CurrencyConverterTest
{
    public class CurrencyConverterControllerTests
    {
        [Fact]
        public void ConvertCurrencyFromDatabase_WithValidModel_ShouldReturnJsonResult()
        {
            // Arrange
            var exchangeRatesData = new List<ExchangeRate>
            {
            new ExchangeRate { CurrencyCode = "USD", Rate = 1.0 },
            new ExchangeRate { CurrencyCode = "EUR", Rate = 0.9 },
            }.AsQueryable();

            var exchangeRatesMock = new Mock<DbSet<ExchangeRate>>();
            exchangeRatesMock.As<IQueryable<ExchangeRate>>().Setup(m => m.Provider).Returns(exchangeRatesData.Provider);
            exchangeRatesMock.As<IQueryable<ExchangeRate>>().Setup(m => m.Expression).Returns(exchangeRatesData.Expression);
            exchangeRatesMock.As<IQueryable<ExchangeRate>>().Setup(m => m.ElementType).Returns(exchangeRatesData.ElementType);
            exchangeRatesMock.As<IQueryable<ExchangeRate>>().Setup(m => m.GetEnumerator()).Returns(exchangeRatesData.GetEnumerator());

            var dbContextMock = new Mock<AppDbContext>();
            dbContextMock.Setup(x => x.ExchangeRates).Returns(exchangeRatesMock.Object);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient(new MockHttpMessageHandler()));

            var controller = new CurrencyConverterController(dbContextMock.Object, httpClientFactoryMock.Object);

            var model = new CurrencyConverterModel
            {
                CurrencyFrom = "USD",
                CurrencyTo = "EUR",
                Amount = 100
            };

            // Act
            var result = controller.ConvertCurrencyFromDatabase(model) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonResult = result as JsonResult;
            var jsonString = JsonConvert.SerializeObject(jsonResult.Value);
            dynamic data = JsonConvert.DeserializeObject(jsonString);
            Assert.NotNull(data);
            Assert.Null(data.error);
            var convertedAmount = data.convertedAmount.Value; // Extracting convertedAmount
            Assert.NotNull(convertedAmount);
        }



        [Fact]
        public void ConvertCurrencyFromApi_WithValidModel_ShouldReturnJsonResult()
        {
            //Done
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient(new MockHttpMessageHandler()));

            var controller = new CurrencyConverterController(null, httpClientFactoryMock.Object);

            var model = new CurrencyConverterModel
            {
                CurrencyFrom = "USD",
                CurrencyTo = "EUR",
                Amount = 100
            };

            // Act
            var result = controller.ConvertCurrencyFromApi(model).Result as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonResult = result as JsonResult;
            var jsonString = JsonConvert.SerializeObject(jsonResult.Value);
            dynamic data = JsonConvert.DeserializeObject(jsonString);
            Assert.NotNull(data);
            Assert.Null(data.error);
            var convertedAmount = data.convertedAmount.Value; // Extracting convertedAmount
            Assert.NotNull(convertedAmount);
        }
        [Fact]
        public void ConvertCurrencyFromDatabase_WithInvalidCurrency_ShouldReturnError()
        {
            // Arrange
            var exchangeRatesData = new List<ExchangeRate>
    {
        new ExchangeRate { CurrencyCode = "USD", Rate = 1.0 },
        new ExchangeRate { CurrencyCode = "EUR", Rate = 0.9 },
    }.AsQueryable();

            var exchangeRatesMock = new Mock<DbSet<ExchangeRate>>();
            exchangeRatesMock.As<IQueryable<ExchangeRate>>().Setup(m => m.Provider).Returns(exchangeRatesData.Provider);
            exchangeRatesMock.As<IQueryable<ExchangeRate>>().Setup(m => m.Expression).Returns(exchangeRatesData.Expression);
            exchangeRatesMock.As<IQueryable<ExchangeRate>>().Setup(m => m.ElementType).Returns(exchangeRatesData.ElementType);
            exchangeRatesMock.As<IQueryable<ExchangeRate>>().Setup(m => m.GetEnumerator()).Returns(exchangeRatesData.GetEnumerator());

            var dbContextMock = new Mock<AppDbContext>();
            dbContextMock.Setup(x => x.ExchangeRates).Returns(exchangeRatesMock.Object);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient(new MockHttpMessageHandler()));

            var controller = new CurrencyConverterController(dbContextMock.Object, httpClientFactoryMock.Object);

            var model = new CurrencyConverterModel
            {
                CurrencyFrom = "USD",
                CurrencyTo = "INVALID", // Invalid currency code
                Amount = 100
            };

            // Act
            var result = controller.ConvertCurrencyFromDatabase(model) as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonResult = result as JsonResult;
            var jsonString = JsonConvert.SerializeObject(jsonResult.Value);
            dynamic data = JsonConvert.DeserializeObject(jsonString);
            Assert.NotNull(data);
            Assert.NotNull(data.error);
            Assert.Null(data.convertedAmount);
        }

        [Fact]
        public void ConvertCurrencyFromApi_WithInvalidCurrency_ShouldReturnError()
        {
            // Arrange
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient(new MockHttpMessageHandler()));

            var controller = new CurrencyConverterController(null, httpClientFactoryMock.Object);

            var model = new CurrencyConverterModel
            {
                CurrencyFrom = "USD",
                CurrencyTo = "INVALID", // Invalid currency code
                Amount = 100
            };

            // Act
            var result = controller.ConvertCurrencyFromApi(model).Result as JsonResult;

            // Assert
            Assert.NotNull(result);
            var jsonResult = result as JsonResult;
            var jsonString = JsonConvert.SerializeObject(jsonResult.Value);
            dynamic data = JsonConvert.DeserializeObject(jsonString);
            Assert.NotNull(data);
            Assert.NotNull(data.error);
            Assert.Null(data.convertedAmount);
        }


        private DbSet<ExchangeRate> GetTestExchangeRates()
        {
            var data = new List<ExchangeRate>
            {
                new ExchangeRate { CurrencyCode = "USD", Rate = 1.0 },
                new ExchangeRate { CurrencyCode = "EUR", Rate = 0.9 },
            }.AsQueryable();

            var mockSet = new Mock<DbSet<ExchangeRate>>();
            mockSet.As<IQueryable<ExchangeRate>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<ExchangeRate>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<ExchangeRate>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<ExchangeRate>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            return mockSet.Object;
        }

        // Define MockHttpMessageHandler
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // Simulate a successful API response
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(@"{
                        ""conversion_rates"": {
                            ""USD"": 1.0,
                            ""EUR"": 0.9
                        }
                    }")
                };

                return Task.FromResult(response);
            }
        }
    }
}
