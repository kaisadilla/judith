using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.serialization;
public class SymbolTableJsonConverter : JsonConverter<SymbolTable> {
    public override void WriteJson (JsonWriter writer, SymbolTable? value, JsonSerializer serializer) {
        if (value == null) {
            writer.WriteNull();
            return;
        }

        var obj = new JObject {
            ["OuterTable"] = value.OuterTable?.TableSymbol.FullyQualifiedName, // Store OuterTable as a string
            ["TableSymbol"] = JToken.FromObject(value.TableSymbol, serializer),
            ["InnerTables"] = JToken.FromObject(value.InnerTables, serializer),
            ["Symbols"] = JToken.FromObject(value.Symbols, serializer)
        };

        obj.WriteTo(writer);
    }

    public override SymbolTable ReadJson (JsonReader reader, Type objectType, SymbolTable? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        throw new NotImplementedException("Deserialization requires context to resolve OuterTable, implement if necessary.");
    }
}
