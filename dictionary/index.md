# How C# Dictionary Actually Work

`Dictionary<TKey, TValue>` is a very popular data structure in C# and a popular choice for interview questions. I've used `Dictionary` a billion times and I was pretty sure I understand how they work. I knew they are very fast in finding a value by its key. However, in a recent interview, I gave a wrong answer when I was asked how exactly fast they are. In this article, I'll correct my mistake and investigate `Dictionary` in depth. Let's get into it!

## What Did I Knew

## Wrapping Up

Before just recently I thought dictionaies use hash codes to form a sorted listed and find a key using binary search, so I assumed a logarithmic complexity of it. Turns out the hash code is actually just used to mathematically find a matching index of a values bucket, which means constant complexity of the algorithm. Although I felt slightly ashamed that I didn't know how such an important data structure works it was nevertheless pleasant to figure out it is even better then I assumed. And hey... claps are appreciated 👏
