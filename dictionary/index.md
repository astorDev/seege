# How C# Dictionary Actually Work

`Dictionary<TKey, TValue>` is a very popular data structure in C# and a popular choice for interview questions. I've used `Dictionary` a billion times and I was pretty sure I understand how they work. I knew they are very fast in finding a value by its key. However, in a recent interview, I gave a wrong answer when I was asked how exactly fast they are. In this article, I'll correct my mistake and investigate `Dictionary` in depth. Let's get into it!

## What Did I Knew

## Original snippets

[TryGetValue](https://referencesource.microsoft.com/#mscorlib/system/collections/generic/dictionary.cs,498):

```csharp
public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
{
    int i = FindEntry(key);
    if (i >= 0) {
        value = entries[i].value;
        return true;
    }
    value = default(TValue);
    return false;
}
```

[FindEntry](https://referencesource.microsoft.com/#mscorlib/system/collections/generic/dictionary.cs,bcd13bb775d408f1)

```csharp
private int FindEntry(TKey key) {
    if( key == null) {
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
    }
 
    if (buckets != null) {
        int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
        for (int i = buckets[hashCode % buckets.Length]; i >= 0; i = entries[i].next) {
            if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key)) return i;
        }
    }
    return -1;
}
```

where `& 0x7FFFFFFF` makes the hashcode positive, roughly like `Math.Abs`.

```csharp
private struct Entry {
    public int hashCode;
    public int next;
    public TKey key;
    public TValue value;
}
 
private int[] buckets;
private Entry[] entries;
private int count;
private int version;
private int freeList;
private int freeCount;
private IEqualityComparer<TKey> comparer;
```

```csharp
private void Insert(TKey key, TValue value, bool add) {

    if( key == null ) {
        ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
    }
 
    if (buckets == null) Initialize(0);
    int hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
    int targetBucket = hashCode % buckets.Length;
 
#if FEATURE_RANDOMIZED_STRING_HASHING
    int collisionCount = 0;
#endif
 
    for (int i = buckets[targetBucket]; i >= 0; i = entries[i].next) {
        if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key)) {
            if (add) { 
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);
            }
            entries[i].value = value;
            version++;
            return;
        } 
 
#if FEATURE_RANDOMIZED_STRING_HASHING
                collisionCount++;
#endif
    }

    int index;
    if (freeCount > 0) {
        index = freeList;
        freeList = entries[index].next;
        freeCount--;
    }
    else {
        if (count == entries.Length)
        {
            Resize();
            targetBucket = hashCode % buckets.Length;
        }
        index = count;
        count++;
    }
 
    entries[index].hashCode = hashCode;
    entries[index].next = buckets[targetBucket];
    entries[index].key = key;
    entries[index].value = value;
    buckets[targetBucket] = index;
    version++;
 
#if FEATURE_RANDOMIZED_STRING_HASHING
 
#if FEATURE_CORECLR
    // In case we hit the collision threshold we'll need to switch to the comparer which is using randomized string hashing
    // in this case will be EqualityComparer<string>.Default.
    // Note, randomized string hashing is turned on by default on coreclr so EqualityComparer<string>.Default will 
    // be using randomized string hashing
 
    if (collisionCount > HashHelpers.HashCollisionThreshold && comparer == NonRandomizedStringEqualityComparer.Default) 
    {
        comparer = (IEqualityComparer<TKey>) EqualityComparer<string>.Default;
        Resize(entries.Length, true);
    }
#else
    if(collisionCount > HashHelpers.HashCollisionThreshold && HashHelpers.IsWellKnownEqualityComparer(comparer)) 
    {
        comparer = (IEqualityComparer<TKey>) HashHelpers.GetRandomizedEqualityComparer(comparer);
        Resize(entries.Length, true);
    }
#endif // FEATURE_CORECLR
 
#endif 
}
```

```csharp
private void Initialize(int capacity) {
    int size = HashHelpers.GetPrime(capacity);
    buckets = new int[size];
    for (int i = 0; i < buckets.Length; i++) buckets[i] = -1;
    entries = new Entry[size];
    freeList = -1;
}
```

```csharp
private void Resize(int newSize, bool forceNewHashCodes) {
    Contract.Assert(newSize >= entries.Length);
    int[] newBuckets = new int[newSize];
    for (int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
    Entry[] newEntries = new Entry[newSize];
    Array.Copy(entries, 0, newEntries, 0, count);
    if(forceNewHashCodes) {
        for (int i = 0; i < count; i++) {
            if(newEntries[i].hashCode != -1) {
                newEntries[i].hashCode = (comparer.GetHashCode(newEntries[i].key) & 0x7FFFFFFF);
            }
        }
    }
    for (int i = 0; i < count; i++) {
        if (newEntries[i].hashCode >= 0) {
            int bucket = newEntries[i].hashCode % newSize;
            newEntries[i].next = newBuckets[bucket];
            newBuckets[bucket] = i;
        }
    }
    buckets = newBuckets;
    entries = newEntries;
}
```

## Educational Dictionary

```csharp
public class EducationalDictionary<TKey, TValue>
{
    private struct Entry {
        public int hashCode;
        public int next;
        public TKey key;
        public TValue? value;

        string ValueString => value == null ? "null" : value!.ToString()!;
        public override string ToString()
        {
            return $"{key} - {ValueString} + (next = {next})";
        }
    }
 
    private int[] buckets;
    private Entry[] entries;
    private int count;
    private readonly EqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;

    public EducationalDictionary()
    {
        var size = HashHelpers.GetPrime(0);
        buckets = new int[size];
        for (var i = 0; i < buckets.Length; i++) buckets[i] = -1;
        entries = new Entry[size];
        
        PrintFullState("\ud83d\ude80 Initialized");
    }
    
    public TValue? GetValueOrDefault(TKey key) {
        var i = FindEntry(key);
        return i >= 0 ? entries[i].value : default;
    }
    
    public void Add(TKey key, TValue value) {
        var hashCode = comparer.GetHashCode(key) & 0x7FFFFFFF;
        var targetBucket = hashCode % buckets!.Length;
        
        if (count == entries!.Length)
        {
            Resize(HashHelpers.ExpandPrime(count));
            targetBucket = hashCode % buckets.Length;
        }
        
        var index = count;
        count++;
        
        entries[index].hashCode = hashCode;
        entries[index].next = buckets[targetBucket];
        entries[index].key = key;
        entries[index].value = value;
        buckets[targetBucket] = index;
        
        Console.WriteLine();
        PrintFullState($"\ud83d\udce5 Add: {key} - {value}. hashCode = {hashCode}, targetBucket = {targetBucket}");
    }
    
    private void Resize(int newSize) {
        var newBuckets = new int[newSize];
        for (var i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;
        var newEntries = new Entry[newSize];
        Array.Copy(entries, 0, newEntries, 0, count);

        for (var i = 0; i < count; i++) {
            if (newEntries[i].hashCode >= 0) {
                var bucket = newEntries[i].hashCode % newSize;
                newEntries[i].next = newBuckets[bucket];
                newBuckets[bucket] = i;
            }
        }

        buckets = newBuckets;
        entries = newEntries;
        
        PrintFullState("\u2194\ufe0f Resize");
    }

    private int FindEntry(TKey key)
    {
        var hashCode = comparer.GetHashCode(key!) & 0x7FFFFFFF;
        var initialBucketIndex = hashCode % buckets.Length;
        
        Console.WriteLine();
        Console.WriteLine($"\ud83d\udd0e Search. Key = {key}. Initial Bucket Index {initialBucketIndex}");
        
        for (var i = buckets[initialBucketIndex]; i >= 0; i = entries[i].next)
        {
            Console.WriteLine();
            Console.WriteLine($"Comparing key from entries[{i}] ({entries[i]}) to {key}");
            
            if (entries[i].hashCode == hashCode && comparer.Equals(entries[i].key, key))
            {
                Console.WriteLine($"Key is equal returning {i}");
                
                return i;
            }
            else {
                Console.WriteLine($"Key is not equal, moving to the next linked index ({entries[i].next})");
            }
        }
        
        Console.WriteLine();
        Console.WriteLine("Search exit condition met (i >= 0). Returning -1 (as not found)");
        return -1;
    }

    public void PrintFullState(string preface)
    {
        Console.WriteLine();
        Console.Write(this.ToString(preface));
    }

    public string ToString(string preface)
    {
        StringBuilder result = new();
        
        result.AppendLine($"{preface}, state:");
        result.AppendLine();
        result.AppendLine($"buckets: [{String.Join(", ", buckets!)}]");
        result.AppendLine($"entries:");
        for (int i = 0; i < entries.Length; i++)
        {
            result.AppendLine($" [{i}] = {entries[i]}");
        }
        result.AppendLine($"count: {count}");
        return result.ToString();
    }
}

public static class HashHelpers
{
    public static readonly int[] primes = {
        3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919,
        1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591,
        17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437,
        187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263,
        1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369};
    
    public static int GetPrime(int min)
    {
        foreach (var prime in primes)
        {
            if (prime >= min) return prime;
        }

        return min;
    }
    
    public static int ExpandPrime(int oldSize)
    {
        return GetPrime(2 * oldSize);
    }
}
```

## Performing the tests

```csharp
[TestMethod]
public void FindsMultipleKeys()
{
    var dict = new EducationalDictionary<int, string>();
    dict.Add(48, "John");
    dict.Add(34, "Josh");
    dict.Add(22, "Jack");
    dict.Add(11, "Alex");

    dict.GetValueOrDefault(48).Should().Be("John");
    dict.GetValueOrDefault(34).Should().Be("Josh");
    dict.GetValueOrDefault(22).Should().Be("Jack");
    dict.GetValueOrDefault(11).Should().Be("Alex");
    dict.GetValueOrDefault(50).Should().Be(null);
}
```

```csharp
🚀 Initialized, state:

buckets: [-1, -1, -1]
entries:
 [0] = 0 - null + (next = 0)
 [1] = 0 - null + (next = 0)
 [2] = 0 - null + (next = 0)
count: 0


📥 Add: 48 - John. hashCode = 48, targetBucket = 0, state:

buckets: [0, -1, -1]
entries:
 [0] = 48 - John + (next = -1)
 [1] = 0 - null + (next = 0)
 [2] = 0 - null + (next = 0)
count: 1


📥 Add: 34 - Josh. hashCode = 34, targetBucket = 1, state:

buckets: [0, 1, -1]
entries:
 [0] = 48 - John + (next = -1)
 [1] = 34 - Josh + (next = -1)
 [2] = 0 - null + (next = 0)
count: 2


📥 Add: 22 - Jack. hashCode = 22, targetBucket = 1, state:

buckets: [0, 2, -1]
entries:
 [0] = 48 - John + (next = -1)
 [1] = 34 - Josh + (next = -1)
 [2] = 22 - Jack + (next = 1)
count: 3

↔️ Resize, state:

buckets: [-1, 2, -1, -1, -1, -1, 1]
entries:
 [0] = 48 - John + (next = -1)
 [1] = 34 - Josh + (next = 0)
 [2] = 22 - Jack + (next = -1)
 [3] = 0 - null + (next = 0)
 [4] = 0 - null + (next = 0)
 [5] = 0 - null + (next = 0)
 [6] = 0 - null + (next = 0)
count: 3


📥 Add: 11 - Alex. hashCode = 11, targetBucket = 4, state:

buckets: [-1, 2, -1, -1, 3, -1, 1]
entries:
 [0] = 48 - John + (next = -1)
 [1] = 34 - Josh + (next = 0)
 [2] = 22 - Jack + (next = -1)
 [3] = 11 - Alex + (next = -1)
 [4] = 0 - null + (next = 0)
 [5] = 0 - null + (next = 0)
 [6] = 0 - null + (next = 0)
count: 4

🔎 Search. Key = 48. Initial Bucket Index 6

Comparing key from entries[1] (34 - Josh + (next = 0)) to 48
Key is not equal, moving to the next linked index (0)

Comparing key from entries[0] (48 - John + (next = -1)) to 48
Key is equal returning 0

🔎 Search. Key = 34. Initial Bucket Index 6

Comparing key from entries[1] (34 - Josh + (next = 0)) to 34
Key is equal returning 1

🔎 Search. Key = 22. Initial Bucket Index 1

Comparing key from entries[2] (22 - Jack + (next = -1)) to 22
Key is equal returning 2

🔎 Search. Key = 11. Initial Bucket Index 4

Comparing key from entries[3] (11 - Alex + (next = -1)) to 11
Key is equal returning 3

🔎 Search. Key = 50. Initial Bucket Index 1

Comparing key from entries[2] (22 - Jack + (next = -1)) to 50
Key is not equal, moving to the next linked index (-1)

Search exit condition met (i >= 0). Returning -1 (as not found)
```

## Wrapping Up

Before just recently I thought dictionaies use hash codes to form a sorted listed and find a key using binary search, so I assumed a logarithmic complexity of it. Turns out the hash code is actually just used to mathematically find a matching index of a values bucket, which means constant complexity of the algorithm. Although I felt slightly ashamed that I didn't know how such an important data structure works it was nevertheless pleasant to figure out it is even better then I assumed. And hey... claps are appreciated 👏