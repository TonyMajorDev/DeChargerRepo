using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


class Caching
{
}

public abstract class Cache<TKey, TValue, TCacheValue>
where TCacheValue : CacheValue
{
    protected abstract class CacheValue
    {
        public CacheValue(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; set; }
    }

    private readonly Dictionary<TKey, TCacheValue> _ValueCache = new Dictionary<TKey, TCacheValue>();
}

public class SizeLimitedCache<TKey, TValue> : Cache<TKey, TValue, SizeLimitedCacheValue>
{
    private class SizeLimitedCacheValue : CacheValue
    {
        public SizeLimitedCacheValue(TValue value)
            : base(value)
        { }

        public LinkedListNode<KeyValuePair<TKey, SizeLimitedCacheValue>> IndexRef { get; set; }
    }
}

public abstract class Cache<TKey, TValue, TCacheValueData>
{
    public abstract class CacheValue
    {
        public CacheValue(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; set; }
        public TCacheValueData Data { get; set; }
    }

    private readonly Dictionary<TKey, CacheValue> _ValueCache = new Dictionary<TKey, CacheValue>();
}

public class AutoExpiryCache<TKey, TValue> : Cache<TKey, TValue, DateTime>
{
}