using System;
using Confluent.Kafka;
using System.Threading.Tasks;
using DailyRunEquity;

namespace Git_Sandbox
{
    class Program
    {
        public static void Main(string[] args)
        {
            //var _pro = new Producer();
            //var _con = new Consumer();
            //string statment = string.Empty; 
            //do {
            //        statment = Console.ReadLine();                        
            //        Console.WriteLine("Sending message for process:" + statment);
            //        Task mytsk =  _pro.Process(statment);             
            //         //var s = await _pro.Process(statment);
            //        statment = string.Empty;

            //} while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            // while(true)
            // {
            //    var s= Console.ReadLine();
            //    if (s!= null)
            //    {
            //         await _pro.Process(); 
            //    }
            //    _con.Process();
            // }

            // var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

            // using (var p = new Producer<Null, string>(config))
            // {
            //     try
            //     {
            //         var dr = await p.ProduceAsync("test-topic", new Message<Null, string> { Value="test" });
            //         Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            //     }
            //     catch (KafkaException e)
            //     {
            //         Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            //     }
            // }

            //Get all the latest NAV for shares
            Eqhelper obj = new Eqhelper();
            //obj.UpdateShareCurrentPrice();

            // Add dividend details that are updated in bse in last 90 days
            //obj.AddDividendDetails();

            //Update Monthly asset details

            obj.UpdateMonthlyAsset(9,2020);

            //obj.ReadNewExcel();

            Procurement objPro = new Procurement();
            var result = objPro.ShowProcurementInfo();
            Console.Write("DO YOU SEE ANY PROPERTY?");
            Console.ReadKey();
        }
    }
}
