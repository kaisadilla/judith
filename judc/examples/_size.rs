use judc::judith::lexical::token::Token;
use judc::judith::syntax::nodes::Identifier;
use judc::SourceSpan;

fn main() {
    println!("size: {} bytes", size_of::<SourceSpan>());
}