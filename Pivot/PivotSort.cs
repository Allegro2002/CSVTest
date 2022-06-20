using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pivot
{
	public class PivotSort: IPivotAlgorithm
	{
		class Label
		{
			public Label(string label)
			{
				Labels.Add(label, 1);
			}
			public Dictionary<string, int> Labels = new Dictionary<string, int>();

			public void Add(string label)
			{
				if (!Labels.ContainsKey(label))
					Labels.Add(label, 1);
				else
					Labels[label]++;
			}

			public string GetLabel()
			{
				if (Labels.Count == 0)
				{
					return "";
				}
				else if (Labels.Count == 1)
				{
					return Labels.Keys.First();
				}
				else
				{
					return Labels.AsEnumerable().OrderByDescending(p => p.Value).First().Key;
				}
			}
		}
		private const char separator = ',';
		List<ForwardQuote> quotes;
		SortedList<DateTime, Label> ColumnLabels;
		private string dateFormat;
		public PivotSort(string dtFormat)
		{
			dateFormat = dtFormat;
		}

		private void Sort()
		{
			quotes.Sort((q1, q2) =>
		 {
			 if (q1 == null)
			 {
				 if (q2 == null)
					 return 0;
				 else
					 return -1;
			 }
			 if (q2 == null)
				 return 1;
			 int compareResult = q1.ObservationDate.CompareTo(q2.ObservationDate);
			 if (compareResult != 0)
				 return compareResult;
			 return q1.From.CompareTo(q2.From);
		 });
		}

		public void Initialize(List<ForwardQuote> quotes)
		{
			this.quotes = quotes;
			Scan();
			Sort();
		}
		/// <summary>
		/// Prepares list of columns for Privot grid
		/// </summary>
		private void Scan()
		{
			ColumnLabels = new SortedList<DateTime, Label>();
			foreach (var quote in quotes)
			{
				if (!ColumnLabels.ContainsKey(quote.From))
					ColumnLabels.Add(quote.From, new Label(quote.Label));
				else
					ColumnLabels[quote.From].Add(quote.Label);
			}
		}

		public IEnumerable<string> GeneratePivot()
		{
			StringBuilder line = new StringBuilder();
			// output header
			line.Append(separator);
			foreach (var entry in ColumnLabels)
			{
				line.Append(entry.Value.GetLabel()).Append(separator);
			}
			// remove last separator
			line.Length = line.Length - 1;
			yield return line.ToString();
			line.Length = 0;


			// first row initialization
			var firstQuote = quotes.FirstOrDefault();
			if (firstQuote != null)
			{
				DateTime curDate = firstQuote?.ObservationDate ?? DateTime.MinValue;
				line.Append(curDate.ToString(dateFormat)).Append(separator);

				int columnIndex = 0;
				foreach (var quote in quotes)
				{
					if (curDate != quote.ObservationDate)
					{
						// all quotes for observation date are processed: finish up all empty columns and return line
						int emptyColumnsToAppend = ColumnLabels.Count - columnIndex-1;

						if (emptyColumnsToAppend > 0)
							line.Append(separator, emptyColumnsToAppend);
						else if(emptyColumnsToAppend < 0)
							line.Length = line.Length - 1;

						yield return line.ToString();

						// reset row string and initialize new row
						curDate = quote.ObservationDate;
						line.Length = 0;
						line.Append(curDate.ToString(dateFormat)).Append(separator);

						columnIndex = 0;
					}


					// find place to put quote into result row
					// 1. put empty columns  for cells which go earlier than currently processed record
					while (columnIndex < ColumnLabels.Count && ColumnLabels.Keys[columnIndex] < quote.From)
					{
						line.Append(separator);
						columnIndex++;
					}
					// if we are in correct cell - put the value there,
					// if for some reason we have duplicate it's value will be less then current column value and it will be skipped
					if (columnIndex < ColumnLabels.Count && ColumnLabels.Keys[columnIndex] == quote.From)
					{
						line.Append(quote.Price).Append(separator);
						columnIndex++;
					}
				}

				// write final part of the last row
				if (line.Length > 0)
				{
					int emptyColumnsToAppend = ColumnLabels.Count - columnIndex - 1;

					if (emptyColumnsToAppend > 0)
						line.Append(separator, emptyColumnsToAppend);
					else
						line.Length = line.Length - 1;

					yield return line.ToString();
				}
			}
		}

		public void WriteTo(TextWriter writer)
		{
			foreach (var line in GeneratePivot())
			{
				writer.WriteLine(line);
			};
		}
		public void WriteToCSV(string outputFile)
		{
			using StreamWriter writer = new StreamWriter(outputFile);

			WriteTo(writer);


			writer.Close();
		}

	}
}
