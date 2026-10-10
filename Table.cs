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
   public Cursor GetTableStartCursor()
   {
      return new Cursor(this);
   }

   public Cursor GetTableEndCursor()
   {
      return new Cursor(this, this.NumberOfRows, true);
   }
}