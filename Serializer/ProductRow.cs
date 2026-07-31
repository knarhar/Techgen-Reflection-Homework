using Serializer.Attributes;

namespace Serializer
{   // csv ignore, csv member (name, order)
    public sealed class ProductRow
    {
        [CsvColumn("SKU", Order = 1)]
        public string Sku { get; set; } = "";

        [CsvColumn("Product Name", Order = 2)]
        public string Name { get; set; } = "";

        [CsvColumn("Unit Price", Order = 3)]
        public decimal Price { get; set; }

        [CsvColumn("In Stock", Order = 4)]
        public bool InStock { get; set; }

        // Internal bookkeeping — never leaves the system
        [CsvIgnore]
        public string? WarehouseCode { get; set; }
    }
}
