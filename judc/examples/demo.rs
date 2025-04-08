use judc::judith::lexical::lexer::{tokenize, Lexer};

fn main () {
    let res = tokenize("5.3.1");
    for t in res.tokens {
        println!("{}, {:?}", t.base().lexeme, t.kind());
    }

    for m in res.messages.all_messages() {
        println!("{}", m.get_elaborate_message(None));
        println!("Code: {}", m.code.i32());
    }
}