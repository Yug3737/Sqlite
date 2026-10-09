namespace Sqlite;

public class Table
{
   public int NumberOfRows { get; set; }
   public Pager Pager { get; set; }

   public Table(Pager pager)
   {
      NumberOfRows =(int)(pager.FileLength / Constants.ROW_SIZE);
      Pager = pager;
   }
}