using com.fabioscagliola.CsvReader.Properties;
using System.Globalization;
using System.Text;

namespace com.fabioscagliola.Util;

/// <summary>
/// Reads data from a CSV file.
/// </summary>
public sealed class CsvReader : IDisposable
{
    /// <summary>
    /// The contents of the CSV file.
    /// </summary>
    private readonly byte[]? data;

    /// <summary>
    /// The full path to the CSV file.
    /// </summary>
    private readonly string? path;

    /// <summary>
    /// A Boolean value indicating if empty strings are returned as null references.
    /// </summary>
    private readonly bool convertEmptyStringToNull;

    /// <summary>
    /// The encoding to be used when reading the CSV file.
    /// </summary>
    private readonly Encoding encoding = Encoding.Default;

    /// <summary>
    /// The character(s) used to delimit field names and values.
    /// </summary>
    private readonly char[] separator;


    /// <summary>
    /// The names of the fields.
    /// </summary>
    private List<string>? keys;

    /// <summary>
    /// The raw contents of the current line.
    /// </summary>
    private string? line = null;

    /// <summary>
    /// The values of current line.
    /// </summary>
    private List<string>? values;

    /// <summary>
    /// The underlying memory stream.
    /// </summary>
    private MemoryStream? memoryStream;

    /// <summary>
    /// The underlying stream reader.
    /// </summary>
    private StreamReader? streamReader;

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvReader"/> class.
    /// </summary>
    /// <param name="path">The full path to the CSV file.</param>
    /// <param name="separator">The character(s) used to delimit field names and values.</param>
    public CsvReader(string path, params char[] separator)
    {
        this.data = null;
        this.path = path;
        this.convertEmptyStringToNull = false;
        this.separator = separator;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvReader"/> class.
    /// </summary>
    /// <param name="path">The full path to the CSV file.</param>
    /// <param name="convertEmptyStringToNull">A Boolean value indicating if empty strings are returned as null references.</param>
    /// <param name="separator">The character(s) used to delimit field names and values.</param>
    public CsvReader(string path, bool convertEmptyStringToNull, params char[] separator)
        : this(path, separator)
    {
        this.convertEmptyStringToNull = convertEmptyStringToNull;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvReader"/> class.
    /// </summary>
    /// <param name="path">The full path to the CSV file.</param>
    /// <param name="convertEmptyStringToNull">A Boolean value indicating if empty strings are returned as null references.</param>
    /// <param name="encoding">The encoding to be used when reading the CSV file.</param>
    /// <param name="separator">The character(s) used to delimit field names and values.</param>
    public CsvReader(string path, bool convertEmptyStringToNull, Encoding encoding, params char[] separator)
        : this(path, convertEmptyStringToNull, separator)
    {
        this.encoding = encoding;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvReader"/> class.
    /// </summary>
    /// <param name="data">The contents of the CSV file.</param>
    /// <param name="separator">The character(s) used to delimit field names and values.</param>
    public CsvReader(byte[] data, params char[] separator)
    {
        this.data = data;
        this.path = null;
        this.convertEmptyStringToNull = false;
        this.separator = separator;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvReader"/> class.
    /// </summary>
    /// <param name="data">The contents of the CSV file.</param>
    /// <param name="convertEmptyStringToNull">A Boolean value indicating if empty strings are returned as null references.</param>
    /// <param name="separator">The character(s) used to delimit field names and values.</param>
    public CsvReader(byte[] data, bool convertEmptyStringToNull, params char[] separator)
        : this(data, separator)
    {
        this.convertEmptyStringToNull = convertEmptyStringToNull;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CsvReader"/> class.
    /// </summary>
    /// <param name="data">The contents of the CSV file.</param>
    /// <param name="convertEmptyStringToNull">A Boolean value indicating if empty strings are returned as null references.</param>
    /// <param name="encoding">The encoding to be used when reading the CSV file.</param>
    /// <param name="separator">The character(s) used to delimit field names and values.</param>
    public CsvReader(byte[] data, bool convertEmptyStringToNull, Encoding encoding, params char[] separator)
        : this(data, convertEmptyStringToNull, separator)
    {
        this.encoding = encoding;
    }

    /// <summary>
    /// Gets the names of the fields.
    /// </summary>
    public List<string>? Keys
    {
        get
        {
            return keys;
        }
    }

    /// <summary>
    /// Gets the raw contents of the current line:
    /// (1) null before invoking the <see cref="Load"/> method,
    /// (2) the raw contents of the first line (containing field names) after invoking the <see cref="Load"/> method and before invoking the <see cref="ReadLine"/> method,
    /// and (3) the raw contents of the current line after each subsequent invocation of the <see cref="ReadLine"/> method.
    /// </summary>
    public string? Line
    {
        get
        {
            return line;
        }
    }

    /// <summary>
    /// Loads the CSV file and reads the field names from the first line.
    /// </summary>
    public void Load()
    {
        InitializeStreamReader();

        if (!ReadLineTo(out keys))
        {
            throw new ApplicationException(Resources.LoadException1);  // Cannot read field names. Is the file empty?
        }
    }

    /// <summary>
    /// Loads the CSV file injecting the specified field names.
    /// </summary>
    /// <param name="keys">The names of the fields.</param>
    public void Load(List<string> keys)
    {
        InitializeStreamReader();

        this.keys = keys;
    }

    /// <summary>
    /// Reads the next line of values from the CSV file.
    /// </summary>
    /// <returns>Returns true if the line exists or false if the end of the file has been reached.</returns>
    public bool ReadLine()
    {
        bool read = ReadLineTo(out values);

        if (read && keys != null && keys.Count != values.Count)
        {
            throw new ApplicationException(Resources.ReadLineException1);  // The number of values does not match the number of keys.
        }

        return read;
    }

    /// <summary>
    /// Returns the value of a bool field.
    /// </summary>
    /// <param name="key">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    public bool GetBoolValue(string key)
    {
        string? value = GetStringValue(key);

        _ = bool.TryParse(value, out bool result);

        return result;
    }

    /// <summary>
    /// Returns the value of a date and time field.
    /// </summary>
    /// <param name="key">The name of the field.</param>
    /// <param name="format">The format of the field.</param>
    /// <returns>The value of the field.</returns>
    public DateTime GetDateTimeValue(string key, string format)
    {
        string? value = GetStringValue(key);

        _ = DateTime.TryParseExact(value, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result);

        return result;
    }

    /// <summary>
    /// Returns the value of a string field.
    /// </summary>
    /// <param name="key">The name of the field.</param>
    /// <returns>The value of the field.</returns>
    public string? GetStringValue(string key)
    {
        if (keys == null)
        {
            throw new ApplicationException(Resources.GetStringValueException1);  // Keys not read. Did you invoke the Load method?
        }

        if (values == null)
        {
            throw new ApplicationException(Resources.GetStringValueException2);  // Values not read. Did you invoke the ReadLine method?
        }

        int index = keys.IndexOf(key);

        if (index == -1)
        {
            throw new ApplicationException(string.Format(Resources.GetStringValueException3, key));  // Key "{0}" not found.
        }

        string? value = values[index].Trim();

        if (value == string.Empty && convertEmptyStringToNull)
        {
            value = null;
        }

        return value;
    }

    /// <summary>
    /// Closes the underlying memory stream and stream reader.
    /// </summary>
    public void Close()
    {
        memoryStream?.Close();
        streamReader?.Close();
    }

    /// <summary>
    /// Disposes of the underlying memory stream and stream reader.
    /// </summary>
    public void Dispose()
    {
        memoryStream?.Dispose();
        streamReader?.Dispose();
    }

    /// <summary>
    /// Initializes the underlying stream reader.
    /// </summary>
    private void InitializeStreamReader()
    {
        if (data == null && path != null)
        {
            streamReader = new StreamReader(path, encoding);
        }
        else if (data != null && path == null)
        {
            memoryStream = new(data);
            streamReader = new StreamReader(memoryStream, encoding);
        }
    }

    /// <summary>
    /// Reads the next line of values from the CSV file to the specified list of strings.
    /// </summary>
    /// <param name="target">The list of strings to store values.</param>
    /// <returns>Returns true if the line exists or false if the end of the file has been reached.</returns>
    private bool ReadLineTo(out List<string> target)
    {
        if (streamReader == null)
        {
            throw new ApplicationException(Resources.ReadLineToException1);  // The underlying stream reader is not initialized. Did you invoke the Load method?
        }

        bool read = false;

        target = new List<string>();

        line = streamReader.ReadLine();

        if (line != null && !string.IsNullOrWhiteSpace(line))
        {
            target.AddRange(line.Split(separator));
            read = true;
        }

        return read;
    }
}
