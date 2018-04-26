using System;
using CsvHelper;
using System.IO;
using IBDownloader.messages;
using System.Collections.Generic;
using System.IO.Compression;
using Newtonsoft.Json;
using IBDownloader.DataStorage;
using System.Dynamic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace IBDownloader.Tasks
{
	public class ImportGenericJSON : BaseTask
	{
		public ImportGenericJSON(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
		{
		}

		public override async System.Threading.Tasks.Task<TaskResultData> ExecuteAsync(IBDTaskInstruction Instruction)
		{
			List<string> files = new List<string>();
			string idField = Instruction.GetParameter("IDField");
			string dateField = Instruction.GetParameter("DateField");
			var filePathName = Instruction.GetParameter("FilePathName");

			if (Directory.Exists(filePathName))
			{
				//is dir
				var tmpFiles = Directory.GetFiles(filePathName);

				foreach (string fileName in tmpFiles)
				{
					if (fileName.EndsWith("json", StringComparison.InvariantCulture))
						files.Add(fileName);
				}

				files.Sort();
			}
			else if (File.Exists(filePathName))
			{
				//is file
				files.Add(filePathName);
			}
			else
			{
				return TaskResultData.Failure(Instruction, "No files found in specified location!");
			}

			this.Log("Reading {0} using id={1} and date={2}...", filePathName, idField, dateField);

			List<IDataRow> allRows = new List<IDataRow>();
			//var results = System.Threading.Tasks.Task.TaskResultData();
			foreach (string file in files)
			{
				allRows.AddRange(ProcessFile(file, idField, dateField));
				await System.Threading.Tasks.Task.Delay(1);
			}

			return new TaskResultData(Instruction, true, allRows);
		}

		protected List<IDataRow> ProcessFile(string CurrentFile, string IDFieldName, string DateFieldName)
		{
			ZipArchive zipFile = null;
			Stream stream;

			if (CurrentFile.EndsWith("zip", StringComparison.InvariantCulture))
			{
				//TODO: support more than 1 file per archive
				zipFile = ZipFile.OpenRead(CurrentFile);
				stream = zipFile.Entries[0].Open();
			}
			else
			{
				stream = File.OpenRead(CurrentFile);
			}

			this.Log("Processing {0}...", CurrentFile);

			var rows = new List<IDataRow>();

			using (stream)
			{
				StreamReader fileStream = new StreamReader(stream);
				string jsonData = fileStream.ReadToEnd();
				var data = JsonConvert.DeserializeObject<List<DataRow>>(jsonData);
				foreach(var row in data)
				{
					row.date = DateTime.Parse(row[DateFieldName].ToString());
					row.id = row[IDFieldName] + " " + row.date;

					rows.Add(row);
				}
			}
			if (zipFile != null)
			{
				zipFile.Dispose();
			}

			return rows;
		}

		class DataRow : Dynamitey.DynamicObjects.Dictionary, IDataRow
		{
			public string id
			{
				get { return this["id"].ToString(); }
				set { this["id"] = value; }
			}

			public DateTime date
			{
				get { return (DateTime)this["date"]; }
				set { this["date"] = value; }
			}
		}
	}
}
