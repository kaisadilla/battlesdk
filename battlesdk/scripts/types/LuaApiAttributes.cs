namespace battlesdk.scripts.types;
/// <summary>
/// Marks a class as a Lua class. 
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class LuaApiClassAttribute : Attribute {
}

/// <summary>
/// Marks a method as a Lua coroutine. These methods's parameters will be
/// documented through their <see cref="LuaApiFunctionParamAttribute"/>
/// annotations.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class LuaApiCoroutineAttribute : Attribute {
}

/// <summary>
/// Describes one parameter from a function marked with
/// <see cref="LuaApiCoroutineAttribute"/>.
/// </summary>
/// <param name="index">The index of this parameter.</param>
/// <param name="name">The parameter's name.</param>
/// <param name="type">The parameter's (C#) type.</param>
/// <param name="description">The parameter's description.</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class LuaApiFunctionParamAttribute(
    int index, string name, Type type, string? description = null
) : Attribute {
    public int Index { get; } = index;
    public string Name { get; } = name;
    public Type Type { get; } = type;
    public string? Description { get; } = description;
}

/// <summary>
/// Describes the return value of a function marked with
/// <see cref="LuaApiCoroutineAttribute"/>.
/// </summary>
/// <param name="type">The return value's (C#) type.</param>
/// <param name="description">The return value's description.</param>
[AttributeUsage(AttributeTargets.Method)]
public class LuaApiFunctionReturnValueAttribute(
    Type type, string? description = null
) : Attribute {
    public Type Type { get; } = type;
    public string? Description { get; } = description;
}
