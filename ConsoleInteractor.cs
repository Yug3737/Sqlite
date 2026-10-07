namespace Sqlite;

// CONSOLE INTERACTION

public class ConsoleInteractor
{
    public void PrintPrompt()
    {
        Console.Write("db> ");
    }

    public void PrintExitMessage()
    {
        Console.WriteLine("Thank you for using Sqlite C#.");
    }
    
    /// <summary>
    /// A null return value means there was nothing to read for Console.ReadLine()
    /// </summary>
    public string? ReadInput()
    {
        return Console.ReadLine();
    }
    
}
