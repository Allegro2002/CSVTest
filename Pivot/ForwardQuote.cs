using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pivot
{
	public class ForwardQuote
	{

		public DateTime ObservationDate { get; set; }
		public string Label { get; set; }
		public DateTime From { get; set; }
		public DateTime To { get; set; }
		public string Price;


		/// <summary>
		/// Parses CSV line. Thows FormatException in case when line has errors
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public static ForwardQuote Parse(string line, string dateFormat)
		{
			var tokens = CSVUtils.QuoteAwareSplit(line);
			if (tokens == null || tokens.Length != 5) // error case
			{
				throw new FormatException();
			}
			DateTime dtFrom, dtTo,dtObsvDate;
			
			bool fromFieldParsed = DateTime.TryParseExact(tokens[2], dateFormat, null, DateTimeStyles.None, out dtFrom);
			bool toFieldParsed = DateTime.TryParseExact(tokens[3], dateFormat, null, DateTimeStyles.None, out dtTo);
			DateTime.TryParseExact(tokens[0], dateFormat, null, DateTimeStyles.None, out dtObsvDate);
			if(!DateTime.TryParseExact(tokens[0], dateFormat, null, DateTimeStyles.None, out dtObsvDate) || dtObsvDate < new DateTime(2000,1,1))
				throw new Exception("Observation date is incorrect");

			if (!fromFieldParsed && toFieldParsed)
			{
				dtFrom = dtTo.AddDays(1).AddMonths(-3);
			}
			else if (fromFieldParsed && !toFieldParsed)
			{
				dtTo = dtFrom.AddMonths(3).AddDays(-1);
			}
			else if (!fromFieldParsed && !toFieldParsed)
				throw new Exception("Parse error: no date range specified");

			return  new ForwardQuote()
			{
				ObservationDate = DateTime.ParseExact(tokens[0], dateFormat, null),
				Label = tokens[1],
				From = dtFrom,
				To = dtTo,
				Price = tokens[4]
			};

		}
	}
}
