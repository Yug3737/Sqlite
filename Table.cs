namespace Sqlite;

public class Table
{
   public int NumberOfRows { get; set; }
   /// <summary>
   /// Pages is an array of nullable rows. Rows themselves are just byte arrays. Pages are created only when necessary.
   /// </summary>
   public byte[]?[] Pages { get; set; } = new byte[Constants.TABLE_MAX_PAGES][];
}