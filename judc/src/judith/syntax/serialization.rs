use serde::{Serialize, Serializer};
use serde::ser::SerializeMap;
use crate::judith::syntax::nodes::Expr;

impl Serialize for Expr {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: Serializer
    {
        match self {
            Expr::Assignment(expr) => custom_serial(serializer, "AssignmentExpr", expr.as_ref()),
            Expr::Binary(expr) => custom_serial(serializer, "BinaryExpr", expr.as_ref()),
            Expr::Group(expr) => custom_serial(serializer, "GroupExpr", expr.as_ref()),
            Expr::Identifier(expr) => custom_serial(serializer, "IdentifierExpr", expr.as_ref()),
            Expr::Literal(expr) => custom_serial(serializer, "LiteralExpr", expr.as_ref()),
        }
    }
}

fn custom_serial<S, T> (serializer: S, name: &str, content: T) -> Result<S::Ok, S::Error>
where
    S: Serializer, T: Serialize
{
    let mut map = serializer.serialize_map(Some(1))?;
    map.serialize_entry(name, &content)?;
    map.end()
}