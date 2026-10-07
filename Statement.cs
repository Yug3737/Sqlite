namespace Sqlite;

/// <summary>
/// C# representation of a Sql statement
/// </summary>
public class Statement
{
    public StatementType Type;
    public Row RowToInsert { get; set; } = new (); // only used by insert statement
}

public enum StatementType
{
    Select,
    Insert,
    Update,
    Delete
};
