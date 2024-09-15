using FluentAssertions;

namespace DictionaryPlayground;

[TestClass]
public class SimplifiedDictionaryTest
{
    [TestMethod]
    public void FindsSingularKey()
    {
        var dict = new EducationalDictionary<string, string>();
        dict.Add("one", "John");

        dict.GetValueOrDefault("one").PrintAndAssert("John");
    }

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
}

public static class PrintedAssertion
{
    public static void PrintAndAssert<T>(this T? actual, T? expected)
    {
        //Console.WriteLine(actual);
        actual.Should().Be(expected);
    }
}