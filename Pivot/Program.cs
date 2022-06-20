using System;

namespace Pivot
{
	class Program
	{
		
		public static readonly string cDefaultDateTimeFormat = "d/M/yyyy";
		static void Main(string[] args)
		{
			// process command line switches
			int i = 0;
			string dtFormat = cDefaultDateTimeFormat;
			bool bUseSort = true;
			bool unknownSwitch = false;
			for (;i< args.Length;i++)
			{
				if (args[i].StartsWith("--"))
				{
					if (args[i].StartsWith("--df="))
						dtFormat = args[i].Substring("--df=".Length);
					else if (args[i].Equals("--alg=linq"))
						bUseSort = false;
					else if (args[i].Equals("--alg=sort"))
						bUseSort = true;
					else
					{
						unknownSwitch = true;
						break;
					}


				}
				else
					break;
			}
			// 2 more params are mandatory after last switch
			if (unknownSwitch || args.Length != i+2)
			{
				Console.Out.WriteLine("Sytax:");
				Console.Out.WriteLine("    PIVOT [--alg=sort|linq] [--df={dateformat}] {input file} {output file}");
				return;
			}

			int inputFileIndex = args.Length - 2;
			int  outputFileIndex = args.Length - 1;


			var inputFile = args[inputFileIndex];
			var outputFile = args[outputFileIndex];
			var csvReader = new CSVReaderSimple(dtFormat);
			var quotesList = csvReader.ReadCSVFile(inputFile);

			IPivotAlgorithm pivot = bUseSort ? new PivotSort(dtFormat) : new PivotLINQ(dtFormat);
			pivot.Initialize(quotesList);
			pivot.WriteToCSV(outputFile);
		}
	}
}

