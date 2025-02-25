using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis;

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

        obj.WriteTo(writer);
    }

    public override Symbol ReadJson (JsonReader reader, Type objectType, Symbol? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        throw new NotImplementedException();
    }
}
