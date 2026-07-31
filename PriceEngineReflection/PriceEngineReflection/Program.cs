using ACA.PriceEngine;

namespace PriceEngineReflection
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PriceEngineWrapper.PriceEngineWrapper pw = new PriceEngineWrapper.PriceEngineWrapper();
            PriceEngine pe = new PriceEngine();

            PriceInput pi = new PriceInput
            {
                Lines =
                    {
                        new BasketLine { Sku = "TEA", Quantity = 1, UnitPrice = 150m }
                    },
                LoyaltyTier = 2,
                CouponAmount = 1,
                VatRate = 10m
            };

            var price = pw.Calculate(pi);
            var price2 = pe.CalculatePayable(pi);

            Console.WriteLine(price);
            Console.WriteLine(price2);
        }
    }
}
