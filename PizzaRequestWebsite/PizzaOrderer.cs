using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PizzaRequestWebsite
{
    class PizzaOrderer
    {
        const int NumPizzasToOrder = 1;
        static readonly string[] AvailableToppings = { "pepperoni", "mushrooms", "pineapple", "pineapple", "anchovies", "jalapenos", "sausage", "green peppers" };
        readonly Random random = new Random();
        static readonly List<string> CorrelationIds = new List<string>();

        class PizzaRequested
        {
            public string CorrelationId { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string[] Toppings { get; set; }
            public string Coupon { get; set; }
        }

        class CouponIssued
        {
            public string CorrelationId { get; set; }
            public string Coupon { get; set; }
        }

        public int OrderPizzas(IModel channel)
        {
            int numPizzas = 0;
            for (int i = 0; i < NumPizzasToOrder; i++)
            {
                OrderPizza(channel, PizzaRequestWithRandomToppings());
                numPizzas++;
            }
            return numPizzas;
        }

        public int OrderFreePizza(IModel channel, string couponQueueName)
        {
            var consumer = new QueueingBasicConsumer(channel);
            channel.BasicConsume(couponQueueName, true, consumer);

            while (true)
            {
                var body = consumer.Queue.Dequeue().Body;
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine(" [x] Received {0}", message);

                var couponIssued = JsonConvert.DeserializeObject<CouponIssued>(message);
                if (!CorrelationIds.Contains(couponIssued.CorrelationId)) 
                    continue;

                OrderPizza(channel, PizzaRequestWithRandomToppings(couponIssued.Coupon));
            }            
        }

        PizzaRequested PizzaRequestWithRandomToppings(string coupon = "")
        {
            return new PizzaRequested
                   {
                       CorrelationId = Guid.NewGuid().ToString(),
                       Name = "Kay",
                       Address = "301 Ashton Blvd",
                       Toppings = new[] {RandomTopping(), RandomTopping(), RandomTopping()},
                       Coupon = coupon
                   };
        }

        static void OrderPizza(IModel channel, PizzaRequested pizzaRequested)
        {
            CorrelationIds.Add(pizzaRequested.CorrelationId);
            var json = JsonConvert.SerializeObject(pizzaRequested);
            Console.Out.WriteLine(json);
            channel.BasicPublish("pizzarequested.v1", "", null, Encoding.UTF8.GetBytes(json));
        }

        string RandomTopping()
        {
            return AvailableToppings[random.Next(AvailableToppings.Length)];
        }
    }
}