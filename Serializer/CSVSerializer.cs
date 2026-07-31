using Serializer.Attributes;
using System.Reflection;
using System.Text;

namespace Serializer
{
    struct CsvProperty
    {
        public PropertyInfo Property;
        public string ColumnName;
        public int ColOrder;
    }
    internal static class CSVSerializer<T> where T : new()
    {
        // --------------------------- Caching logic -------------------------------------
        private static readonly Dictionary<Type, CsvProperty[]> _cache = new();

        private static CsvProperty[] GetProperties()
        {
            Type type = typeof(T);

            if (_cache.TryGetValue(type, out CsvProperty[]? cached))
            {
                return cached;
            }

            CsvProperty[] props = HandleAttributes(type.GetProperties());
            _cache[type] = props;
            return props;
        }
        // -------------------------------------------------------------------------------

        public static string WriteAll(List<T> data)
        {
            var props = GetProperties();

            StringBuilder sb = new();

            for (int i = 0; i < props.Length; i++)
            {
                sb.Append(props[i].ColumnName);
                if (i < props.Length - 1)
                    sb.Append(',');
            }

            sb.Append('\n');

            foreach (T item in data)
            {
                for (int i = 0; i < props.Length; i++)
                {
                    var value = props[i].Property.GetValue(item);

                    if (value != null)
                    {
                        if (value.GetType() == typeof(string))
                            value = Quoting(value.ToString()!);

                        sb.Append(value);
                        if (i < props.Length - 1)
                            sb.Append(',');
                    }
                }
                sb.Append('\n');
            }

            return sb.ToString();
        }

        public static List<T> ReadAll(string data)
        {
            var props = GetProperties();
            var list = new List<T>();

            string[] lines = data.Split('\n');
            string[] headers = lines[0].Split(',');

            for (int r = 1; r < lines.Length; r++)
            {
                if (string.IsNullOrWhiteSpace(lines[r]))
                    continue;

                string[] fields = SplitCsvLine(lines[r]);
                T item = new();

                for (int col = 0; col < fields.Length; col++)
                {
                    // find the matching property by header name
                    for (int p = 0; p < props.Length; p++)
                    {
                        if (props[p].ColumnName == headers[col])
                        {
                            string text = fields[col];
                            Type type = props[p].Property.PropertyType;

                            object value;
                            if (type == typeof(bool))
                                value = bool.Parse(text);
                            else if (type == typeof(decimal))
                                value = decimal.Parse(text);
                            else
                                value = text; // string

                            props[p].Property.SetValue(item, value);
                            break;
                        }
                    }
                }

                list.Add(item);
            }

            return list;
        }


        // --------------------------------- Writing helpers -------------------------------------

        #region Write
        private static CsvProperty[] HandleAttributes(PropertyInfo[] props)
        {
            var nameProp = typeof(CsvColumnAttribute).GetProperty("Name");
            var orderProp = typeof(CsvColumnAttribute).GetProperty("Order");
            var result = new List<CsvProperty>();

            foreach (PropertyInfo prop in props)
            {
                var ignoreAttr = prop.GetCustomAttribute(typeof(CsvIgnoreAttribute));

                if (ignoreAttr == null)
                {
                    var csvColAttr = prop.GetCustomAttribute(typeof(CsvColumnAttribute));

                    if (csvColAttr != null)
                    {
                        string columnName = (string)nameProp!.GetValue(csvColAttr)!;
                        int order = (int)orderProp!.GetValue(csvColAttr)!;

                        result.Add(new CsvProperty
                        {
                            Property = prop,
                            ColumnName = string.IsNullOrEmpty(columnName) ? prop.Name : columnName,
                            ColOrder = order
                        });
                    }
                    else
                    {
                        result.Add(new CsvProperty
                        {
                            Property = prop,
                            ColumnName = prop.Name,
                            ColOrder = int.MaxValue
                        });
                    }
                }
            }

            var array = result.ToArray();

            Array.Sort(array, (a, b) => a.ColOrder.CompareTo(b.ColOrder));

            return array;
        }

        private static string Quoting(string field)
        {
            bool needsQuoting = field.Contains(',')
            || field.Contains('"')
            || field.Contains('\n')
            || field.Contains('\r');

            if (!needsQuoting)
            {
                return field;
            }
            string escaped = field.Replace("\"", "\"\"");
            return "\"" + escaped + "\"";
        }
        #endregion Write

        // --------------------------------- Reading helpers -------------------------------------

        #region Read

        private static string[] SplitCsvLine(string line)
        {
            var fields = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            fields.Add(current.ToString());

            return fields.ToArray();
        }

        #endregion Read

    }
}
