using Relewise.Client.DataTypes;
using Relewise_assignment.Mappers.Interfaces;
using System.Configuration;
using System.Globalization;
using System.Net.Http.Json;

namespace Relewise_assignment.Mappers
{
    public class JSONDataMapperJob : IJob
	{
		// Throws if no url is configured - I think this is preferable since there's no good way to recover
		public static Uri dataUrl = new(ConfigurationManager.AppSettings.Get("JSONDataUrl"));
		private HttpClient _httpClient;
		private CultureInfo _culture;

		public JSONDataMapperJob(CultureInfo language)
		{
			_httpClient = new HttpClient();
			_culture = language;
		}

		public async Task<string> Execute(JobArguments arguments, Func<string, Task> info, Func<string, Task> warn, CancellationToken token)
		{
			List<JsonProductData> result = await _httpClient.GetFromJsonAsync<List<JsonProductData>>(dataUrl, token);

			int objectCount = 0;
			List<Product> products = new List<Product>();
			foreach (var element in result)
			{
				token.ThrowIfCancellationRequested();

				var lineProduct = new Product(element.productId);
				lineProduct.DisplayName = new Multilingual(new Language(_culture), element.productName);
				lineProduct.Brand = new Brand(element.brandName) { DisplayName = element.brandName };
				lineProduct.SalesPrice = new MultiCurrency(MappingHelpers.extractCurrency(element.salesPrice, _culture));
				lineProduct.ListPrice = new MultiCurrency(MappingHelpers.extractCurrency(element.listPrice, _culture));
				lineProduct.CategoryPaths = new List<CategoryPath>(){MappingHelpers.mapCategoryPath(element.category, _culture)};

				products.Add(lineProduct);
				objectCount++;
			}
			return $"{objectCount} objects mapped";
		}
	}

	internal class JsonProductData
	{
		public string productId { get; set; }
		public string productName { get; set; }
		public string brandName { get; set; }
		public string salesPrice { get; set; }
		public string listPrice { get; set; }
		public string shortDescription { get; set; }
		public string inStock { get; set; }
		public string color { get; set; }
		public string category { get; set; }
	}

}
