using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace LetExpandoObject
{
    internal class LetDictionary : LetDictionary<string, object>
    {
    }

    internal class LetDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        protected Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();

        public TValue this[TKey key]
        {
            get
            {
                TValue val = default(TValue);
                _map.TryGetValue(key, out val);
                return val;
            }
            set
            {
                _map[key] = value;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(TKey key, TValue value)
        {
            _map.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _map.ContainsKey(key);
        }

        public ICollection<TKey> Keys
        {
            get { return _map.Keys; }
        }

        public bool Remove(TKey key)
        {
            return _map.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _map.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return _map.Values; }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ((IDictionary<TKey, TValue>)_map).Add(item);
        }

        public void Clear()
        {
            _map.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((IDictionary<TKey, TValue>)_map).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((IDictionary<TKey, TValue>)_map).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _map.Count; }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return ((IDictionary<TKey, TValue>)_map).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _map.GetEnumerator();
        }
    }

    public class let : DynamicObject, IDictionary<string, object>
    {
        private LetDictionary _map = new LetDictionary();
        private Type type;
        private object value;
        private IEnumerable<PropertyInfo> properties;
        private bool isPrimitive;


        public let()
        {
        }

        public let(object obj)
        {
            type = obj.GetType();
            isPrimitive = IsPrimitive(type);
            
            if (isPrimitive)
            {
                value = obj;
            }
            else
            {
                properties = GetProperties(type);
                foreach (var property in properties)
                {
                    var value = property.GetValue(obj);

                    this.Add(property.Name, value);
                }
            }
        }

        public object this[string key]
        {
            get
            {
                object? val = null;
                _map.TryGetValue(key, out val);
                return val;
            }
            set
            {
                type = value.GetType();
                _map[key] = value;
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(string key, object value)
        {
            _map.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _map.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _map.Keys; }
        }

        public bool Remove(string key)
        {
            return _map.Remove(key);
        }

        public ICollection<object> Values
        {
            get { return _map.Values; }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            ((IDictionary<string, object>)_map).Add(item);
        }

        public void Clear()
        {
            _map.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((IDictionary<string, object>)_map).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((IDictionary<string, object>)_map).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _map.Count; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return Remove(item);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return ((IDictionary<string, object>)_map).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _map.GetEnumerator();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = _map[binder.Name];
            return true;
        }

        public bool TryGetValue(string key, out object value)
        {
            return ((IDictionary<string, object>)_map).TryGetValue(key, out value);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _map[binder.Name] = value;
            return true;
        }

        public static let FromObject(object obj, bool deep = false, bool ignoreNullValues = false)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return deep ? FromObjectDeep(obj, ignoreNullValues) : FromObjectShallow(obj, ignoreNullValues);
        }

        private static let FromObjectShallow(object obj, bool ignoreNullValues)
        {
            var geo = new let();
            var type = obj.GetType();
            var properties = GetProperties(type);

            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                if (ignoreNullValues && !IsPrimitive(property.PropertyType) && value == null)
                {
                    continue;
                }
                geo.Add(property.Name, value);
            }

            return geo;
        }

        private static let FromObjectDeep(object obj, bool ignoreNullValues)
        {
            var geo = new let();
            var type = obj.GetType();
            var properties = GetProperties(type);

            foreach (var property in properties)
            {
                if (IsPrimitive(property.PropertyType))
                {
                    var value = property.GetValue(obj);
                    geo.Add(property.Name, value);
                }
                else
                {
                    var value = property.GetValue(obj);
                    if (ignoreNullValues && value == null)
                    {
                        continue;
                    }
                    if (value == null)
                    {
                        geo.Add(property.Name, null);
                    }
                    else
                    {
                        geo.Add(property.Name, FromObjectDeep(value, ignoreNullValues));
                    }
                }
            }

            return geo;
        }

        private static IEnumerable<PropertyInfo> GetProperties(Type type) =>
            type.GetRuntimeProperties();

        private static bool IsPrimitive(Type type) =>
            type.GetTypeInfo().IsPrimitive ||
            type.GetTypeInfo().IsValueType ||
            type == typeof(string);
    }
}
