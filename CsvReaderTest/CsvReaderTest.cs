using com.fabioscagliola.CsvReader.Properties;
using FluentAssertions;
using NUnit.Framework;

namespace com.fabioscagliola.Util.Test;

public abstract class CsvReaderTest
{
    protected abstract CsvReader LoadCsvFileEmpty();

    protected abstract CsvReader LoadCsvFileIncludingTheWrongNumberOfKeys();

    protected abstract CsvReader LoadCsvFileWithNoHeaderRow();

    protected abstract CsvReader LoadCsvFileWithHeaderRow();

    [Test]
    public void GivenEmptyFile_WhenInvokingLoad_ThenThrowsApplicationException()
    {
        using CsvReader reader = LoadCsvFileEmpty();
        FluentActions.Invoking(() => reader.Load()).Should().Throw<ApplicationException>()
            .Which.Message.Should().BeEquivalentTo(Resources.LoadException1);  // Cannot read field names. Is the file empty?
    }

    [Test]
    public void GivenWrongNumberOfKeys_WhenInvokingReadLine_ThenThrowsApplicationException()
    {
        using CsvReader reader = LoadCsvFileIncludingTheWrongNumberOfKeys();
        reader.Load();
        FluentActions.Invoking(() => reader.ReadLine()).Should().Throw<ApplicationException>()
            .Which.Message.Should().BeEquivalentTo(Resources.ReadLineException1);  // The number of values does not match the number of keys.
    }

    [Test]
    public void GivenWrongNumberOfKeysInjected_WhenInvokingReadLine_ThenThrowsApplicationException()
    {
        using CsvReader reader = LoadCsvFileWithNoHeaderRow();
        reader.Load(new List<string>());  // Injecting the wrong number of keys
        FluentActions.Invoking(() => reader.ReadLine()).Should().Throw<ApplicationException>()
            .Which.Message.Should().BeEquivalentTo(Resources.ReadLineException1);  // The number of values does not match the number of keys.

    }

    [Test]
    public void GivenLoadHasNotBeenInvoked_WhenInvokingReadLine_ThenThrowsApplicationException()
    {
        using CsvReader reader = LoadCsvFileWithHeaderRow();
        FluentActions.Invoking(() => reader.ReadLine()).Should().Throw<ApplicationException>()
            .Which.Message.Should().BeEquivalentTo(Resources.ReadLineToException1);  // The underlying stream reader is not initialized. Did you invoke the Load method?
    }

    [Test]
    public void GivenLoadHasNotBeenInvoked_WhenInvokingGetStringValue_ThenThrowsApplicationException()
    {
        using CsvReader reader = LoadCsvFileWithHeaderRow();
        FluentActions.Invoking(() => reader.GetStringValue("String")).Should().Throw<ApplicationException>()
            .Which.Message.Should().BeEquivalentTo(Resources.GetStringValueException1);  // Keys not read. Did you invoke the Load method?
    }

    [Test]
    public void GivenReadLineHasNotBeenInvoked_WhenInvokingGetStringValue_ThenThrowsApplicationException()
    {
        using CsvReader reader = LoadCsvFileWithHeaderRow();
        reader.Load();
        FluentActions.Invoking(() => reader.GetStringValue("String")).Should().Throw<ApplicationException>()
            .Which.Message.Should().BeEquivalentTo(Resources.GetStringValueException2);  // Values not read. Did you invoke the ReadLine method?
    }

    [Test]
    public void GivenInexistentKey_WhenInvokingGetStringValue_ThenThrowsApplicationException()
    {
        const string key = "InvalidKey";
        using CsvReader reader = LoadCsvFileWithHeaderRow();
        reader.Load();
        reader.ReadLine();
        FluentActions.Invoking(() => reader.GetStringValue(key)).Should().Throw<ApplicationException>()
            .Which.Message.Should().BeEquivalentTo(string.Format(Resources.GetStringValueException3, key));  // Key "{0}" not found.
    }

    [Test]
    [TestCase("Boolean1", false)]
    [TestCase("Boolean2", true)]
    public void GivenVaidKey_WhenInvokingGetBoolValue_ThenOk(string key, bool value)
    {
        using CsvReader reader = LoadCsvFileWithHeaderRow();
        reader.Load();
        reader.ReadLine();
        reader.GetBoolValue(key).Should().Be(value);
    }

    [Test]
    public void GivenVaidKey_WhenInvokingGetDateTimeValue_ThenOk()
    {
        using CsvReader reader = LoadCsvFileWithHeaderRow();
        reader.Load();
        reader.ReadLine();
        reader.GetDateTimeValue("DateTime", "yyyy-MM-dd").Should().Be(new DateTime(1975, 01, 23));
    }

    [Test]
    public void GivenVaidKey_WhenInvokingGetStringValue_ThenOk()
    {
        using CsvReader reader = LoadCsvFileWithHeaderRow();
        reader.Load();
        reader.ReadLine();
        reader.GetStringValue("String").Should().BeEquivalentTo("This is a test");
    }
}

[TestFixture]
public class CsvReaderTestData : CsvReaderTest
{
    protected override CsvReader LoadCsvFileEmpty()
    {
        return new(File.ReadAllBytes("CsvFile1.csv"), ';');
    }

    protected override CsvReader LoadCsvFileIncludingTheWrongNumberOfKeys()
    {
        return new(File.ReadAllBytes("CsvFile2.csv"), ';');
    }

    protected override CsvReader LoadCsvFileWithNoHeaderRow()
    {
        return new(File.ReadAllBytes("CsvFile3.csv"), ';');
    }

    protected override CsvReader LoadCsvFileWithHeaderRow()
    {
        return new(File.ReadAllBytes("CsvFile4.csv"), ';');
    }
}

[TestFixture]
public class CsvReaderTestPath : CsvReaderTest
{
    protected override CsvReader LoadCsvFileEmpty()
    {
        return new("CsvFile1.csv", ';');
    }

    protected override CsvReader LoadCsvFileIncludingTheWrongNumberOfKeys()
    {
        return new("CsvFile2.csv", ';');
    }

    protected override CsvReader LoadCsvFileWithNoHeaderRow()
    {
        return new("CsvFile3.csv", ';');
    }

    protected override CsvReader LoadCsvFileWithHeaderRow()
    {
        return new("CsvFile4.csv", ';');
    }

    [Test]
    public void ExampleUsage()
    {
        List<Person> people = ReadPeople();
        people.Count.Should().Be(2);
    }

    private class Person
    {
        public string? Name { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsMarried { get; set; }
    }

    private static List<Person> ReadPeople()
    {
        List<Person> people = new();
        using CsvReader reader = new("People.csv", ';');
        reader.Load();
        while (reader.ReadLine())
        {
            people.Add(new()
            {
                Name = reader.GetStringValue("Name"),
                BirthDate = reader.GetDateTimeValue("BirthDate", "yyyy-MM-dd"),
                IsMarried = reader.GetBoolValue("IsMarried")
            });
        }
        return people;
    }
}
