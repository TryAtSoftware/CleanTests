namespace TryAtSoftware.CleanTests.Core.Utilities;

using System;
using System.Collections.Generic;

public class GlobalUtilitiesProvider
{
    private readonly Dictionary<string, Dictionary<Type, object>> _utilities = new ();

    public bool AddUtility(string uniqueId, Type type, object instance)
    {
        if (!this._utilities.ContainsKey(uniqueId)) this._utilities[uniqueId] = new Dictionary<Type, object>();
        if (this._utilities[uniqueId].ContainsKey(type)) return false;

        this._utilities[uniqueId][type] = instance;
        return true;
    }

    public object? GetUtility(string uniqueId, Type type)
    {
        if (!this._utilities.ContainsKey(uniqueId) || !this._utilities[uniqueId].ContainsKey(type)) return null;
        return this._utilities[uniqueId][type];
    }

    public IEnumerable<T> GetUtilities<T>()
    {
        foreach (var (_, ubt) in this._utilities)
        {
            foreach (var (_, utility) in ubt)
                if (utility is T t) yield return t;
        }
    }
}