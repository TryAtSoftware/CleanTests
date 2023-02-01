namespace TryAtSoftware.CleanTests.Core.Utilities;

using System;
using System.Collections.Generic;

public class GlobalUtilitiesProvider
{
    private readonly Dictionary<string, object> _utilities = new ();

    public bool AddUtility(string uniqueId, object instance)
    {
        if (!this._utilities.ContainsKey(uniqueId)) this._utilities[uniqueId] = new Dictionary<Type, object>();

        this._utilities[uniqueId] = instance;
        return true;
    }

    public object? GetUtility(string uniqueId)
    {
        if (!this._utilities.ContainsKey(uniqueId)) return null;
        return this._utilities[uniqueId];
    }

    public IEnumerable<T> GetUtilities<T>()
    {
        foreach (var (_, utility) in this._utilities)
            if (utility is T t) yield return t;
    }
}