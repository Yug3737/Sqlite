using System.Runtime.CompilerServices;

namespace Sqlite;

using System.Buffers.Binary;
using System.Text;
public class Db
{
    public PrepareResult PrepareStatement(string userInput, Statement statement)
    {
        var arguments = userInput.Split(' ');
        var command = arguments[0];
        if (command == "select")
        {
            statement.Type = StatementType.Select;
            return PrepareResult.Success;
        }
        if (command == "insert")
        {
            statement.Type = StatementType.Insert;
            if (arguments.Length < 4) return PrepareResult.SyntaxError;

            var parsedInt = int.Parse(arguments[1]);
            if (parsedInt <= 0)
            {
                Console.WriteLine("Sqlite Error: Id cannot be negative.");
                return PrepareResult.SyntaxError;
            }
            statement.RowToInsert.Id = parsedInt;
            statement.RowToInsert.Username = arguments[2];
            statement.RowToInsert.Email = arguments[3];
            
            // ensure correct byte count for each data type
            // anything that passes int.Parse is a valid int32, so no need to verify the size of integers
            var usernameSize = Encoding.UTF8.GetByteCount(statement.RowToInsert.Username);
            var emailSize = Encoding.UTF8.GetByteCount(statement.RowToInsert.Email);

            if (usernameSize > Constants.USERNAME_SIZE)
            {
                Console.WriteLine($"Username is too long. Given byte count = {usernameSize}. Allowed byte count = {Constants.USERNAME_SIZE}");
                return PrepareResult.SyntaxError;
            }

            if (emailSize > Constants.EMAIL_SIZE)
            {
                Console.WriteLine($"Email is too long. Current byte count = {emailSize}. Allowed byte count = {Constants.EMAIL_SIZE}");
                return PrepareResult.SyntaxError;
            }
            
            return PrepareResult.Success;
        }
        if (command == "update")
        {
            statement.Type = StatementType.Update;
            return PrepareResult.Success;
        }
        if (command == "delete")
        {
            statement.Type = StatementType.Delete;
            return PrepareResult.Success;
        }
        
        return  PrepareResult.UnrecognizedStatement;
    }
    // Virtual Machine
    // Row object to Span<byte>
    public void SerializeRow(Row source, Span<byte> destination)
    {
        var idSlot = destination.Slice(Constants.ID_OFFSET, Constants.ID_SIZE);
        idSlot.Clear(); // sets all bytes to zero, helpful in Deserialization
        BinaryPrimitives.WriteInt32LittleEndian(idSlot, source.Id);
        
        var usernameSlot = destination.Slice(Constants.USERNAME_OFFSET, Constants.USERNAME_SIZE);
        usernameSlot.Clear();
        Encoding.UTF8.GetBytes(source.Username, usernameSlot);

        var emailSlot = destination.Slice(Constants.EMAIL_OFFSET, Constants.EMAIL_SIZE);
        emailSlot.Clear();
        Encoding.UTF8.GetBytes(source.Email, emailSlot);
    }

    // Span<byte> to Row object
    public Row DeserializeRow(Span<byte> source)
    {
        if (source.IsEmpty) 
            throw new InvalidOperationException("Cannot deserialize an empty Span<byte>.");

        var result = new Row();
        // no need to worry about writing less than max allowed bytes for int, since int would always store 4 bytes.
        result.Id = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(Constants.ID_OFFSET, Constants.ID_SIZE));
        
        var usernameSlot = source.Slice(Constants.USERNAME_OFFSET, Constants.USERNAME_SIZE);
        var indexOfFirstZero = usernameSlot.IndexOf((byte)0);
        result.Username = Encoding.UTF8.GetString(
            indexOfFirstZero == -1 ? usernameSlot : usernameSlot[..indexOfFirstZero]);
        
        var emailSlot = source.Slice(Constants.EMAIL_OFFSET, Constants.EMAIL_SIZE);
        indexOfFirstZero = emailSlot.IndexOf((byte)0);
        result.Email = Encoding.UTF8.GetString(
            indexOfFirstZero == -1 ? emailSlot : emailSlot[..indexOfFirstZero]);
        
        return result;
    }

    // Returns the exact memory slot where the new row (represented by rowNumber) needs to be written
    public Span<byte> GetRowSlot(Cursor cursor)
    {
        int pageNumber = cursor.RowNumber / Constants.ROWS_PER_PAGE;
        var page = cursor.Table.Pager.GetPage(pageNumber);
        
        if (page == null)
        {
            // Allocate memory only when we try to access the page
            page = cursor.Table.Pager.Pages[pageNumber] = new byte[Constants.PAGE_SIZE];
        }
        // what is the relative row number of this new row on the page it needs to be written to
        int rowOffset = cursor.RowNumber % Constants.ROWS_PER_PAGE;
        int byteOffset = rowOffset * Constants.ROW_SIZE;
        return page.AsSpan(byteOffset, Constants.ROW_SIZE);
    }

    public ExecuteResult ExecuteStatement(Statement statement, Table table)
    {
        switch (statement.Type)
        {
            case StatementType.Select:
                return ExecuteSelect(statement, table);
            case StatementType.Insert:
                return ExecuteInsert(statement, table);
            case StatementType.Update:
                Console.WriteLine("This is where we do an update.");
                return ExecuteResult.CommandNotImplemented;
            case StatementType.Delete:
                Console.WriteLine("This is where we do a delete.");
                return ExecuteResult.CommandNotImplemented;
        }

        return ExecuteResult.CommandNotImplemented;
    }

    public ExecuteResult ExecuteSelect(Statement statement, Table table)
    {
        var cursor = table.GetTableStartCursor();
        while (!cursor.EndOfTable)
        {
            var row = DeserializeRow(GetRowSlot(cursor));
            row.Print();
            cursor.Advance();
        }

        return ExecuteResult.Success;
    }

    public ExecuteResult ExecuteInsert(Statement statement, Table table)
    {
        if (table.NumberOfRows >= Constants.TABLE_MAX_ROWS)
            return ExecuteResult.TableFull;

        Row rowToInsert = statement.RowToInsert;
        var cursor = table.GetTableEndCursor();
        SerializeRow(rowToInsert, GetRowSlot(cursor));
        ++table.NumberOfRows;

        return ExecuteResult.Success;
    }
    
    // COMPILER FUNCTIONS
    public MetaCommandResult RunMetaCommand(string userInput, Table table)
    {
        if (userInput == ".exit")
        {
            Close(table);
            return  MetaCommandResult.Exit;
        }
        else
        {
            return MetaCommandResult.UnrecognizedCommand;
        }
    }
    
    // Part 5 functions
    /// <summary>
    /// Initializes the Pager, connect the pager to file, initialize Table and return the Table
    /// </summary>
    public Table Open(string fileName)
    {
        Pager pager = new Pager(fileName);
        Table table = new Table(pager);
        return table;
    }

    /// <summary>
    /// Flush Page cache to disk, close db file, free memory for Pager and Table
    /// </summary>
    public void Close(Table table)
    {
        var pager = table.Pager;
        int fullPageCount = table.NumberOfRows / Constants.ROWS_PER_PAGE;
        
        // Empty the pager
        for (int i = 0; i < fullPageCount; ++i)
        {
            if (pager.Pages[i] == null) continue;
            pager.Flush(i, Constants.PAGE_SIZE);
            pager.Pages[i] = null;
        }
        
        // Handle the partial page which may be present in the end
        int additionalRowCount = table.NumberOfRows % Constants.ROWS_PER_PAGE;
        if (additionalRowCount > 0)
        {
            int pageNumber = fullPageCount;
            if (pager.Pages[pageNumber] != null)
            {
                pager.Flush(pageNumber, additionalRowCount * Constants.ROW_SIZE);
                pager.Pages[pageNumber] = null;
            }
        }

        pager.Dispose();
        // Since the table only consists of the pager, and we already nulled all pages in table.Pager, at this point we 
        // have made both table.Pager and table obsolete
    }
}
