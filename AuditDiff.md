# AuditDiffReflection

Compares two snapshots of the same object — **before** and **after** an edit —
and lists what changed, so it can be written into an audit log.

```csharp
Order.Customer: Anna -> Anna Smith
Order.Total.Amount: 40 -> 49.90
Order.Lines[0].Quantity: 2 -> 5
Order.Tags[2]: (missing) -> sale
```


## How it works

`AuditDiffer<T>.Diff(before, after)` returns a `List<AuditChange>`
(`Path`, `OldValue`, `NewValue`).

It reads the object's public properties with **reflection** (asking a type at
runtime what it contains) and sorts each one into four buckets:

| Category | Example | What happens |
|---|---|---|
| Scalar | `string`, `int`, `decimal`, `Guid` | compare values directly |
| Nested object | `Money Total` | walk inside → `Total.Amount` |
| Simple collection | `List<string> Tags` | compare item by item → `Tags[1]` |
| Complex collection | `List<OrderLine> Lines` | walk each item → `Lines[0].Quantity` |

- Lists are matched **by index**. A missing side shows as `(missing)`.
- Property lists are **cached per type**, so reflection runs once per type.
- A `HashSet` of visited instances prevents infinite loops on cycles
  (object A pointing back at object B pointing at A).

## Attributes

```csharp
[AuditIgnore] // skip this property entirely
public byte[]? RowVersion { get; set; }
[AuditName("Customer")] // use this name in the path instead
public string CustomerName { get; set; } = "";
```

## Demo

`Program.cs` runs four scenarios: the main example, a removed order line,
a currency-only change, and a no-change control.

