// file: Program.cs
// author: Yug Patel
// last modified: 12 July 2026

using Sqlite;

// Start MAIN LOOP
var ci = new ConsoleInteractor();
var db = new Db();

var table = new Table();
var running = true;
while (running)
{
    ci.PrintPrompt();
    var userInput = ci.ReadInput();
    if (string.IsNullOrEmpty(userInput))
    {
        break;
    }

    // Handle Meta commands first
    if (userInput.StartsWith('.'))
    {
        switch (db.RunMetaCommand(userInput))
        {
            case MetaCommandResult.Exit:
                running = false;
                break; // from the current switch block
            case MetaCommandResult.Success:
                continue; 
            case MetaCommandResult.UnrecognizedCommand:
                Console.WriteLine($"Unrecognized command '{userInput}'.");
                continue;
        }
    }
    else // handle CRUD commands
    {
        Statement statement = new Statement();
        switch (db.PrepareStatement(userInput, statement)) // passing statement(object) in the method(by value) would modify the same object
        {
            case PrepareResult.Success:
                break;
            case PrepareResult.SyntaxError:
                continue;
            case PrepareResult.UnrecognizedStatement:
                Console.WriteLine($"Unrecognized keyword at start of '{userInput}'.");
                continue;
        }

        var result = db.ExecuteStatement(statement, table);
        switch (result)
        {
            case ExecuteResult.Success:
                Console.WriteLine("Executed.");
                continue;
            case ExecuteResult.TableFull:
                Console.WriteLine("Table is full. Free up some memory first.");
                continue;
        }

    }
}

ci.PrintExitMessage();
// END MAIN LOOP


// Console.WriteLine(AppContext.BaseDirectory);