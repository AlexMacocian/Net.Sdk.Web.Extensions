namespace Net.Sdk.Web.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class OptionsNameAttribute : Attribute
{
    public string? Name { get; set; }
}