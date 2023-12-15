using Relewise.Client.DataTypes;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Relewise_assignment.Mappers
{
	static internal class MappingHelpers
	{
		static internal Money extractCurrency(string input, CultureInfo culture)
		{
			// Consider: Can we get the culture based on the currency symbol or identifier, or does that not matter since we are passing in the culture to the jobs anyway?
			// Do we ever get cases where the language for the text is one culture but the currency is a different one?
			string pattern = @"^(?<CurrencySymbol>.*?)(?<Value>\d+\.{0,1}\d*) *(?<CurrencyIdentifier>.*?)$";
			var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);

			if (decimal.TryParse(match.Groups["Value"].Value, culture, out var result))
			{
				return new Money(new Currency(culture), result);
			}
			throw new InvalidDataException($"Invalid decimal format {match.Groups["Value"].Value}");
		}

		static internal CategoryPath mapCategoryPath(string input, CultureInfo culture)
		{
			return new CategoryPath(input.Split('>', StringSplitOptions.TrimEntries)
											.Select(cat =>
												new CategoryNameAndId(cat, new Multilingual(new Language(culture), cat))
											).ToArray());
		}
	}
}
