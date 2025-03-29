use judc::judith::lexical::lexer::{tokenize, Lexer};

fn main () {
    let mut lexer = Lexer::new("- !");
    lexer.next_token();
}