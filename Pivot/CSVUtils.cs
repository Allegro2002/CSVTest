using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pivot
{
	static class CSVUtils
	{
		delegate void MethodInvoker();
		/// <summary>
		/// Splits string into tokens taking into account quote symbol
		/// </summary>
		/// <param name="str"string to be split</param>
		/// <returns>returns array of tokens or null in case of processing error</returns>
		/// 
		public static string[] QuoteAwareSplit(string str, char separator = ',', char quoteChar = '"')
		{
			if (str == null)
				throw new ArgumentNullException();
			int tokenStart = 0;
			List<string> tokens = new List<string>();
			bool inQuote = false, escape = false;
			string replaceOldValue = null, replaceNewValue = null;
			int i = 0;
			MethodInvoker addTokenFunc = delegate ()
			{
				var token = str.Substring(tokenStart, i-tokenStart);
				if (tokenStart < str.Length && str[tokenStart] == quoteChar)
				{
					if (replaceOldValue == null)
					{
						replaceOldValue = new string(new char[] { quoteChar, quoteChar });
						replaceNewValue = quoteChar.ToString();
					}
					token = token.Substring(1, token.Length - 2).Replace(replaceOldValue, replaceNewValue);
				}
				tokens.Add(token);

			};

			for (; i < str.Length; i++)
			{
				if (str[i] == quoteChar)
				{
					if (i == tokenStart)
					{
						inQuote = true;
					}
					else if (inQuote)
					{
						if (i + 1 > str.Length)
						{
							if (str[i + 1] == quoteChar)
								escape = true;
							else
								inQuote = false;
						}
						else
							inQuote = false;
					}
				}
				else if (str[i] == separator && !inQuote)
				{
					addTokenFunc();
					tokenStart = i + 1;
				}
			}

			if (inQuote)
			{
				return null;
			}
			// add last token 
			addTokenFunc();
			return tokens.ToArray();
		}
	}
}
