using System.Text;

namespace DictionaryPlayground;

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
