using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pivot
{
	public class PivotLINQ: IPivotAlgorithm
	{
		private const char separator = ',';
		DataTable pivotDatatable = new DataTable();
		
		private string dateFormat;
		public PivotLINQ(string dtFormat)
		{
			dateFormat = dtFormat;
		}


		public void Initialize(List<ForwardQuote> quotes)
		{
			Prepare(quotes);
		}
		/// <summary>
		/// Prepares pivot data table
		/// </summary>
		private void Prepare(List<ForwardQuote> quotes)
		{
			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn("ObservationDate", typeof(DateTime)));

			// find out list of columns (by From date) and a label which is most used for specified from-to range
			// label can contain error - to reduce chance that we pick up mistake label - choose the one with maxium number of occurences within column
			var columns = quotes.GroupBy(p => p.From).Select(columnGroup => new
			{
				Key = columnGroup.Key,
				Label = columnGroup.GroupBy(p => p.Label).OrderByDescending(gp => gp.Count()).Select(p => p.Key).FirstOrDefault() 
			}).OrderBy(p=>p.Key);

			// fill DataTable columns list 
			foreach (var column in columns)
				table.Columns.Add(new DataColumn(column.Label));

			// query for rows and data
			var rows = quotes.GroupBy(p=>p.ObservationDate)
								 .Select(rowGroup => new
								 {
									 Key = rowGroup.Key,
									 Values = columns.GroupJoin(
												 rowGroup,
												 c => c.Key,
												 r => r.From,
												 (c, columnGroup) => columnGroup.FirstOrDefault()?.Price)
								 });
			foreach (var row in rows)
			{
				var dataRow = table.NewRow();
				var items = row.Values.Cast<object>().ToList();
				items.Insert(0, row.Key);
				dataRow.ItemArray = items.ToArray();
				table.Rows.Add(dataRow);
			}
			pivotDatatable = table;
		}

		public IEnumerable<string> GeneratePivot()
		{
			StringBuilder line = new StringBuilder();
			// output header
			foreach (DataColumn column  in pivotDatatable.Columns)
			{
				// skip 1st column caption to match example results file
				line.Append(column.Caption == "ObservationDate" ? "" : column.Caption ).Append(separator);
			}
			// remove last separator
			line.Length = line.Length - 1;
			yield return line.ToString();
			// output data rows
			foreach (DataRow row in pivotDatatable.Rows)
			{
				line.Length = 0;
				foreach (DataColumn column in pivotDatatable.Columns)
				{
					var obj = row[column];
					var strValue = obj is DateTime dt ? dt.ToString(dateFormat) : obj.ToString();
					line.Append(strValue).Append(separator);
				}
				line.Length = line.Length - 1;
				yield return line.ToString();
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
