using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SayDB.IoJobs;

internal class AlreadyHandledItems
{
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<object, object>> _map = new();

    public bool Contains(Type type, object primaryKey)
    {
        if (!_map.ContainsKey(type)) return false;

        var keyValueMap = _map[type];
        return keyValueMap.ContainsKey(primaryKey);
    }

    public void Add(Type type, object primaryKey, object value)
    {
        if (!_map.ContainsKey(type))
        {
            _map.TryAdd(type, new ConcurrentDictionary<object, object>());
        }

        var keyValueMap = _map[type];
        keyValueMap.TryAdd(primaryKey, value);
    }

    public object GetObject(Type type, object primaryKey) => _map[type][primaryKey];
}
