using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace ImageGallery.API.ServiceBus
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("API Service Bus - Receiver");

            var factory = new ConnectionFactory()
            {
                HostName = "192.168.99.100",
                Port = Protocols.DefaultProtocol.DefaultPort,
                UserName = "guest",
                Password = "guest",
                ContinuationTimeout = new TimeSpan(10, 0, 0, 0)
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                //Declare the exchange
                channel.ExchangeDeclare(exchange: "logs", type: "fanout");

                //Declare the queue
                var queueName = channel.QueueDeclare().QueueName;

                //Declare the binding
                channel.QueueBind(queue: queueName,
                    exchange: "logs",
                    routingKey: "");

                Console.WriteLine(" [*] Waiting for logs.");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" [x] {0}", message);
                };
                channel.BasicConsume(queue: queueName,
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
