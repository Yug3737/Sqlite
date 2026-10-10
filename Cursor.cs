
namespace Sqlite;

/// <summary>
/// Represents a location in the table.
/// </summary>
public class Cursor
{
    public Table Table { get; set; }
    public int RowNumber { get; set; }
    public bool EndOfTable { get; set; } // representing a position past the end of current table

    public Cursor(Table table, int rowNumber = 0, bool endOfTable = false)
    {
        Table = table;
        RowNumber = rowNumber;
        EndOfTable = endOfTable;
    }

    public void Advance()
    {
        ++RowNumber;
        if (RowNumber >= Table.NumberOfRows) EndOfTable = true;
    }
}