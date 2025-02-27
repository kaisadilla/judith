using Judith.NET.analysis.semantics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Judith.NET.diagnostics.serialization;
public class SymbolJsonConverter : JsonConverter<Symbol> {
    public override void WriteJson (JsonWriter writer, Symbol? value, JsonSerializer serializer) {
        if (value == null) {
            writer.WriteNull();
            return;
        }

        var obj = new JObject {
            ["Table"] = value.Table.Qualifier,
            ["Kind"] = JToken.FromObject(value.Kind, serializer),
            ["Name"] = JToken.FromObject(value.Name, serializer),
            ["FullyQualifiedName"] = JToken.FromObject(value.FullyQualifiedName, serializer),
            ["Type"] = value.Type != null ? JToken.FromObject(value.Type, serializer) : null,
        };

        if (value is FunctionSymbol funcSymbol) {
            obj["Overloads"] = JToken.FromObject(funcSymbol.Overloads, serializer);
        }
        else if (value is TypeSymbol typeSymbol) {
        }

        obj.WriteTo(writer);
    }

    public override Symbol ReadJson (JsonReader reader, Type objectType, Symbol? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        throw new NotImplementedException();
    }
}
