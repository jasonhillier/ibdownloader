using System;
using CsvHelper;
using System.IO;
using IBDownloader.messages;
using System.Collections.Generic;

namespace IBDownloader.Tasks
{
	public class ImportCsv : BaseTask
    {
		private Queue<string> _FileQueue = new Queue<string>();
		private bool _IsPrepared = false;

		public ImportCsv(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
        {
        }

        //Indicate that this task wants repeated execution until no data is left
		public override bool IsMulti => true;

		public override async System.Threading.Tasks.Task<TaskResultData> ExecuteMultiAsync(IBDTaskInstruction Instruction)
		{
			if (_IsPrepared)
			{
				return await ProcessFileAsync(Instruction);
			}         

			var filePathName = Instruction.GetParameter("FilePathName");
			if (Directory.Exists(filePathName))
			{
				//is dir
				var files = Directory.GetFiles(filePathName);

				//TODO: process zip files

				foreach(string fileName in files)
				{
					if (fileName.EndsWith("csv", StringComparison.InvariantCulture))
					    _FileQueue.Enqueue(fileName);
				}
			}
			else if (File.Exists(filePathName))
			{
				//is file
				_FileQueue.Enqueue(filePathName);
			}
			else
			{
				return TaskResultData.Failure(Instruction, "No files found in specified location!");
			}

			_IsPrepared = true;
			return await ProcessFileAsync(Instruction);
		}

		protected async System.Threading.Tasks.Task<TaskResultData> ProcessFileAsync(IBDTaskInstruction Instruction)
		{
			string currentFile;
			if (!_FileQueue.TryDequeue(out currentFile))
				return new TaskResultData(Instruction, false); //signal there is no data left

			IEnumerable<CboeCsvRecord> records;
			using (StreamReader fileStream = new StreamReader(File.OpenRead(currentFile)))
            {
				CsvReader reader = new CsvReader(fileStream);
				while (reader.CanRead())
				{
					records = reader.GetRecords<CboeCsvRecord>();
				}
            }
            await System.Threading.Tasks.Task.Delay(1);


            TaskResultData results = new TaskResultData(Instruction, false);
            return results;
		}

		//TODO: build library for these
        public interface CsvRecord
		{
			HistoricalDataMessage AsOptionQuote();
			IBApi.Contract AsOptionContract();
			HistoricalDataMessage AsUnderlyingQuote();
		}

		public class CboeCsvRecord : CsvRecord
		{
			public DateTime quote_datetime { get; set; }
			public DateTime expiration { get; set; }
			public string root { get; set; } //baseSymbol
			public double strike { get; set; }
			public string option_type { get; set; }
			public double bid { get; set; }
			public double ask { get; set; }
			public double underlying_bid { get; set; }
			public double underlying_ask { get; set; }

			public IBApi.Contract AsOptionContract()
			{
				return null;
			}

            public HistoricalDataMessage AsOptionQuote()
			{
				return new HistoricalDataMessage(0, new IBApi.Bar(
					this.quote_datetime.ToString(),
					this.bid,
					this.ask,
					this.bid,
					this.ask,
					0,
					0,
					0
				));
			}

			public HistoricalDataMessage AsUnderlyingQuote()
			{
				return new HistoricalDataMessage(0, new IBApi.Bar(
                    this.quote_datetime.ToString(),
					this.underlying_bid,
					this.underlying_ask,
					this.underlying_bid,
					this.underlying_ask,
                    0,
                    0,
                    0
                ));
			}
		}
    }
}
