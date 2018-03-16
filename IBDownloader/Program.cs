using System;
using System.Threading;

namespace IBDownloader
{
    class Program
    {
		

		static void Main(string[] args)
        {
			var controller = new IBController();
			controller.Connect();

            Console.WriteLine("Hello World!");
			Console.ReadLine();
        }
	}
}
