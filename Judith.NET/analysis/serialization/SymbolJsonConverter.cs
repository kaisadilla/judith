using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.serialization;
public class SymbolJsonConverter : JsonConverter<Symbol> {
    public override void WriteJson (JsonWriter writer, Symbol? value, JsonSerializer serializer) {
        if (value == null) {
            writer.WriteNull();
            return;
        }

        var obj = new JObject {
            ["Table"] = value.Table.TableSymbol.FullyQualifiedName,
            ["Kind"] = JToken.FromObject(value.Kind, serializer),
            ["Name"] = JToken.FromObject(value.Name, serializer),
            ["FullyQualifiedName"] = JToken.FromObject(value.FullyQualifiedName, serializer)
        };

        obj.WriteTo(writer);
    }

    public override Symbol ReadJson (JsonReader reader, Type objectType, Symbol? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        throw new NotImplementedException();
    }
}
