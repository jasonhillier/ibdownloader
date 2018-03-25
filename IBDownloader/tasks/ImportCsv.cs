using System;
using CsvHelper;
using System.IO;
using IBDownloader.messages;
using System.Collections.Generic;
using System.IO.Compression;

namespace IBDownloader.Tasks
{
	public class ImportCsv : BaseTask
    {
		public ImportCsv(IBDTaskHandler TaskHandler)
			: base(TaskHandler)
        {
        }

		// Indicate that this task will handle firing result callback
		public override bool IsMulti => true;

		public override async System.Threading.Tasks.Task ExecuteMultiAsync(IBDTaskInstruction Instruction, Action<TaskResultData> OnTaskResult)
		{
			List<string> files = new List<string>();

			var filePathName = Instruction.GetParameter("FilePathName");
			if (Directory.Exists(filePathName))
			{
				//is dir
				var tmpFiles = Directory.GetFiles(filePathName);

				foreach(string fileName in tmpFiles)
				{
					if (fileName.EndsWith("csv", StringComparison.InvariantCulture) ||
						fileName.EndsWith("zip", StringComparison.InvariantCulture))
						files.Add(fileName);
				}
			}
			else if (File.Exists(filePathName))
			{
				//is file
				files.Add(filePathName);
			}
			else
			{
				this.LogWarn("No files found in specified location!");
				return;
			}

			foreach (string file in files)
			{
				await ProcessFileAsync(Instruction, file, OnTaskResult);
			}
		}

		protected async System.Threading.Tasks.Task ProcessFileAsync(IBDTaskInstruction Instruction, string CurrentFile, Action<TaskResultData> OnTakResult)
		{
			IBApi.Contract underlyingContract = null;
			Dictionary<string, List<CboeCsvRecord>> fileOptionQuotes = new Dictionary<string, List<CboeCsvRecord>>();

			Stream stream;
			if (CurrentFile.EndsWith("zip", StringComparison.InvariantCulture))
			{
				//TODO: support more than 1 file per archive
				stream = ZipFile.OpenRead(CurrentFile).Entries[0].Open();
			}
			else
			{
				stream = File.OpenRead(CurrentFile);
			}

			this.Log("Processing {0}...", CurrentFile);

			using (StreamReader fileStream = new StreamReader(stream))
            {
				CsvReader reader = new CsvReader(fileStream);
				foreach(var record in reader.GetRecords<CboeCsvRecord>())
				{
					if (underlyingContract == null)
					{
						underlyingContract = record.AsUnderlyingContract();
					}
					else if (underlyingContract.Symbol != record.AsUnderlyingContract().Symbol)
					{
						throw new Exception("Task does not support data with multiple underlying contracts!");
					}

					string optionLocalSymbol = record.AsOptionContract().LocalSymbol;
					if (!fileOptionQuotes.ContainsKey(optionLocalSymbol))
						fileOptionQuotes[optionLocalSymbol] = new List<CboeCsvRecord>();

					fileOptionQuotes[optionLocalSymbol].Add(record);
				}
            }

			//grouped by option symbol/contract
			foreach (var optionQuoteSet in fileOptionQuotes)
			{
				var underlyingQuotes = new Dictionary<DateTime, HistoricalDataMessage>();
				var optionQuotes = new List<HistoricalDataMessage>();
				foreach(CboeCsvRecord quote in optionQuoteSet.Value)
				{
					optionQuotes.Add(quote.AsOptionQuote());

					var underlyingQuote = quote.AsUnderlyingQuote();
					underlyingQuotes[underlyingQuote.Date.ParseElse<DateTime>(DateTime.Now)] = underlyingQuote;
				}

				var ins = new IBDTaskInstruction() { contract = optionQuoteSet.Value[0].AsOptionContract(), parameters = Instruction.parameters.Clone() }; //clone parameters so we get same settings (barSize etc)
				ins.metadata[IBDMetadataType.underlying] = underlyingContract;
				ins.metadata[IBDMetadataType.underlyingData] = underlyingQuotes;

				var taskResult = new TaskResultData(ins, true, optionQuotes);

				//fire event with this task result
				OnTakResult(taskResult);

				//give storage a second to keep up
				await System.Threading.Tasks.Task.Delay(10);
			}
		}

		//TODO: build library for these
        public interface CsvRecord
		{
			IBApi.Contract AsOptionContract();
			IBApi.Contract AsUnderlyingContract();

			HistoricalDataMessage AsOptionQuote();
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
				return new IBApi.Contract() {
					Strike = this.strike,
					LastTradeDateOrContractMonth = this.expiration.ToString("yyyyMMdd"),
					Right = this.option_type,
					SecType = "OPT",
					LocalSymbol = String.Format(
					this.root + " {0}{1}{2:00000000}",
					this.expiration.ToString("yyMMdd"),
					this.option_type,
					this.strike*1000
				)};
			}

			public IBApi.Contract AsUnderlyingContract()
			{
				return new IBApi.Contract() { Symbol = this.root };
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
