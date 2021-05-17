using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.WebApp.Core
{
    public sealed class MqClient
    {
        private const string ServiceExchange = "Client.ServiceExchange";
        private const string ReplyQueue = "Client.ReplyQueue";

        private IConnection _conn;
        private IModel _ch;
        private IDictionary<string, ReplySlot> _replies;

        public bool IsConnected { get; private set; } = false;

        public static MqClient Default { get; } = CreateDefault();

        private static MqClient CreateDefault()
        {
            var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST");
            var port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT"));
            var routes = new Dictionary<string, string>();
            foreach (var route in JObject.Parse(Environment.GetEnvironmentVariable("RABBITMQ_SERVICE_EXCHANGE_ROUTES")))
            {
                routes.Add(route.Key, (string)route.Value);
            }

            return new MqClient(host, port, routes);
        }

        private MqClient(string host, int port, IDictionary<string, string> routes)
        {
            for (var i = 0; i < 10; i++)
            {
                if (TryConnect(host, port, routes))
                {
                    IsConnected = true;
                    break;
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
        }

        private bool TryConnect(string host, int port, IDictionary<string, string> routes)
        {
            try
            {
                Console.WriteLine($"Trying to connect to '{host}:{port}'...");

                var connFactory = new ConnectionFactory()
                {
                    HostName = host,
                    Port = port
                };

                _conn = connFactory.CreateConnection();
                _ch = _conn.CreateModel();

                Console.WriteLine("Connected");

                Console.WriteLine($"Creating a exchange named '{ServiceExchange}'...");

                _ch.ExchangeDeclare(ServiceExchange, ExchangeType.Direct);

                Console.WriteLine($"Creating a queue named '{ReplyQueue}'...");

                _ch.QueueDeclare(ReplyQueue, false, false, false, null);

                if (routes != null)
                {
                    Console.WriteLine($"{routes.Count} routes to be binded...");

                    foreach (var route in routes)
                    {
                        Console.WriteLine($"Binging '{route.Key}' to '{route.Value}' for '{ServiceExchange}'");

                        _ch.QueueDeclare(route.Value, false, false, false, null);
                        _ch.QueueBind(route.Value, ServiceExchange, route.Key);
                    }
                }
                else
                {
                    Console.WriteLine("No routes");
                }

                _replies = new Dictionary<string, ReplySlot>(StringComparer.InvariantCultureIgnoreCase);

                var consumer = new EventingBasicConsumer(_ch);
                consumer.Received += (object sender, BasicDeliverEventArgs e) =>
                {
                    Console.WriteLine($"[MSG] CorrelationId='{e.BasicProperties.CorrelationId}';");

                    var correlationId = e.BasicProperties.CorrelationId;
                    lock (_replies)
                    {
                        if (_replies.TryGetValue(correlationId, out var reply) && !reply.Received)
                        {
                            var data = Encoding.UTF8.GetString(e.Body.ToArray());

                            Console.WriteLine($"\n\nData:\n{data}");

                            reply.Data = data;
                            reply.Received = true;

                            _ch.BasicAck(e.DeliveryTag, false);
                        }
                        else
                        {
                            Console.WriteLine("Message not recognized");
                        }
                    }
                };

                Console.WriteLine($"Listening '{ReplyQueue}'...");

                _ch.BasicConsume(ReplyQueue, false, consumer);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                return false;
            }
        }

        private string WaitForData(string correlationId, uint timeout = 600000)
        {
            var timer = Stopwatch.StartNew();
            while (true)
            {
                Thread.Sleep(1000);

                Console.WriteLine("Waiting...");

                lock (_replies)
                {
                    if (_replies.TryGetValue(correlationId, out var reply) && reply.Received)
                    {
                        Console.WriteLine("Return data");

                        return reply.Data;
                    }
                }

                if (timer.ElapsedMilliseconds > timeout)
                {
                    throw new TimeoutException($"CorrelationId='{correlationId}'; Timeout={timeout};");
                }
            }
        }

        public Task<string> CallAsync(string route, string data, uint timeout = 600000) =>
            Task.Factory.StartNew(() =>
            {
                var correlationId = Guid.NewGuid().ToString("D");

                var props = _ch.CreateBasicProperties();
                props.ReplyTo = ReplyQueue;
                props.CorrelationId = correlationId;

                Console.WriteLine($"Sending message to '{ServiceExchange}' with key '{route}' and '{correlationId}' CorrelationId...");

                lock(_replies)
                {
                    _replies.Add(correlationId, new ReplySlot(correlationId));
                }

                _ch.BasicPublish(
                    ServiceExchange,
                    route,
                    props,
                    Encoding.UTF8.GetBytes(data));

                return WaitForData(correlationId, timeout);
            });

        public void Destroy()
        {
            _ch.Close();
            _conn.Close();
            IsConnected = false;
        }

        private class ReplySlot
        {
            public string Id { get; set; }

            public string Data { get; set; } = string.Empty;

            public bool Received { get; set; } = false;

            public ReplySlot(string id) => Id = id;
        }
    }
}