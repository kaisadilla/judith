use judc::judith::lexical::lexer::{tokenize, Lexer};

fn main () {
    let tokens = tokenize("5eeu2");
    for t in tokens {
        println!("{}, {:?}", t.base().lexeme, t.kind());
    }
}