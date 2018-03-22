using IBDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using Nest;
using Elasticsearch.Net;

namespace IBDownloader.DataStorage
{
    public class ElasticsearchStorage : BaseDataStorage
    {
		private Nest.ElasticClient _client;
		//requires a data processor to ensure unique row ids
		public ElasticsearchStorage(IDataProcessor DataProcessor, string Server = null, string Index = null, string Username = null, string Password = null)
			: base(DataProcessor)
		{
			this.Server = Framework.Settings.Get("ELASTICSEARCH_URL", Server);
			this.Index = Framework.Settings.Get("ELASTICSEARCH_INDEX", Index);
			this.Username = Framework.Settings.Get("ELASTICSEARCH_USER", Username);
			this.Password = Framework.Settings.Get("ELASTICSEARCH_PWD", Password);

			if (String.IsNullOrEmpty(this.Server))
				throw new Exception("No elasticsearch server defined!");
			if (String.IsNullOrEmpty(this.Index))
				throw new Exception("No elasticsearch index defined!");

			/*
			_client = new Nest.ElasticClient(new ConnectionSettings(
				new Uri(this.Server))
				.DefaultIndex(this.Index)
				.BasicAuthentication(this.Username, this.Password)
				.EnableHttpCompression()
				);
			*/
			this.Log("Elasticsearch URI: {0}", this.Server);
			this.Log("Elasticsearch Index: {0}", this.Index);
		}

		public string Server { get; set; }
		public string Index { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }

		protected override async Task ProcessQueue()
		{
			var rows = Dequeue();

			string dataPackage = Package(rows);
			var result = await BulkInsert(dataPackage);

			if (result)
				this.Log("Imported {0} rows.", rows.Count);
		}

		public async Task<bool> BulkInsert(string dataPackage)
		{
			if (String.IsNullOrEmpty(dataPackage))
				return false;

			this.Log("Sending " + dataPackage.Length + " bytes...");

			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Server + "/_bulk");

			if (!String.IsNullOrEmpty(this.Username))
			{
				String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(this.Username + ":" + this.Password));
				request.Headers.Add("Authorization", "Basic " + encoded);
			}
			request.Method = "PUT";
			request.AutomaticDecompression = DecompressionMethods.GZip;
			request.ContentType = "application/json";
			var writer = request.GetRequestStream();
			byte[] data = System.Text.Encoding.UTF8.GetBytes(dataPackage);
			writer.Write(data, 0, data.Length);

			var response = (HttpWebResponse)await request.GetResponseAsync();

			return (response.StatusCode == HttpStatusCode.OK);
		}

		/// <summary>
		/// Serialize computed option data into a data package (string).
		/// </summary>
		/// <returns>Size of data package</returns>
		public string Package(List<IDataRow> dataRows)
		{
			StringBuilder builder = new StringBuilder();

			dataRows.ForEach((c) =>
			{
				//elasticsearch document header
				builder.AppendLine(
					JsonConvert.SerializeObject(
						new
						{
							index = new
							{
								_index = this.Index,
								_type = "doc",
								_id = c.id
							}
						})
					);

				//elasticsearch document body
				builder.AppendLine(
					JsonConvert.SerializeObject(c)
						);
			});

			return builder.ToString();
		}

		public async Task<List<IDataRow>> FetchQuotes(string Symbol, DateTime Start, DateTime End)
		{
			await Task.Delay(1);
			return new List<IDataRow>();
		}
	}
}
