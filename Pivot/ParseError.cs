using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pivot
{
	class ParseError
	{
		public int LineNumber { get; set; }
		public string ErrorText { get; set; }
		public ParseError(int lineNumber, string errorText)
		{
			LineNumber = lineNumber;
			ErrorText = errorText;
		}
	}
}
