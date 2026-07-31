namespace AuditDiffReflection.Attributes
{
    internal class AuditNameAttribute : Attribute
    {
        public string Name { get; set; }

        public AuditNameAttribute(string name)
        {
            Name = name;
        }
    }
}
