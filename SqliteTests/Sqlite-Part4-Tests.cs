using System.Diagnostics;
using Sqlite;
namespace SqliteTests;

[TestFixture]
public class Tests
{
	private ProcessStartInfo _startInfo;
	
    [SetUp]
    public void Setup()
    {
	    _startInfo = new ProcessStartInfo
	    {
		    FileName = "dotnet",
		    ArgumentList = {"run", "--project", "C:\\Users\\Yug Patel\\RiderProjects\\Sqlite\\Sqlite.csproj", "--no-build"},
		    UseShellExecute = false,
		    RedirectStandardInput = true,
		    RedirectStandardOutput = true,
		    RedirectStandardError = true,
	    };
    }

    [Test]
    public void InsertAndSelectRow()
    {
	    var runtimeDir = AppContext.BaseDirectory;
		using Process? process = Process.Start(_startInfo);
		
		// Here StandardInput is a writer because we are the ones writing to it
		process.StandardInput.WriteLine("insert 1 foo bar@gmail.com");
		process.StandardInput.WriteLine("select");
		
		process.StandardInput.Close();
		
		var output = process.StandardOutput.ReadToEnd();
		var errors = process.StandardError.ReadToEnd();
		TestContext.Out.WriteLine(errors);
		TestContext.Out.WriteLine(process.StandardError.ReadToEnd());
		
		Assert.That(output, Does.Contain("Executed."));
		Assert.That(output, Does.Contain("Id: 1, Username: foo, Email: bar@gmail.com"));
		
		// StandardOutput and StandardError are closed automatically with the using keyword
		process.WaitForExit();
    }

    [Test]
    public void AllowMaxLengthStrings()
    {
	    using Process? process = Process.Start(_startInfo);

	    var longUsername = String.Concat(Enumerable.Repeat("A", Constants.USERNAME_SIZE));
	    var longEmail = String.Concat(Enumerable.Repeat("B", Constants.EMAIL_SIZE));
	    process.StandardInput.WriteLine($"insert 1 {longUsername} {longEmail}");
	    process.StandardInput.WriteLine("select");

	    process.StandardInput.Close();
	    
	    var output = process.StandardOutput.ReadToEnd();
	    var errors = process.StandardError.ReadToEnd();
	    TestContext.Out.WriteLine(errors);
	    TestContext.Out.WriteLine(process.StandardError.ReadToEnd());
	    
	    Assert.That(output, Does.Contain("Executed."));
	    Assert.That(output, Does.Not.Contain("too long"));
		
	    process.WaitForExit();
    }
    
    [Test]
    public void ErrorWhenStringsTooLong()
    {
	    using Process? process = Process.Start(_startInfo);

	    var longUsername = String.Concat(Enumerable.Repeat("A", Constants.USERNAME_SIZE + 1));
	    var longEmail = String.Concat(Enumerable.Repeat("B", Constants.EMAIL_SIZE + 1));
	    process.StandardInput.WriteLine($"insert 1 {longUsername} {longEmail}");
	    process.StandardInput.WriteLine("select");

	    process.StandardInput.Close();
	    
	    var output = process.StandardOutput.ReadToEnd();
	    var errors = process.StandardError.ReadToEnd();
	    TestContext.Out.WriteLine(errors);
	    TestContext.Out.WriteLine(process.StandardError.ReadToEnd());
	    
	    Assert.That(output, Does.Contain("Username is too long."));
		
	    process.WaitForExit();
    }

    [Test]
    public void ErrorWhenIdNegative()
    {
	    using Process? process = Process.Start(_startInfo);
	    process.StandardInput.WriteLine("insert -11 foo bar@bario.com");
		process.StandardInput.WriteLine("select");
		
		process.StandardInput.Close();
		
		var output = process.StandardOutput.ReadToEnd();
		// var errors = process.StandardError.ReadToEnd();
		// TestContext.Out.WriteLine(errors);
		// TestContext.Out.WriteLine(process.StandardError.ReadToEnd());
		
		Assert.That(output, Does.Contain("Sqlite Error: Id cannot be negative."));
		
		process.WaitForExit();
    }
    
    // Add Test 5
	//ErrorWhenTableFull

	[Test]
	public void KeepDataAfterClosingConnection()
	{
		using Process? process = Process.Start(_startInfo);
		process.StandardInput.WriteLine("insert 1 user1 person1@gmail.com");
		process.StandardInput.WriteLine(".exit");
		process.StandardInput.Close();
		
		var output = process.StandardOutput.ReadToEnd();
		var errors = process.StandardError.ReadToEnd();
		TestContext.Out.WriteLine(errors);
		TestContext.Out.WriteLine(process.StandardError.ReadToEnd());
		
		Assert.That(output, Does.Contain("Executed."));
		process.WaitForExit();
		
		using Process? process2 = Process.Start(_startInfo);
		process2.StandardInput.WriteLine("select");
		process2.StandardInput.WriteLine(".exit");
		
		var output2 = process.StandardOutput.ReadToEnd();
		var errors2 = process.StandardError.ReadToEnd();
		TestContext.Out.WriteLine(errors);
		TestContext.Out.WriteLine(process.StandardError.ReadToEnd());
		
		Assert.That(output2, Does.Contain("Id: 1, Username: user1, Email: person1@gmail.com"));
	}
}
