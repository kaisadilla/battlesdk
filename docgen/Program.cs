global using static battlesdk.types.TypesUtils;
using docgen;

Console.WriteLine("Generating Lua API documentation.");

var docPath = Path.Combine(AppContext.BaseDirectory, ".api.d.lua");

LuaDocGenerator gen = new(@"E:\repos\battlesdk\battlesdk\doc.xml");
gen.GenerateTypesFile(@"E:\repos\battlesdk\battlesdk\res\scripts");
