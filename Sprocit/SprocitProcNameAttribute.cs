
namespace Sprocit;

[AttributeUsage(AttributeTargets.Method)]
public class SprocitProcNameAttribute : Attribute
{
    public string ProcedureName { get; }
    public SprocitProcNameAttribute(string procedureName)
    {
        ProcedureName = procedureName;
    }
}