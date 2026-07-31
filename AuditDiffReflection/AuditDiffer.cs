using AuditDiffReflection.Attributes;
using System.Reflection;

namespace AuditDiffReflection
{
    enum PropertyCategory
    {
        Scalar,
        NestedObject,
        SimpleCollection,
        ComplexCollection
    }

    struct AuditChange
    {
        public string Path;
        public object? OldValue;
        public object? NewValue;
    }

    struct AuditProperty
    {
        public PropertyInfo Property;
        public string Name;
    }

    internal static class AuditDiffer<T>
    {
        // -------- Cache ---------
        private static readonly Dictionary<Type, AuditProperty[]> _cache = new();

        private static AuditProperty[] GetProperties(Type type)
        {
            if (_cache.TryGetValue(type, out AuditProperty[]? cached))
            {
                return cached;
            }

            AuditProperty[] props = HandleAttributes(type.GetProperties());
            _cache[type] = props;
            return props;
        }


        // -------- Attributes ---------
        private static AuditProperty[] HandleAttributes(PropertyInfo[] props)
        {
            var propName = typeof(AuditNameAttribute).GetProperty("Name");
            var list = new List<AuditProperty>();

            foreach (PropertyInfo prop in props)
            {
                var ignoreAttr = prop.GetCustomAttribute(typeof(AuditIgnoreAttribute));
                if (ignoreAttr != null)
                {
                    continue;
                }

                var nameAttr = prop.GetCustomAttribute(typeof(AuditNameAttribute));

                string? name = null;
                if (nameAttr != null)
                {
                    name = (string)propName!.GetValue(nameAttr)!;
                }

                list.Add(new AuditProperty
                {
                    Name = string.IsNullOrWhiteSpace(name) ? prop.Name : name,
                    Property = prop
                });

            }

            return list.ToArray();
        }

        // -------- Type Classification ---------
        private static bool IsScalar(Type t)
        {
            if (t.IsPrimitive) return true;
            if (t == typeof(string)) return true;
            if (t == typeof(decimal)) return true;
            if (t == typeof(Guid)) return true;
            if (t == typeof(DateTime)) return true;
            return false;
        }

        private static PropertyCategory TypeClassifier(Type t)
        {
            if (IsScalar(t))
                return PropertyCategory.Scalar;

            if (t != typeof(string) && typeof(System.Collections.IEnumerable).IsAssignableFrom(t))
            {
                Type elementType = GetElementType(t);
                return IsScalar(elementType)
                    ? PropertyCategory.SimpleCollection
                    : PropertyCategory.ComplexCollection;
            }

            return PropertyCategory.NestedObject;
        }

        private static Type GetElementType(Type collectionType)
        {
            if (collectionType.IsGenericType)
            {
                return collectionType.GetGenericArguments()[0];
            }

            // fallback for non-generic collections (ArrayList, arrays, etc.)
            return typeof(object);
        }

        // -------------- Public Diff ----------------
        public static List<AuditChange> Diff(T before, T after)
        {
            var changes = new List<AuditChange>();
            var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);

            Walk(before, after, typeof(T), typeof(T).Name, changes, visited);

            return changes;
        }

        // -------------- Diff Helpers ---------------
        private static void Walk(
            object? before, 
            object? after, 
            Type type, 
            string path,
            List<AuditChange> changes, 
            HashSet<object> visited
            )
        {
            // if we've already walked this exact instance, stop
            if (before != null && visited.Contains(before))
                return;
            if (after != null && visited.Contains(after))
                return;

            if (before != null) visited.Add(before);
            if (after != null) visited.Add(after);

            AuditProperty[] props = GetProperties(type);

            foreach (AuditProperty prop in props)
            {
                object? oldValue = before != null ? prop.Property.GetValue(before) : null;
                object? newValue = after != null ? prop.Property.GetValue(after) : null;
                string newPath = path + "." + prop.Name;

                PropertyCategory category = TypeClassifier(prop.Property.PropertyType);

                switch (category)
                {
                    case PropertyCategory.Scalar:
                        CompareScalar(oldValue, newValue, newPath, changes);
                        break;

                    case PropertyCategory.NestedObject:
                        if (oldValue != null || newValue != null)
                        {
                            Walk(oldValue, 
                                newValue, 
                                prop.Property.PropertyType, 
                                newPath, 
                                changes, 
                                visited);
                        }
                        break;

                    case PropertyCategory.SimpleCollection:
                        CompareSimpleCollection(
                            (System.Collections.IEnumerable)oldValue!,
                            (System.Collections.IEnumerable)newValue!, 
                            newPath, changes);
                        break;

                    case PropertyCategory.ComplexCollection:
                        CompareComplexCollection(
                            (System.Collections.IEnumerable)oldValue!,
                            (System.Collections.IEnumerable)newValue!,
                            prop.Property.PropertyType, newPath, changes, visited);
                        break;
                }
            }
        }

        private static void CompareScalar(
            object? oldValue, 
            object? newValue, 
            string path, 
            List<AuditChange> changes
            )
        {
            if (!Equals(oldValue, newValue))
            {
                changes.Add(new AuditChange 
                { 
                    Path = path, 
                    OldValue = oldValue, 
                    NewValue = newValue 
                });
            }
        }

        private static void CompareSimpleCollection(
            System.Collections.IEnumerable? before, 
            System.Collections.IEnumerable? after, 
            string path, 
            List<AuditChange> changes
            )
        {
            List<object?> oldList = before == null ? new List<object?>() : ToList(before);
            List<object?> newList = after == null ? new List<object?>() : ToList(after);

            int max = Math.Max(oldList.Count, newList.Count);

            for (int i = 0; i < max; i++)
            {
                object? oldValue = i < oldList.Count ? oldList[i] : null;
                object? newValue = i < newList.Count ? newList[i] : null;
                string itemPath = $"{path}[{i}]";

                CompareScalar(oldValue, newValue, itemPath, changes);
            }
        }

        private static void CompareComplexCollection(
            System.Collections.IEnumerable? before, 
            System.Collections.IEnumerable? after, 
            Type collectionType,
            string path, List<AuditChange> changes, HashSet<object> visited)
        {
            List<object?> oldList = before == null ? new List<object?>() : ToList(before);
            List<object?> newList = after == null ? new List<object?>() : ToList(after);

            Type elementType = GetElementType(collectionType);
            int max = Math.Max(oldList.Count, newList.Count);

            for (int i = 0; i < max; i++)
            {
                object? oldItem = i < oldList.Count ? oldList[i] : null;
                object? newItem = i < newList.Count ? newList[i] : null;
                string itemPath = $"{path}[{i}]";

                Walk(oldItem, newItem, elementType, itemPath, changes, visited);
            }
        }

        private static List<object?> ToList(System.Collections.IEnumerable source)
        {
            var list = new List<object?>();
            foreach (object? item in source)
            {
                list.Add(item);
            }
            return list;
        }


    }
}
