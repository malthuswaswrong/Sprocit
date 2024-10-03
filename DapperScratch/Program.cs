using Sprocit.Extensions.YourClassNameGoesHere;
using System.Data.SqlClient;

SqlConnection connection = new SqlConnection(@"Server=(localdb)\ProjectModels;Database=Sprocit;Integrated Security=True;");

IMySprocitTest testClass = new MySprocitTest(connection);
string? message;
message = testClass.RunHelloProcedure("Mike");
Console.WriteLine(message);
message = testClass.RunHelloProcedure("");
if(message == null)
{
    Console.WriteLine("Null message returned successfully");
}
else
{
    Console.WriteLine("Null expected but not returned");
}
try
{
    message = testClass.RunHelloProcedure(null);
    Console.WriteLine("Exception should have been thrown bad result");
}
catch (System.Exception)
{
    Console.WriteLine("Exception thrown as expected");
}