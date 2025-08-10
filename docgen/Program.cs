global using static battlesdk.types.TypesExtension;
using docgen;

Console.WriteLine("Generating Lua API documentation.");

LuaDocGenerator gen = new(@"E:\repos\battlesdk\battlesdk\doc.xml"); // TODO: Yeah, these are hardcoded, that'll be corrected eventually.
gen.GenerateTypesFile(@"E:\repos\battlesdk\battlesdk\res\scripts"); // TODO: Yeah, these are hardcoded, that'll be corrected eventually.
