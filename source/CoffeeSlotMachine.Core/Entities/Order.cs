using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace CoffeeSlotMachine.Core.Entities
{
    /// <summary>
    /// Bestellung verwaltet das bestellte Produkt, die eingeworfenen Münzen und
    /// die Münzen die zurückgegeben werden.
    /// </summary>
    public class Order : EntityObject
    {
        /// <summary>
        /// Datum und Uhrzeit der Bestellung
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Werte der eingeworfenen Münzen als Text. Die einzelnen 
        /// Münzwerte sind durch ; getrennt (z.B. "10;20;10;50")
        /// </summary>
        public String ThrownInCoinValues { get; set; }

        /// <summary>
        /// Zurückgegebene Münzwerte mit ; getrennt
        /// </summary>
        public String ReturnCoinValues { get; set; }

        /// <summary>
        /// Summe der eingeworfenen Cents.
        /// </summary>
        public int ThrownInCents => ThrownInCoinValues.Split(";").Sum(c => Convert.ToInt32(c));

        /// <summary>
        /// Summe der Cents die zurückgegeben werden
        /// </summary>
        public int ReturnCents => ReturnCoinValues.Split(";").Sum(c => Convert.ToInt32(c));


        public int ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        /// <summary>
        /// Kann der Automat mangels Kleingeld nicht
        /// mehr herausgeben, wird der Rest als Spende verbucht
        /// </summary>
        public int DonationCents => ThrownInCents - ReturnCents - Product.PriceInCents;

        /// <summary>
        /// Münze wird eingenommen.
        /// </summary>
        /// <param name="coinValue"></param>
        /// <returns>isFinished ist true, wenn der Produktpreis zumindest erreicht wurde</returns>
        public bool InsertCoin(int coinValue)
        {
            if (ThrownInCoinValues == null)
            {
                ThrownInCoinValues += coinValue;
            }
            else
            {
                ThrownInCoinValues += ";" + coinValue;
            }
            if (Product.PriceInCents <= ThrownInCents)
            {
                return true;

            }

            return false;
        }

        /// <summary>
        /// Übernahme des Einwurfs in das Münzdepot.
        /// Rückgabe des Retourgeldes aus der Kasse. Staffelung des Retourgeldes
        /// hängt vom Inhalt der Kasse ab.
        /// </summary>
        /// <param name="coins">Aktueller Zustand des Münzdepots</param>
        public void FinishPayment(IEnumerable<Coin> coins)
        {
            foreach (var v in ThrownInCoinValues
                                   .Split(";")
                                   .Select(s => Convert.ToInt32(s))
                                   .GroupBy(s => s))
            {
                coins
                    .First(c => c.CoinValue == v.Key)
                    .Amount += v.Count();
            }
            coins = coins.OrderByDescending(c => c.CoinValue);
            int sum = ThrownInCents - Product.PriceInCents;
            foreach (Coin c in coins)
            {
                for (int i = 0; i < c.Amount && sum != 0; i++)
                {
                    if (c.CoinValue <= sum)
                    {
                        ReturnCoinValues += c.CoinValue + ";";
                        sum -= c.CoinValue;
                        c.Amount--;
                    }
                }
            }

            if (ReturnCoinValues != null)
            {
                ReturnCoinValues = ReturnCoinValues.Substring(0, ReturnCoinValues.Length - 1);
            }
            else
            {
                ReturnCoinValues = "0";
            }
        }
    }
}
