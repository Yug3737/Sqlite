namespace Sqlite;

/// <summary>
/// The pager would be asked for page number X and it gives back a block of memory(page).
/// Page numbers go from 0 until Constants.TABLE_MAX_PAGES - 1
/// </summary>
public class Pager : IDisposable
{
    /// <summary>
    /// Pages is an array of nullable rows. Rows themselves are just byte arrays. Pages are created only when necessary.
    /// This structure also represents the page cache / how the pages are stored in-memory while the DB session is running.
    /// </summary>
    public byte[]?[] Pages { get; set; } = new byte[Constants.TABLE_MAX_PAGES][];
    
    public long FileLength => DbFileStream.Length;

    public FileStream DbFileStream { get; }

    /// <summary>
    /// Initializing a Pager would automatically connect the FileStream to given fileName
    /// </summary>
    public Pager(string fileName)
    {
        DbFileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
    }

    public void Dispose()
    {
        DbFileStream.Dispose();
    }

    public byte[] GetPage(int pageNumber)
    {
        if (pageNumber >= Constants.TABLE_MAX_PAGES)
        {
            Console.WriteLine($"Tried to fetch page number out of bounds {pageNumber} >= {Constants.TABLE_MAX_PAGES}");
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (Pages[pageNumber] == null) // Cache miss, Page has not been initialized yet
        {
            var newPage = new byte[Constants.PAGE_SIZE];
            int currPageCount = (int)(DbFileStream.Length / Constants.PAGE_SIZE);
            
            // may save a partial page at end of file
            if (DbFileStream.Length % Constants.PAGE_SIZE != 0) currPageCount += 1;

            if (pageNumber < currPageCount) // Does this page exist on disk?
            {
                int offset = pageNumber * Constants.PAGE_SIZE;
                DbFileStream.Seek(offset, SeekOrigin.Begin);
                DbFileStream.Read(newPage, 0, Constants.PAGE_SIZE); // here 0 is the offset in the buffer where read bytes should be placed starting from
            }
            else 
            {
                // brand new page to load, nothing to read
                // Do nothing here, just return the empty page
            }

            Pages[pageNumber] = newPage;
        }

        return Pages[pageNumber]; // Cache hit/ The page the user was looking for was already created in the Pages array
    }

    /// <summary>
    /// Persist the bytes in the given pageNumber to disk
    /// </summary>
    /// <param name="pageNumber">Page Number to flush</param>
    /// <param name="size">Number of bytes needing to be flushed</param>
    public void Flush(int pageNumber, int size)
    {
        if (Pages[pageNumber] == null)
        {
            Console.WriteLine($"Trying to flush a null page. Tried to flush Page number {pageNumber}");
            return;
        }

        int offset = pageNumber * Constants.PAGE_SIZE;
        DbFileStream.Seek(offset, SeekOrigin.Begin);
        DbFileStream.Write(Pages[pageNumber], 0, size);
        DbFileStream.Flush();
    }
}