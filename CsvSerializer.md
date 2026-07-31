# CSV Serializer

A lightweight, reflection-based CSV serializer for C# that converts between `List<T>` and CSV text using attributes to control column names, order, and ignored properties.

## Features

- Write a `List<T>` to CSV text
- Read CSV text back into a `List<T>`
- Custom column names and ordering via attributes
- Skip properties from serialization
- Proper CSV quoting (handles commas, quotes, and newlines in field values)
- Reflection results cached per type for performance

## Usage

### Define your model

```csharp
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

    [CsvIgnore]
    public string? WarehouseCode { get; set; }
}
```

### Write CSV

```csharp
List<ProductRow> products = new() { p1, p2 };
string csv = CSVSerializer<ProductRow>.WriteAll(products);
File.WriteAllText("products.csv", csv);
```

### Read CSV

```csharp
string csvText = File.ReadAllText("products.csv");
List<ProductRow> loaded = CSVSerializer<ProductRow>.ReadAll(csvText);
```

## Attributes

| Attribute | Purpose |
|---|---|
| `[CsvColumn(name, Order = n)]` | Sets the CSV header name and column position |
| `[CsvIgnore]` | Excludes the property from serialization |

## Notes

- Field values containing commas, quotes, or newlines are automatically quoted/escaped on write, and correctly parsed back on read.
- Properties without `[CsvColumn]` use the property name as the header and are placed after ordered columns.
- Column-to-property mapping on read is done by matching header text, so column order in the file doesn't need to match the model.