use std::sync::{Arc, Weak};
use serde::Serialize;

pub struct BoundJudithProgram {

}

#[derive(Debug, Serialize)]
pub struct SymbolTable {
    scope_kind: ScopeKind,

    /// This table's name. This is usually the same as the name of the symbol that originated this
    /// table.
    name: String,

    /// The table that contains this one, if any.
    #[serde(skip_serializing)]
    parent: Option<Weak<SymbolTable>>,

    
}

#[derive(Debug, Serialize, PartialEq)]
pub enum ScopeKind {
    Module,
    Function,
    Block,
}