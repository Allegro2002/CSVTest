using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pivot
{
	interface IPivotAlgorithm
	{
		void Initialize(List<ForwardQuote> quotes);
		void WriteTo(TextWriter writer);
		void WriteToCSV(string outputFile);

	}
}
