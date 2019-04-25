using RabbitMQ.Client;
using System;
using System.Text;

namespace ImageGallery.IdentityServer.ServiceBus
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("IdentityServer Service Bus - Sender");

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

                //Our Message
                Console.WriteLine("Enter message to send");
                var msg = Console.ReadLine();
                var body = Encoding.UTF8.GetBytes(msg);

                //Publish to the exchange
                channel.BasicPublish(exchange: "logs",
                                     routingKey: "",
                                     basicProperties: null,
                                     body: body);

                Console.WriteLine(" [x] Sent {0}", msg);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();


            }
        }
    }
}
