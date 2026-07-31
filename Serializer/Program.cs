namespace Serializer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var products = new List<ProductRow>();

            ProductRow p1 = new ProductRow
            {
                Sku = "TEA-1",
                Name = "Armenian Tea",
                Price = 4.50m,
                InStock = true,
                WarehouseCode = "1123"
            };

            ProductRow p2 = new ProductRow
            {
                Sku = "COF-2",
                Name = "'Coffee, Premium'",
                Price = 9.99m,
                InStock = false,
                WarehouseCode = "1123"

            };

            products.Add(p1);
            products.Add(p2);

            string result = CSVSerializer<ProductRow>.WriteAll(products);


            using (var writer = new StreamWriter("products.csv"))
            {
                writer.Write(result);
            }

            string csvText = File.ReadAllText("products.csv");
            List<ProductRow> loaded = CSVSerializer<ProductRow>.ReadAll(csvText);

            foreach (var product in loaded)
            {
                Console.WriteLine(
                    product.Sku + " " +
                    product.Name + " " +
                    product.Price + " " +
                    product.InStock + " " +
                    product.WarehouseCode
                    );
            }
        }
    }
}
