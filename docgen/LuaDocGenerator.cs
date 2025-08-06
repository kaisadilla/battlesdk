using battlesdk.scripts;
using battlesdk.scripts.types;
using MoonSharp.Interpreter;
using System.Reflection;
using System.Xml.Linq;

namespace docgen;
internal class LuaDocGenerator {
    private readonly Dictionary<string, XElement>? _xmlDocs;

    public LuaDocGenerator (string xmlPath) {
        if (File.Exists(xmlPath) == false) {
            return;
        }

        var doc = XDocument.Load(xmlPath);
        _xmlDocs = doc.Descendants("member")
            .Where(m => m.Attribute("name") != null)
            .ToDictionary(
                m => m.Attribute("name")!.Value,
                m => m
            );

        Lua.Init();
    }

    public void GenerateTypesFile (string folder) {
        var docPath = Path.Combine(folder, ".api.types.d.lua");

        using var writer = new StreamWriter(docPath);

        var types = typeof(Lua).Assembly
            .GetTypes()
            .Where(t => t.GetCustomAttribute<LuaApiClassAttribute>() is not null)
            .OrderBy(t => t.Name);

        foreach (var t in types) {
            DescribeClass(writer, t);
        }
    }

    private void DescribeClass (StreamWriter writer, Type type) {
        var className = GetLuaClassName(type);

        var fields = type.GetFields();
        var props = type.GetProperties();
        var methods = type.GetMethods();

        writer.WriteLine($"-- #region {className}");
        writer.WriteLine($"---@class {className}");
        for (int i = 0; i < fields.Length; i++) {
            var field = fields[i];
            if (field.IsPublic == false) continue;
            if (field.GetCustomAttribute<MoonSharpHiddenAttribute>() is not null) {
                continue;
            }

            DescribeField(writer, className, field);
        }
        for (int i = 0; i < props.Length; i++) {
            var prop = props[i];
            if (prop.GetGetMethod()?.IsPublic != true) continue;
            if (prop.GetCustomAttribute<MoonSharpHiddenAttribute>() is not null) {
                continue;
            }

            DescribeProperty(writer, className, prop);
        }
        writer.WriteLine($"{className} = {{}}");



        for (int i = 0; i < methods.Length; i++) {
            var method = methods[i];
            if (method.IsPublic == false) continue;
            if (method.GetCustomAttribute<MoonSharpHiddenAttribute>() is not null) {
                continue;
            }
            if (method.Name == "Equals") continue;
            if (method.Name == "GetHashCode") continue;
            if (method.Name == "GetType") continue;

            writer.WriteLine();
            DescribeMethod(writer, className, method);
        }

        writer.WriteLine($"-- #endregion {className}");
        writer.WriteLine();
    }

    private void DescribeField (
        StreamWriter writer, string className, FieldInfo field
    ) {
        var name = field.Name.ToSnakeCase();
        var xmlDoc = GetSummary(field);
        string? summary = xmlDoc?.Element("summary")?.Value?.Trim();

        writer.Write($"---@field {name} {GetLuaType(field.FieldType)}");

        if (summary is not null) {
            writer.WriteLine($" -- {summary.Replace('\n', ' ')}");
        }
        else {
            writer.WriteLine();
        }
    }

    private void DescribeProperty (
        StreamWriter writer, string className, PropertyInfo prop
    ) {
        var name = prop.Name.ToSnakeCase();
        var xmlDoc = GetSummary(prop);
        string? summary = xmlDoc?.Element("summary")?.Value?.Trim();

        writer.Write($"---@field {name} {GetLuaType(prop.PropertyType)}");

        if (summary is not null) {
            writer.WriteLine($" -- {summary.Replace('\n', ' ')}");
        }
        else {
            writer.WriteLine();
        }
    }

    private void DescribeMethod (
        StreamWriter writer, string className, MethodInfo method
    ) {
        var name = method.Name.ToSnakeCase();
        var xmlDoc = GetSummary(method);
        string? summary = xmlDoc?.Element("summary")?.Value?.Trim();

        if (summary is not null) {
            foreach (var l in summary.Split('\n')) {
                writer.WriteLine($"---{l.Trim()}");
            }
        }

        var paramObjs = method.GetParameters();
        foreach (var param in paramObjs) {
            string? paramDoc = xmlDoc
                ?.Elements("param")
                .FirstOrDefault(p => p.Attribute("name")?.Value == param.Name)
                ?.Value?.Trim();

            writer.WriteLine(
                $"---@param {param.Name} {GetLuaType(param.ParameterType)} {paramDoc ?? ""}"
            );
        }

        if (method.ReturnType != typeof(void)) {
            writer.WriteLine($"---@return {GetLuaType(method.ReturnType)}");
        }

        writer.Write($"function {className}.{name} (");

        for (int i = 0; i < paramObjs.Length; i++) {
            var param = paramObjs[i];

            writer.Write(param.Name);

            if (i < paramObjs.Length - 1) {
                writer.Write(", ");
            }
        }

        writer.WriteLine(") end");
    }

    private string GetLuaType (Type paramType) {
        bool nullable = false;

        if (Nullable.GetUnderlyingType(paramType) is Type underlying) {
            paramType = underlying;
            nullable = true;
        }

        if (paramType == typeof(int)) return "number" + (nullable ? "?" : "");
        if (paramType == typeof(float)) return "number" + (nullable ? "?" : "");
        if (paramType == typeof(double)) return "number" + (nullable ? "?" : "");
        if (paramType == typeof(string)) return "string" + (nullable ? "?" : "");
        if (paramType == typeof(bool)) return "boolean" + (nullable ? "?" : "");
        if (paramType == typeof(Table)) return "table" + (nullable ? "?" : "");
        if (paramType == typeof(DynValue)) return "any" + (nullable ? "?" : "");
        if (paramType == typeof(void)) return "void" + (nullable ? "?" : "");

        if (paramType.IsArray) {
            var elType = paramType.GetElementType();
            return (elType is null ? "any[]" : $"{GetLuaType(elType!)}[]") + (nullable ? "?" : "");
        }

        var attr = paramType.GetCustomAttribute<LuaApiClassAttribute>();
        if (attr is not null) {
            return GetLuaClassName(paramType) + (nullable ? "?" : "");
        }

        return "unknown" + (nullable ? "?" : "");
    }

    private XElement? GetSummary (MemberInfo member) {
        if (_xmlDocs is null) return null;

        var key = GetMemberKey(member);
        if (key is null) return null;

        return _xmlDocs.TryGetValue(key, out var sum) ? sum : null;
    }

    private static string? GetMemberKey (MemberInfo member) {
        if (member is MethodInfo method) {
            var par = method.GetParameters();
            var parTypes = par.Length > 0
                ? $"({string.Join(',', par.Select(p => GetTypeName(p.ParameterType)))})"
                : "";

            return $"M:{method.DeclaringType?.FullName}.{method.Name}{parTypes}";
        }

        if (member is Type type) {
            return $"T:{type.FullName}";
        }
        if (member is PropertyInfo prop) {
            return $"P:{prop.DeclaringType?.FullName}.{prop.Name}";
        }
        if (member is FieldInfo field) {
            return $"P:{field.DeclaringType?.FullName}.{field.Name}";
        }

        return null;
    }

    private static string GetTypeName (Type type) {
        if (type.IsGenericType) {
            var genericArgs = string.Join(",", type.GetGenericArguments().Select(GetTypeName));
            var baseName = type.FullName?.Split('`')[0];
            return $"{baseName}{{{genericArgs}}}";
        }
        return type.FullName ?? type.Name;
    }

    private static string GetLuaClassName (Type type) {
        return type.GetField(nameof(LuaRenderer.CLASSNAME))
            ?.GetValue(null) as string
            ?? type.Name;
    }
}
