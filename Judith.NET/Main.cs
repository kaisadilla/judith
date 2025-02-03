// See https://aka.ms/new-console-template for more information
using Judith.NET;
using Judith.NET.message;
using Newtonsoft.Json;

Console.WriteLine("> judith test.judith");
Console.WriteLine();

string src = File.ReadAllText(AppContext.BaseDirectory + "/res/test.judith");

MessageContainer messages = new();
Lexer lexer = new(src, messages);

lexer.Tokenize();

foreach (var token in lexer.Tokens!) {
    Console.WriteLine(token);
}
Console.WriteLine();

Console.WriteLine($"Errors: {messages.Errors.Count} ---");
foreach (var error in messages.Errors) {
    Console.WriteLine(error);
}

Parser parser = new(lexer.Tokens, messages);
parser.Parse();
string astJson = JsonConvert.SerializeObject(parser.Nodes, Formatting.Indented);

Console.WriteLine();
Console.WriteLine("=== AST ===");
//Console.WriteLine(astJson);
foreach (var node in parser.Nodes!) {
    Console.WriteLine(node);
}

File.WriteAllText(AppContext.BaseDirectory + "/res/test.ast.json", astJson);