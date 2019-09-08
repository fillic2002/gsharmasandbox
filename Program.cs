using System;
using Confluent.Kafka;
using System.Threading.Tasks;

namespace Git_Sandbox
{
    class Program
    {
        public static void Main(string[] args)
        {
            var _pro = new Producer();
            var _con = new Consumer();
            string statment = string.Empty; 
            do {
                    statment = Console.ReadLine();                        
                    Console.WriteLine("Sending message for process:" + statment);
                    Task mytsk =  _pro.Process(statment);             
                     //var s = await _pro.Process(statment);
                    statment = string.Empty;
              
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);

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
        }
    }
}
