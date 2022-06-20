using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pivot
{
	public class CSVReaderSimple
	{
		private string _dateFormat;
		public CSVReaderSimple(string dateFormat)
		{
			_dateFormat = dateFormat;
		}
		List<ParseError> ParseErrors = new List<ParseError>();

		private List<ForwardQuote> ReadFromReader(TextReader reader)
		{

			List<ForwardQuote> result = new List<ForwardQuote>();
			string line = null;
			int lineNumber = 0;

			while ((line = reader.ReadLine()) != null)
			{
				if (lineNumber > 0) // skip header
				{
					if (!string.IsNullOrEmpty(line))
					{
						ForwardQuote fq = null;
						try
						{
							fq = ForwardQuote.Parse(line, _dateFormat);
							if (fq == null)
								ParseErrors.Add( new ParseError(lineNumber, "Cannot parse row"));
							else
								result.Add(fq);
						}
						catch (Exception e)
						{
							Console.Error.WriteLine(line);
							Console.Error.WriteLine(e.ToString());
							ParseErrors.Add( new ParseError(lineNumber,e.ToString()));
						}

					}
				}
				lineNumber++;
			}

			return result;
		}
		public List<ForwardQuote> ReadCSVFromString(string text)
		{
			using StringReader reader = new StringReader(text);
			try
			{
				return ReadFromReader(reader);
			}
			finally
			{
				reader.Close();
			}
		}
		public List<ForwardQuote> ReadCSVFile(string fileName)
		{
			
			using StreamReader reader = new StreamReader(fileName);
			try
			{
				return  ReadFromReader(reader);
			}
			finally
			{
				reader.Close();
			}
		}
	}
}
