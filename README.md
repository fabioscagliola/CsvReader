# CsvReader

A utility class providing a way of reading a forward-only stream of rows from a CSV file.

## Overview

The **CsvReader** class allows reading from a CSV file given its path or its contents as an array of bytes.

It is designed to load one line at a time and retrieve the values of the individual fields using the names included in the header row.

If the file does include a header row, it is possible to inject the field names.

It supports multiple separators and encodings, and it allows to convert empty strings to null values.

## Example usage

Let us consider the following CSV file including a header row.

```
Name;BirthDate;IsMarried
John;1975-01-23;true
Jane;1985-07-07;false
```

The following sample code reads from the file into a list of objects.

```csharp
private class Person
{
    public string Name { get; set; }
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
```

If the file did not include the header row, the field names could be injected using the Load() method.

```csharp
reader.Load(new List<string> { "Name", "BirthDate", "IsMarried", });
```

## Documentation

This section describes the constructors, properties, and methods of the **CsvReader** class.

### Constructors


The following constructors allow reading from a CSV file given its path.

```csharp
public CsvReader(string path, params char[] separator)

public CsvReader(string path, bool convertEmptyStringToNull, params char[] separator)

public CsvReader(string path, bool convertEmptyStringToNull, Encoding encoding, params char[] separator)
```

The following constructors allow reading from a CSV file given its contents as an array of bytes.

```csharp
public CsvReader(byte[] data, params char[] separator)

public CsvReader(byte[] data, bool convertEmptyStringToNull, params char[] separator)

public CsvReader(byte[] data, bool convertEmptyStringToNull, Encoding encoding, params char[] separator)
```

#### Parameters

| Name | Type | Description |
|--|--|--|
| path| [byte](https://learn.microsoft.com/dotnet/api/system.byte)[] | The contents of the CSV file. |
| data | [string](https://learn.microsoft.com/dotnet/api/system.string) | The full path to the CSV file. |
| convertEmptyStringToNull | [bool](https://learn.microsoft.com/dotnet/api/system.boolean) | A Boolean value indicating if empty strings are returned as null references. |
| encoding | [Encoding](https://learn.microsoft.com/dotnet/api/system.text.encoding) | The encoding to be used when reading the CSV file. |
| separator | [char](https://learn.microsoft.com/dotnet/api/system.char)[] | The character(s) used to delimit field names and values. |

### Properties

The **CsvReader** class exposes the following properties.

#### Keys

Gets the names of the fields.

```csharp
public List<string>? Keys { get; }
```

#### Line

Gets the raw contents of the current line: (1) null before invoking the Load() method, (2) the raw contents of the first line (containing field names) after invoking the Load() method and before invoking the ReadLine() method, and (3) the raw contents of the current line after each subsequent invocation of the ReadLine() method.

```csharp
public string? Line { get; }
```

### Methods

The **CsvReader** class exposes the following methods.

#### Load()

Loads the CSV file and reads the field names from the first line.

```csharp
public void Load()
```

#### Load(List<string>) 

Loads the CSV file injecting the specified field names.

```csharp
public void Load(List<string> keys)
```

##### Parameters

| Name | Type | Description |
|--|--|--|
| keys | [List](https://learn.microsoft.com/dotnet/api/system.collections.generic.list-1)<[string](https://learn.microsoft.com/dotnet/api/system.string)> | The names of the fields. |

#### ReadLine()

Reads the next line of values from the CSV file.

**Returns true if the line exists or false if the end of the file has been reached.**

```csharp
public bool ReadLine()
```

#### GetBoolValue(string)

Returns the value of a bool field.

```csharp
public bool GetBoolValue(string key)
```

#####  Parameters

| Name | Type | Description |
|--|--|--|
| key | [string](https://learn.microsoft.com/dotnet/api/system.string) | The name of the field. |

#### GetDateTimeValue(string, string)

Returns the value of a date and time field.

```csharp
public DateTime GetDateTimeValue(string key, string format)
```

##### Parameters

| Name | Type | Description |
|--|--|--|
| key | [string](https://learn.microsoft.com/dotnet/api/system.string) | The name of the field. |
| format | [string](https://learn.microsoft.com/dotnet/api/system.string) | The format of the field. |

#### GetStringValue(string)

Returns the value of a string field.

```csharp
public string? GetStringValue(string key)
```

##### Parameters

| Name | Type | Description |
|--|--|--|
| key | [string](https://learn.microsoft.com/dotnet/api/system.string) | The name of the field. |

#### Close()

Closes the underlying memory stream and stream reader.

```csharp
public void Close()
```

#### Dispose()

Disposes of the underlying memory stream and stream reader.

```csharp
public void Dispose()
```

