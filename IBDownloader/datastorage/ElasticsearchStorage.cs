using IBDownloader.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IBDownloader.DataStorage
{
    class ElasticsearchStorage : BaseDataStorage
    {
		//requires a data processor to ensure unique row ids
		public ElasticsearchStorage(IDataProcessor DataProcessor, string Server = null, string Index = null)
			: base(DataProcessor)
		{
			this.Server = Framework.Settings.Get("ELASTICSEARCH_URL", Server);
			this.Index = Framework.Settings.Get("ELASTICSEARCH_INDEX", Index);

			if (String.IsNullOrEmpty(this.Server))
				throw new Exception("No elasticsearch server defined!");
			if (String.IsNullOrEmpty(this.Index))
				throw new Exception("No elasticsearch index defined!");

			this.Log("Elasticsearch URI: {0}", this.Server);
			this.Log("Elasticsearch Index: {0}", this.Index);
		}

		public string Server { get; private set; }
		public string Index { get; set; }

		/// <summary>
		/// Serialize computed option data into a data package (string).
		/// </summary>
		/// <returns>Size of data package</returns>
		/*
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
								_index = _Storage,
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
		*/
	}
}
