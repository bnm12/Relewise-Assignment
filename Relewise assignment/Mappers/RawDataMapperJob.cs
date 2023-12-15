using Relewise.Client.DataTypes;
using Relewise_assignment.Mappers.Interfaces;
using System.Configuration;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Relewise_assignment.Mappers
{
    public class RawDataMapperJob : IJob
	{
        // Throws if no url is configured - I think this is preferable since there's no good way to recover
		public static Uri dataUrl = new(ConfigurationManager.AppSettings.Get("RawDataUrl"));
		private HttpClient _httpClient;
        private CultureInfo _culture;

        public RawDataMapperJob(CultureInfo language)
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
            using (StreamReader responseStreamReader = new StreamReader(responseStream)) {

                int lineIndex = 0;
                int objectCount = 0;
                List<Product> products = new List<Product>();
                while(!responseStreamReader.EndOfStream)
                {
                    // Bail if canceled
                    token.ThrowIfCancellationRequested();

                    // This should only ever be null if the EndOfStream check has somehow failed, and then we have bigger problems
                    string line = await responseStreamReader.ReadLineAsync(token) ?? "";

					if (lineIndex < 2)
                    {
					    // Skip header
					    lineIndex++;
					    continue;
                    }

					string[] lineParts = line.Split('|', StringSplitOptions.TrimEntries);

                    if(lineParts.Length < 3)
                    {
                        // Don't know how to map something without an ID, skip
					    lineIndex++;
					    continue;
                    }   
                
                    var lineProduct = new Product(lineParts[1]);
                    // Since most fields are nullable we are doing a best effort mapping here to recover all we know for any entries that are only partially broken
                    lineProduct.DisplayName = lineParts.Length > 3 ? new Multilingual(new Language(_culture), lineParts[2]) : null;
				    lineProduct.Brand = lineParts.Length > 4 ? new Brand(lineParts[3]) { DisplayName= lineParts[3] } : null;
				    lineProduct.SalesPrice = lineParts.Length > 5 ? new MultiCurrency(MappingHelpers.extractCurrency(lineParts[4], _culture)) : null;
				    lineProduct.ListPrice = lineParts.Length > 6 ? new MultiCurrency(MappingHelpers.extractCurrency(lineParts[5], _culture)) : null;
				    lineProduct.CategoryPaths = lineParts.Length > 10 ? new List<CategoryPath>(){MappingHelpers.mapCategoryPath(lineParts[9], _culture)} : null;

                    products.Add(lineProduct);
				    lineIndex++;
                    objectCount++;
			    }

			    return $"{objectCount} objects mapped";
			}
		}
	}
}
