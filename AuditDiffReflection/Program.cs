using AuditDiffReflection.Attributes;

namespace AuditDiffReflection
{
    public sealed class Money
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public sealed class OrderLine
    {
        public string Sku { get; set; } = "";
        public int Quantity { get; set; }
    }

    public sealed class Order
    {
        public Guid Id { get; set; }

        [AuditName("Customer")]
        public string CustomerName { get; set; } = "";

        public string Status { get; set; } = "";
        public Money Total { get; set; } = new Money();
        public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
        public List<string> Tags { get; set; } = new List<string>();

        [AuditIgnore]
        public byte[]? RowVersion { get; set; }
    }

    internal class Program
    {
        // same id for all test cases, to prevent id regeneration
        private static readonly Guid DemoId = Guid.NewGuid();

        static Order MakeBaseOrder()
        {
            return new Order
            {
                Id = DemoId,
                CustomerName = "Anna",
                Status = "Pending",
                Total = new Money { Amount = 40m, Currency = "USD" },
                Lines = new List<OrderLine>
                {
                    new OrderLine { Sku = "TEA-1", Quantity = 2 }
                },
                Tags = new List<string> { "vip", "tea" },
                RowVersion = new byte[] { 1, 2, 3 }
            };
        }

        public static void PrintDiff(string label, List<AuditChange> diff)
        {
            Console.WriteLine($"--- {label} ---");

            if (diff.Count == 0)
            {
                Console.WriteLine("(no changes)");
            }

            foreach (var change in diff)
            {
                Console.WriteLine(
                    $"{change.Path}: {change.OldValue ?? "(missing)"} -> {change.NewValue ?? "(missing)"}");
            }

            Console.WriteLine();
        }

        static void Main()
        {
            MainScenario();
            RemovedLineScenario();
            CurrencyScenario();
            NoChangesScenario();
        }

        static void MainScenario()
        {
            var before = MakeBaseOrder();

            var after = MakeBaseOrder();
            after.CustomerName = "Anna Smith";
            after.Status = "Paid";
            after.Total = new Money { Amount = 49.90m, Currency = "USD" };
            after.Lines = new List<OrderLine>
            {
                new OrderLine { Sku = "TEA-1", Quantity = 5 }
            };
            after.Tags = new List<string> { "vip", "coffee", "sale" };
            after.RowVersion = new byte[] { 9, 9, 9 };

            PrintDiff("Main example", AuditDiffer<Order>.Diff(before, after));
        }

        static void RemovedLineScenario()
        {
            var before = MakeBaseOrder();
            before.Lines.Add(new OrderLine { Sku = "MILK-9", Quantity = 1 });
            var after = MakeBaseOrder();

            PrintDiff("Removed line", AuditDiffer<Order>.Diff(before, after));
        }

        static void CurrencyScenario()
        {
            var before = MakeBaseOrder();
            var after = MakeBaseOrder();
            after.Total.Currency = "EUR";

            PrintDiff("Currency changed", AuditDiffer<Order>.Diff(before, after));
        }

        static void NoChangesScenario()
        {
            PrintDiff("No changes", AuditDiffer<Order>.Diff(MakeBaseOrder(), MakeBaseOrder()));
        }
    }
}