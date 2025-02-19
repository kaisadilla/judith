using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Judith.NET.analysis;

namespace Judith.NET.diagnostics.serialization;
public class SymbolTableJsonConverter : JsonConverter<SymbolTable> {
    public override void WriteJson (JsonWriter writer, SymbolTable? value, JsonSerializer serializer) {
        if (value == null) {
            writer.WriteNull();
            return;
        }

        var obj = new JObject {
            [nameof(SymbolTable.OuterTable)] = value.OuterTable?.Qualifier, // Store OuterTable as a string
            [nameof(SymbolTable.TableSymbol)] = value.TableSymbol != null ? JToken.FromObject(value.TableSymbol, serializer) : null,
            [nameof(SymbolTable.InnerTables)] = JToken.FromObject(value.InnerTables, serializer),
            [nameof(SymbolTable.AnonymousInnerTables)] = JToken.FromObject(value.AnonymousInnerTables, serializer),
            [nameof(SymbolTable.Symbols)] = JToken.FromObject(value.Symbols, serializer)
        };

        obj.WriteTo(writer);
    }

    public override SymbolTable ReadJson (JsonReader reader, Type objectType, SymbolTable? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        throw new NotImplementedException("Deserialization requires context to resolve OuterTable, implement if necessary.");
    }
}
