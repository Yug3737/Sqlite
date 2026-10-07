namespace Sqlite;

public static class Constants
{
    public const int USERNAME_SIZE = 32;
    public const int EMAIL_SIZE = 255;
    public const int ID_SIZE = 4;

    public const int ID_OFFSET = 0;
    public const int USERNAME_OFFSET = ID_OFFSET + ID_SIZE; // offset means which index position do the characters for username start?
    public const int EMAIL_OFFSET = USERNAME_OFFSET + USERNAME_SIZE;
    public const int ROW_SIZE = ID_SIZE + USERNAME_SIZE + EMAIL_SIZE;

    public const int PAGE_SIZE = 4096;
    public const int TABLE_MAX_PAGES = 100;
    public const int ROWS_PER_PAGE = PAGE_SIZE / ROW_SIZE;
    public const int TABLE_MAX_ROWS = TABLE_MAX_PAGES * ROWS_PER_PAGE;
}

public enum MetaCommandResult
{
    Exit,
    Success,
    UnrecognizedCommand,
}

public enum PrepareResult
{
    Success,
    SyntaxError,
    UnrecognizedStatement
}

public enum ExecuteResult
{
    Success,
    TableFull,
    CommandNotImplemented
}
