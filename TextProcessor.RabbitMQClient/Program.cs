using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;

namespace TextProcessor.RabbitMQClient
{
    internal class Program
    {
        private static readonly string RABBITMQ_HOST = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
        private static readonly int RABBITMQ_PORT = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"));

        private static readonly string Queue = "TextProcessor.InputQueue";

        private static readonly TextProcessorService service = new TextProcessorService();

        private static void Main(string[] args)
        {
            for (var i = 0; i < 30; i++)
            {
                Console.WriteLine($"Try: {i}");

                try
                {
                    Console.WriteLine($"Trying to connect to '{RABBITMQ_HOST}:{RABBITMQ_PORT}'...");

                    var connFactory = new ConnectionFactory() { HostName = RABBITMQ_HOST, Port = RABBITMQ_PORT };
                    using (var conn = connFactory.CreateConnection())
                    {
                        using (var ch = conn.CreateModel())
                        {
                            Console.WriteLine("Connected");

                            Console.WriteLine($"Creating a queue named '{Queue}'...");
                            ch.QueueDeclare(Queue, false, false, false, null);

                            Console.WriteLine($"Waiting for messages...");

                            var consumer = new EventingBasicConsumer(ch);
                            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
                            {
                                Console.WriteLine($"[MSG] CorrelationId='{e.BasicProperties.CorrelationId}'; ReplyTo={e.BasicProperties.ReplyTo};");

                                var inputJson = Encoding.UTF8.GetString(e.Body.ToArray());

                                Console.WriteLine($"\n\nInput:\n{inputJson}");

                                var text = JsonConvert.DeserializeObject<string>(inputJson);
                                var props = ch.CreateBasicProperties();
                                props.CorrelationId = e.BasicProperties.CorrelationId;

                                var outputJson = JsonConvert.SerializeObject(service.Split(text));

                                Console.WriteLine($"\n\nOutput:\n{outputJson}");

                                Console.WriteLine($"Sending message back to '{e.BasicProperties.ReplyTo}'...");

                                ch.BasicPublish("", e.BasicProperties.ReplyTo, props, Encoding.UTF8.GetBytes(outputJson));
                                ch.BasicAck(e.DeliveryTag, false);
                            };

                            Console.WriteLine($"Listening {Queue}...");

                            ch.BasicConsume(Queue, false, consumer);

                            while (true) { }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);

                    Thread.Sleep(5000);
                }
            }
        }
    }
}