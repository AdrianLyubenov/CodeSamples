using System;

[Serializable]
public class Pair<TKey, TValue>
{
    public TKey Key;
    public TValue Value;
}

[Serializable]
public class PairStringString : Pair<string, string>
{ }

[Serializable]
public class Triple<TKey, TValue, TValue2>
{
    public TKey Key;
    public TValue Value;
    public TValue2 Value2;
}