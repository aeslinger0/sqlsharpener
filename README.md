# SqlSharpener
Rather than generating code from the database or using a heavy abstraction layer that might miss differences between the database and data access layer until run-time, this project aims to provide a very fast and simple data access layer that is generated at **design-time** using SQL ___files___ as the source-of-truth (such as those found in an SSDT project) without having to deploy the database first.

SqlSharpener accomplishes this by parsing the SQL files to create a meta-object hierarchy with which you can generate C# code such as stored procedure wrappers or Entity Framework Code-First entities. You can do this manually or by invoking one of the included pre-compiled T4 templates.

# Donate

If you find SqlSharpener to be useful, please consider making a [donation via PayPay](https://paypal.me/adam0101). Thank you.

# Examples

See examples of what SqlSharpener can do on the [Examples wiki page](https://github.com/aeslinger0/sqlsharpener/wiki/Examples). Also, a solution with the examples [is included with the code for you to download](https://github.com/aeslinger0/sqlsharpener/tree/master/examples/SimpleExample).

# Performance

According to the performance tests from [Dapper](https://github.com/StackExchange/dapper-dot-net#performance-of-select-mapping-over-500-iterations---poco-serialization), the best performance came from hand-coded functions using SqlDataReader. SqlSharpener gives you that performance without needing to hand-code your functions.

# Installation

[Using NuGet](https://www.nuget.org/packages/SqlSharpener/), run the following command to install SqlSharpener:

    PM> Install-Package SqlSharpener
    
This will add SqlSharpener as a solution-level package. That means that the dll's do not get added to any of your projects (nor should they). 

# Quick Start

See the [Quick Start Guide](https://github.com/aeslinger0/sqlsharpener/wiki/Quick-Start-Guide) for more examples.

The fastest way to get up and running is to call one of SqlSharpener's included pre-compiled templates from your template. Add a new T4 template (\*.tt) file to your data project and set its content as follows: *(Ensure you have the correct version number in the dll path)*

````c#   
    
    <#@ template debug="false" hostspecific="true" language="C#" #>
    <#@ assembly name="$(SolutionDir)\packages\SqlSharpener.1.0.2\tools\SqlSharpener.dll" #>
    <#@ output extension=".cs" #>
    <#@ import namespace="Microsoft.VisualStudio.TextTemplating" #>
    <#@ import namespace="System.Collections.Generic" #>
    <#@ import namespace="SqlSharpener" #>
    <#
   	// Specify paths to your *.sql files. Remember to include your tables as well! We need them to get the data types.
    	var sqlPaths = new List<string>();
    	sqlPaths.Add(Host.ResolvePath(@"..\SimpleExample.Database\dbo\Tables"));
    	sqlPaths.Add(Host.ResolvePath(@"..\SimpleExample.Database\dbo\Stored Procedures"));
    
    	// Set parameters for the template.
    	var session = new TextTemplatingSession();
    	session["outputNamespace"] = "SimpleExample.DataLayer";
    	session["procedurePrefix"] = "usp_";
    	session["sqlPaths"] = sqlPaths;
    
    	// Generate the code.
    	var t = new SqlSharpener.StoredProceduresTemplate();
        t.Session = session;
    	t.Initialize();
    	this.Write(t.TransformText());
    #>
````

The generated .cs file will contain a class with functions for all your stored procedures, DTO objects for procedures that return records, and an interface you can used if you use dependency-injection. Whenever your database project changes, simply right-click on the .tt file and click "Run Custom Tool" to regenerate the code.

# Usage
````c#
 // Once the code is generated, your business layer can call it like any other function. Here is one example:

 public TaskGetDto Get(int id)
 {
     return storedProcedures.TaskGet(id);
 }
````        
# Dependency Injection

If you use a dependency-injection framework such as Ninject, you can use the interface generated. For example:

````c#
  public class  DataModule : NinjectModule
  {
     public override void Load()
     {
        Bind<IStoredProcedures>().To<StoredProcedures>();
      }
  }
````    
# Documentation

Check out the [wiki](https://github.com/aeslinger0/sqlsharpener/wiki) for more info.
    
# License

SqlSharpener uses The MIT License (MIT), but also has dependencies on DacFx and ScriptDom. I have included their license info in the root directory.
