using Relewise.Client.DataTypes;
using Relewise_assignment.Mappers.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Relewise_assignment.Mappers
{
    public class ShoppingFeedDataMapperJob : IJob
	{
		// Throws if no url is configured - I think this is preferable since there's no good way to recover
		public static Uri dataUrl = new(ConfigurationManager.AppSettings.Get("ShoppingFeedDataUrl"));
		private HttpClient _httpClient;
		private CultureInfo _culture;

		public ShoppingFeedDataMapperJob(CultureInfo language)
		{
			_httpClient = new HttpClient();
			_culture = language;
		}

		public async Task<string> Execute(JobArguments arguments, Func<string, Task> info, Func<string, Task> warn, CancellationToken token)
		{
			HttpResponseMessage? rawDataResponse = await _httpClient.GetAsync(dataUrl, token);

			if (!rawDataResponse.IsSuccessStatusCode)
			{
				// Throw exception? What are the execption semantics supposed to be in this setup?
				return "0 objects mapped";
			}

			Stream responseStream = await rawDataResponse.Content.ReadAsStreamAsync(token);

			XmlSerializer serializer = new XmlSerializer(typeof(Rss));
			var responseData = (Rss)serializer.Deserialize(responseStream);

			if(responseData == null)
			{
				// Throw exception? What are the execption semantics supposed to be in this setup?
				return "0 objects mapped";
			}

			int objectCount = 0;
			List<Product> products = new List<Product>();
			foreach (Item item in responseData.Channel.Item)
			{
				token.ThrowIfCancellationRequested();

				var lineProduct = new Product(item.Id);
				lineProduct.DisplayName = new Multilingual(new Language(_culture), item.Title);
				lineProduct.Brand = new Brand(item.Brand) { DisplayName = item.Brand };
				lineProduct.SalesPrice = new MultiCurrency(MappingHelpers.extractCurrency(item.SalePrice, _culture));
				lineProduct.ListPrice = new MultiCurrency(MappingHelpers.extractCurrency(item.Price, _culture));
				lineProduct.CategoryPaths = new List<CategoryPath>(){MappingHelpers.mapCategoryPath(item.ProductType, _culture)};

				products.Add(lineProduct);
				objectCount++;
			}
			return $"{objectCount} objects mapped";
		}
	}

	[XmlRoot(ElementName = "item", Namespace = "")]
	public class Item
	{

		[XmlElement(ElementName = "id", Namespace = "http://base.google.com/ns/1.0")]
		public string Id { get; set; }

		[XmlElement(ElementName = "title", Namespace = "")]
		public string Title { get; set; }

		[XmlElement(ElementName = "description", Namespace = "")]
		public string Description { get; set; }

		[XmlElement(ElementName = "link", Namespace = "")]
		public string Link { get; set; }

		[XmlElement(ElementName = "image_link", Namespace = "http://base.google.com/ns/1.0")]
		public string ImageLink { get; set; }

		[XmlElement(ElementName = "availability", Namespace = "http://base.google.com/ns/1.0")]
		public string Availability { get; set; }

		[XmlElement(ElementName = "price", Namespace = "http://base.google.com/ns/1.0")]
		public string Price { get; set; }

		[XmlElement(ElementName = "sale_price", Namespace = "http://base.google.com/ns/1.0")]
		public string SalePrice { get; set; }

		[XmlElement(ElementName = "brand", Namespace = "http://base.google.com/ns/1.0")]
		public string Brand { get; set; }

		[XmlElement(ElementName = "product_type", Namespace = "http://base.google.com/ns/1.0")]
		public string ProductType { get; set; }

		[XmlElement(ElementName = "color", Namespace = "http://base.google.com/ns/1.0")]
		public string Color { get; set; }

		[XmlElement(ElementName = "condition", Namespace = "http://base.google.com/ns/1.0")]
		public string Condition { get; set; }

		[XmlElement(ElementName = "identifier_exists", Namespace = "http://base.google.com/ns/1.0")]
		public string IdentifierExists { get; set; }
	}

	[XmlRoot(ElementName = "channel", Namespace = "")]
	public class Channel
	{

		[XmlElement(ElementName = "title", Namespace = "")]
		public string Title { get; set; }

		[XmlElement(ElementName = "link", Namespace = "")]
		public string Link { get; set; }

		[XmlElement(ElementName = "description", Namespace = "")]
		public string Description { get; set; }

		[XmlElement(ElementName = "item", Namespace = "")]
		public List<Item> Item { get; set; }
	}

	[XmlRoot(ElementName = "rss", Namespace = "")]
	public class Rss
	{

		[XmlElement(ElementName = "channel", Namespace = "")]
		public Channel Channel { get; set; }

		[XmlAttribute(AttributeName = "g", Namespace = "http://www.w3.org/2000/xmlns/")]
		public string G { get; set; }

		[XmlAttribute(AttributeName = "version", Namespace = "")]
		public double Version { get; set; }

		[XmlText]
		public string Text { get; set; }
	}
}