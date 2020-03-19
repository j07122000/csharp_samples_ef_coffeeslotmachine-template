using CoffeeSlotMachine.Core.Logic;
using CoffeeSlotMachine.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace CoffeeSlotMachine.ControllerTest
{
    [TestClass]
    public class ControllerTest
    {
        [TestInitialize]
        public void MyTestInitialize()
        {
            using (ApplicationDbContext applicationDbContext = new ApplicationDbContext())
            {
                applicationDbContext.Database.EnsureDeleted();
                applicationDbContext.Database.Migrate();
            }
        }


        [TestMethod]
        public void T01_GetCoinDepot_CoinTypesCount_ShouldReturn6Types_3perType_SumIs1155Cents()
        {
            using (OrderController controller = new OrderController())
            {
                var depot = controller.GetCoinDepot().ToArray();
                Assert.AreEqual(6, depot.Count(), "Sechs Münzarten im Depot");
                foreach (var coin in depot)
                {
                    Assert.AreEqual(3, coin.Amount, "Je Münzart sind drei Stück im Depot");
                }

                int sumOfCents = depot.Sum(coin => coin.CoinValue * coin.Amount);
                Assert.AreEqual(1155, sumOfCents, "Beim Start sind 1155 Cents im Depot");
            }
        }

        [TestMethod]
        public void T02_GetProducts_9Products_FromCappuccinoToRistretto()
        {
            using (OrderController statisticsController = new OrderController())
            {
                var products = statisticsController.GetProducts().ToArray();
                Assert.AreEqual(9, products.Length, "Neun Produkte wurden erzeugt");
                Assert.AreEqual("Cappuccino", products[0].Name);
                Assert.AreEqual("Ristretto", products[8].Name);
            }
        }

        [TestMethod]
        public void T03_BuyOneCoffee_OneCoinIsEnough_CheckCoinsAndOrders()
        {
            using (OrderController controller = new OrderController())
            {
                var products = controller.GetProducts();
                var product = products.Single(p => p.Name == "Cappuccino");
                var order = controller.OrderCoffee(product);
                bool isFinished = controller.InsertCoin(order, 100);
                Assert.AreEqual(true, isFinished, "100 Cent genügen");
                Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
                Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
                Assert.AreEqual(0, order.DonationCents);
                Assert.AreEqual("20;10;5", order.ReturnCoinValues);

                // Depot überprüfen
                var coins = controller.GetCoinDepot().ToArray();
                int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
                Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
                Assert.AreEqual("3*200 + 4*100 + 3*50 + 2*20 + 2*10 + 2*5", controller.GetCoinDepotString());

                var orders = controller.GetAllOrdersWithProduct().ToArray();
                Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
                Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
                Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
                Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
            }
        }

        [TestMethod]
        public void T04_BuyOneCoffee_ExactThrowInOneCoin_CheckCoinsAndOrders()
        {
       
            OrderController controller = new OrderController();
            var products = controller.GetProducts();
            var product = products.Single(p => p.Name == "Espresso");
            var order = controller.OrderCoffee(product);
            bool isFinished = controller.InsertCoin(order, 50);
            Assert.AreEqual(true, isFinished, "50 Cent genügen");
            Assert.AreEqual(50, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(50 - product.PriceInCents, order.ReturnCents);
            Assert.AreEqual(0, order.DonationCents);
            Assert.AreEqual("0", order.ReturnCoinValues);
           
            var coins = controller.GetCoinDepot().ToArray();
            int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
            Assert.AreEqual(1205, sumOfCents, "Beim Start sind 1155 Cents + 50 Cents für Espresso");
            Assert.AreEqual("3*200 + 3*100 + 4*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());
            var orders = controller.GetAllOrdersWithProduct().ToArray();
            Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
            Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
            Assert.AreEqual(50, orders[0].ThrownInCents, "50 Cents wurden eingeworfen");
            Assert.AreEqual("Espresso", orders[0].Product.Name, "Produktname Espresso");
        }

        [TestMethod]
        public void T05_BuyOneCoffee_MoreCoins_CheckCoinsAndOrders()
        {
          
            OrderController controller = new OrderController();
            var products = controller.GetProducts();
            var product = products.Single(p => p.Name == "Cappuccino");
            var order = controller.OrderCoffee(product);
            bool isNotFinished = controller.InsertCoin(order, 50);
            Assert.AreEqual(false, isNotFinished, "50 Cent genügen nicht");
            controller.InsertCoin(order, 10);
            bool isFinished = controller.InsertCoin(order, 5);
            Assert.AreEqual(true, isFinished, "65 Cent genügen");
            Assert.AreEqual(65, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(65 - product.PriceInCents, order.ReturnCents);
            Assert.AreEqual(0, order.DonationCents);
            Assert.AreEqual("0", order.ReturnCoinValues);
            // Depot überprüfen
            var coins = controller.GetCoinDepot().ToArray();
            int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
            Assert.AreEqual(1220, sumOfCents, "Beim Start sind 1155 Cents + 65 Cents für Cappuccino");
            Assert.AreEqual("3*200 + 3*100 + 4*50 + 3*20 + 4*10 + 4*5", controller.GetCoinDepotString());
            var orders = controller.GetAllOrdersWithProduct().ToArray();
            Assert.AreEqual(1, orders.Length, "Es ist genau eine Bestellung");
            Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
            Assert.AreEqual(65, orders[0].ThrownInCents, "65 Cents wurden eingeworfen");
            Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
        }
        [TestMethod()]
        public void T06_BuyMoreCoffees_OneCoins_CheckCoinsAndOrders()
        {
         
            OrderController controller = new OrderController();
            var products = controller.GetProducts();
            var product = products.Single(p => p.Name == "Espresso");
            var order = controller.OrderCoffee(product);
            var order2 = controller.OrderCoffee(product);


            bool isFinished = controller.InsertCoin(order, 100);
            Assert.AreEqual(true, isFinished, "100 Cent genügen");
            Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
            Assert.AreEqual(0, order.DonationCents);
            Assert.AreEqual("50", order.ReturnCoinValues);

            bool isFinished2 = controller.InsertCoin(order2, 50);
            Assert.AreEqual(true, isFinished2, "50 Cent genügen");
            Assert.AreEqual(50, order2.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(50 - product.PriceInCents, order2.ReturnCents);
            Assert.AreEqual(0, order2.DonationCents);
            Assert.AreEqual("0", order2.ReturnCoinValues);
            // Depot überprüfen
            var coins = controller.GetCoinDepot().ToArray();
            int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
            Assert.AreEqual(1255, sumOfCents, "Beim Start sind 1155 Cents + 100 Cents für 2 Espresso");
            Assert.AreEqual("3*200 + 4*100 + 3*50 + 3*20 + 3*10 + 3*5", controller.GetCoinDepotString());
            var orders = controller.GetAllOrdersWithProduct().ToArray();
            Assert.AreEqual(2, orders.Length, "Es sind genau zwei Bestellungen");
            Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
            Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
            Assert.AreEqual("Espresso", orders[0].Product.Name, "Produktname Espresso");

            Assert.AreEqual(0, orders[1].DonationCents, "Keine Spende");
            Assert.AreEqual(50, orders[1].ThrownInCents, "50 Cents wurden eingeworfen");
            Assert.AreEqual("Espresso", orders[1].Product.Name, "Produktname Espresso");
        }


        [TestMethod()]
        public void T07_BuyMoreCoffees_UntilDonation_CheckCoinsAndOrders()
        {
          
            OrderController controller = new OrderController();
            var products = controller.GetProducts();
            var product = products.Single(p => p.Name == "Cappuccino");
            var order = controller.OrderCoffee(product);
            bool isFinished = controller.InsertCoin(order, 100);
            Assert.AreEqual(true, isFinished, "100 Cent genügen");
            Assert.AreEqual(100, order.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(100 - product.PriceInCents, order.ReturnCents);
            Assert.AreEqual(0, order.DonationCents);
            Assert.AreEqual("20;10;5", order.ReturnCoinValues);

            var order2 = controller.OrderCoffee(product);
            bool isFinished2 = controller.InsertCoin(order2, 100);
            Assert.AreEqual(true, isFinished2, "100 Cent genügen");
            Assert.AreEqual(100, order2.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(100 - product.PriceInCents, order2.ReturnCents);
            Assert.AreEqual(0, order2.DonationCents);
            Assert.AreEqual("20;10;5", order2.ReturnCoinValues);

            var order3 = controller.OrderCoffee(product);
            bool isFinished3 = controller.InsertCoin(order3, 100);
            Assert.AreEqual(true, isFinished3, "100 Cent genügen");
            Assert.AreEqual(100, order3.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(100 - product.PriceInCents, order3.ReturnCents);
            Assert.AreEqual(0, order3.DonationCents);
            Assert.AreEqual("20;10;5", order3.ReturnCoinValues);

            var order4 = controller.OrderCoffee(product);
            bool isFinished4 = controller.InsertCoin(order4, 100);
            Assert.AreEqual(true, isFinished4, "100 Cent genügen");
            Assert.AreEqual(100, order4.ThrownInCents, "Einwurf stimmt nicht");
            Assert.AreEqual(0, order4.ReturnCents);
            Assert.AreEqual(35, order4.DonationCents);
            Assert.AreEqual("0", order4.ReturnCoinValues);

            var coins = controller.GetCoinDepot().ToArray();
            int sumOfCents = coins.Sum(c => c.CoinValue * c.Amount);
            Assert.AreEqual(1450, sumOfCents, "Beim Start sind 1155 Cents + 3*65 Cents und 1*100 Cents für Cappuccino");
            Assert.AreEqual("3*200 + 7*100 + 3*50 + 0*20 + 0*10 + 0*5", controller.GetCoinDepotString());
            var orders = controller.GetAllOrdersWithProduct().ToArray();

            Assert.AreEqual(4, orders.Length, "Es sind genau vier Bestellungen");
            Assert.AreEqual(0, orders[0].DonationCents, "Keine Spende");
            Assert.AreEqual(0, orders[1].DonationCents, "Keeine Spende");
            Assert.AreEqual(0, orders[2].DonationCents, "Keine Spende");
            Assert.AreEqual(35, orders[3].DonationCents, "35 Cent Spende");

            Assert.AreEqual(100, orders[0].ThrownInCents, "100 Cents wurden eingeworfen");
            Assert.AreEqual(100, orders[1].ThrownInCents, "100 Cents wurden eingeworfen");
            Assert.AreEqual(100, orders[2].ThrownInCents, "100 Cents wurden eingeworfen");
            Assert.AreEqual(100, orders[3].ThrownInCents, "100 Cents wurden eingeworfen");

            Assert.AreEqual("Cappuccino", orders[0].Product.Name, "Produktname Cappuccino");
            Assert.AreEqual("Cappuccino", orders[1].Product.Name, "Produktname Cappuccino");
            Assert.AreEqual("Cappuccino", orders[2].Product.Name, "Produktname Cappuccino");
            Assert.AreEqual("Cappuccino", orders[3].Product.Name, "Produktname Cappuccino");
        }

    }
}
