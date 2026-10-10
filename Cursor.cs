namespace Sqlite;

/// <summary>
/// Represents a location in the table.
/// </summary>
public class Cursor
{
    public Table table { get; set; }
    public int rowNumber { get; set; }
    public bool endOfTable { get; set; } 
}