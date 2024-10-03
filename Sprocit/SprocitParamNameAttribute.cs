
namespace Sprocit;

[AttributeUsage(AttributeTargets.Parameter)]
public class SprocitParamNameAttribute : Attribute
{
    public string ParamName { get; }
    public SprocitParamNameAttribute(string paramName)
    {
        ParamName = paramName;
    }
}