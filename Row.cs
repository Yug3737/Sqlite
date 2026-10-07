using System.Runtime.InteropServices;

namespace Sqlite;

public class Row
{
    public int Id;
    public string Username;
    public string Email;

    public override string ToString()
    {
        return $"Id: {Id}, Username: {Username}, Email: {Email}";
    }
    public void Print()
    {
        Console.WriteLine(ToString());
    }
}