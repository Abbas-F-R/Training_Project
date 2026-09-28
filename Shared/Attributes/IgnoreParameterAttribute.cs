namespace Training_Project.Shared.Attributes;

/// <summary>
/// Instructs BaseRepository to exclude the decorated property from generated Dapper query parameters.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class IgnoreParameterAttribute : Attribute
{
}
