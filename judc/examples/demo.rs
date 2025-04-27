use std::{env, fs};
use std::fs::File;
use std::io::Write;
use std::path::Path;
use serde_json::ser;
use judc::judith::compiler_messages::MessageContainer;
use judc::judith::lexical::lexer::{tokenize, Lexer};
use judc::judith::syntax::parser::{parse, ParseAttempt, Parser};

fn __main2__() {
    let exe_path = env::current_exe().unwrap();
    let run_path = exe_path.parent().unwrap();
    let res_path = run_path.join("resources").clone();
    let out_path = run_path.join(".out").clone();

    //let lexer_res = tokenize("mut sS (Num) -> Num?");
    let lexer_res = tokenize("Num?[5] | String & ISend");
    let mut parser = Parser::new(&lexer_res.tokens);
    let ParseAttempt::Ok(ty) = parser.parse_type() else { panic!("Invalid type???") };

    let ty_json = serde_json::to_string_pretty(&ty).unwrap();

    let mut file = File::create(out_path.join("ast.json")).unwrap();
    file.write_all(ty_json.as_bytes()).unwrap();
}

fn main() {
    let exe_path = env::current_exe().unwrap();
    let run_path = exe_path.parent().unwrap();
    let res_path = run_path.join("resources").clone();
    let out_path = run_path.join(".out").clone();

    let src = fs::read_to_string(res_path.join("test.jud")).unwrap();
    let mut messages = MessageContainer::new();

    // 1. Build AST
    // 1.1 Tokenize
    let lexer_res = tokenize(&src);
    let token_json = serde_json::to_string_pretty(&lexer_res.tokens).unwrap();

    if lexer_res.messages.count() != 0 { return; }

    // 1.2 Parse
    let parser_res = parse(lexer_res.tokens);
    let ast_json = serde_json::to_string_pretty(&parser_res.nodes).unwrap();

    messages.add_all(lexer_res.messages);
    messages.add_all(parser_res.messages);
    let msg_json = serde_json::to_string_pretty(&messages).unwrap();

    // DEBUG FILES:
    if Path::new(&out_path).exists() == false {
        let res = fs::create_dir_all(&out_path);
        if res.is_err() {
            println!("Couldn't create directory '{}'", out_path.display().to_string());
            return;
        }
    }

    println!("Output path: {}", out_path.display().to_string());
    let mut file = File::create(out_path.join("tokens.json")).unwrap();
    file.write_all(token_json.as_bytes()).unwrap();

    let mut file = File::create(out_path.join("ast.json")).unwrap();
    file.write_all(ast_json.as_bytes()).unwrap();

    let mut file = File::create(out_path.join("messages.json")).unwrap();
    file.write_all(msg_json.as_bytes()).unwrap();
}
