use std::{env, fs};
use std::path::Path;

fn main() {
    //let out_dir = env::var("OUT_DIR").unwrap();

    let profile = env::var("PROFILE").unwrap();
    let target_dir = Path::new("target").join(profile).join("examples").join("resources");

    if target_dir.exists() {
        fs::remove_dir_all(&target_dir).unwrap();
    }
    fs::create_dir_all(&target_dir).unwrap();

    for entry in fs::read_dir("examples/resources").unwrap() {
        let entry = entry.unwrap();
        let from = entry.path();
        let to = target_dir.join(from.file_name().unwrap());
        fs::copy(from, to).unwrap();
    }

    println!("cargo:rerun-if-changed=examples/resources");
}