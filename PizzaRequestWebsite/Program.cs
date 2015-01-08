using System;
using RabbitMQ.Client;

namespace PizzaRequestWebsite
{
    class Program
    {
        static void Main()
        {
            var factory = new ConnectionFactory {Uri = "amqp://craftsman:utahsc2015@54.149.117.183"};
            using (var conn = factory.CreateConnection())
            {
                Console.Out.WriteLine("Connected to Rabbit");
                using (var channel = conn.CreateModel())
                {
                    Console.Out.WriteLine("Opened a channel");

                    var couponQueueName = channel.QueueDeclare().QueueName;
                    channel.QueueBind(couponQueueName, "couponissued.v1", "");

                    Console.Out.WriteLine("Created queue {0} bound to couponissued.v1", couponQueueName);

                    var pizzaOrderer = new PizzaOrderer();
                    var numPizzasOrdered = pizzaOrderer.OrderPizzas(channel);
                    Console.Out.WriteLine("Ordered {0} pizzas", numPizzasOrdered);

                    Console.WriteLine(" [*] Waiting for messages. To exit press CTRL+C");
                    pizzaOrderer.OrderFreePizza(channel, couponQueueName);
                }
            }

            Console.Out.WriteLine("Hit Enter to exit");
            Console.In.ReadLine();
        }
    }
}