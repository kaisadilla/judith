use serde::Serialize;

pub mod judith;

#[derive(Debug, Clone, Copy, Serialize)]
pub struct SourceSpan {
    pub start: i64,
    pub end: i64,
    pub line: i64,
}

impl SourceSpan {
    pub fn new(start: i64, end: i64, line: i64) -> SourceSpan {
        SourceSpan { start, end, line }
    }

    pub fn no_location() -> SourceSpan {
        SourceSpan { start: -1, end: -1, line: -1 }
    }

    pub fn length(&self) -> i64 {
        self.end - self.start
    }
}
