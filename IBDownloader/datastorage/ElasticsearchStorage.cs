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
		public ElasticsearchStorage(IDataProcessor DataProcessor)
			: base(DataProcessor)
		{

		}

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
