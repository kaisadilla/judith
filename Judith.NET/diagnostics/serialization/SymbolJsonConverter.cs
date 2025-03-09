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
        };

        //if (value.Type == value) {
        //    obj["Type"] = "(itself)";
        //}
        //else if (value.Name == "!Undefined") {
        //    obj["Type"] = "=> !Undefined";
        //}
        //else if (value.Name == "!Function") {
        //    obj["Type"] = "=> !Function";
        //}
        //else if (value.Name == "<no-type>") {
        //    obj["Type"] = "=> <no-type>";
        //}
        //else if (value.Name == "<error-type>") {
        //    obj["Type"] = "=> <error-type>";
        //}
        //else if (value.Name == "<anonymous-type>") {
        //    obj["Type"] = "=> <anonymous-type>";
        //}
        //else if (value.Name == "Void") {
        //    obj["Type"] = "=> Void";
        //}
        //else {
        //    obj["Type"] = value.Type != null ? JToken.FromObject(value.Type, serializer) : null;
        //}

        obj["Type"] = value.Type == null ? null : $"=> {value.Type?.FullyQualifiedName}";

        if (value is FunctionSymbol f) {
            obj["Overloads"] = JToken.FromObject(f.Overloads, serializer);
        }
        else if (value is FunctionOverloadSymbol fol) {
            obj["ParamTypes"] = JToken.FromObject(fol.ParamTypes, serializer);
            obj["ReturnType"] = fol.ReturnType == null ? null : JToken.FromObject(fol.ReturnType, serializer);
            obj["IsDuplicate"] = fol.IsDuplicate;
            obj["IsResolved"] = fol.IsResolved();
            obj["Signature"] = fol.GetSignatureString();
        }
        else if (value is TypeSymbol t) {
        }

        obj.WriteTo(writer);
    }

    public override Symbol ReadJson (JsonReader reader, Type objectType, Symbol? existingValue, bool hasExistingValue, JsonSerializer serializer) {
        throw new NotImplementedException();
    }
}
