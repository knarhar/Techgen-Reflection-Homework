namespace Serializer.Attributes
{   // csv ignore, csv member (name, order)
    internal class CsvColumnAttribute : Attribute
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public CsvColumnAttribute(string name)
        {
            Name = name;
        }
    }
}
