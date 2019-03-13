using RabbitMQ.Client;
using System;
using System.Text;

namespace ImageGallery.IdentityServer.ServiceBus
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IdentityServer - Sender");

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
                channel.QueueDeclare(queue: "msgKey",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                Console.WriteLine("Enter message to send");
                var msg = Console.ReadLine();
                var body = Encoding.UTF8.GetBytes(msg);

                channel.BasicPublish(exchange: "",
                                     routingKey: "msgKey",
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" [x] Sent {0}", msg);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
