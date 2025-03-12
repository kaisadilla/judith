﻿using Judith.NET.analysis.syntax;
using Judith.NET.message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judith.NET.analysis.analyzers;

using RetInfo = (bool hasReturn, bool hasYield);

internal class ImplicitNodeAnalyzer : SyntaxVisitor<RetInfo?> {
    public MessageContainer Messages { get; private set; } = new();

    private readonly Compilation _cmp;
    private readonly ScopeResolver _scope;

    private Stack<bool> _returnRequiredStack = [];
    private Stack<bool> _yieldRequiredStack = [];
    private Stack<bool> _returnAllowedStack = [];
    private Stack<bool> _yieldAllowedStack = [];

    private bool ReturnRequired => _returnRequiredStack.Peek();
    private bool YieldRequired => _yieldRequiredStack.Peek();
    private bool ReturnAllowed => _returnAllowedStack.Peek();
    private bool YieldAllowed => _yieldAllowedStack.Peek();

    public ImplicitNodeAnalyzer (Compilation cmp) {
        _cmp = cmp;
        _scope = new(_cmp);
    }

    public void Analyze (CompilerUnit unit) {
        foreach (var item in unit.TopLevelItems) {
            Visit(item);
        }

        if (unit.ImplicitFunction != null) Visit(unit.ImplicitFunction);
    }

    protected override RetInfo? VisitChildren (SyntaxNode node) {
        List<bool> hasReturns = new();
        List<bool> hasYields = new();

        foreach (var item in node.Children) {
            var retInfo = Visit(item);
            hasReturns.Add(retInfo?.hasReturn ?? false);
            hasYields.Add(retInfo?.hasYield ?? false);
        }

        if (hasReturns.Count == 0) return (false, false);

        return (hasReturns.All(t => t), hasYields.All(t => t));
    }

    public override RetInfo? Visit (FunctionDefinition node) {
        // right now, this should always return true.
        if (node.Body is BlockStatement blockStmt) {
            _returnRequiredStack.Push(true); // Regular functions must return.
            _yieldRequiredStack.Push(false); // Regular functions don't need to yield.
            _returnAllowedStack.Push(true); // Regular functions can return.
            _yieldAllowedStack.Push(false); // Regular functions cannot yield.

            Visit(blockStmt);

            _returnRequiredStack.Pop();
            _yieldRequiredStack.Pop();
            _returnAllowedStack.Pop();
            _yieldAllowedStack.Pop();
        }

        return null;
    }

    public override RetInfo? Visit (BlockStatement node) {
        bool hasReturn = false;
        bool hasYield = false;

        foreach (var child in node.Children) {
            RetInfo? retInfo = Visit(child);

            if (retInfo.HasValue == false) continue;

            if (retInfo.Value.hasReturn) hasReturn = true;
            if (retInfo.Value.hasYield) hasYield = true;
        }

        // If return is required but not found, append one at the end.
        if (ReturnRequired && hasReturn == false) {
            var autoReturn = new ReturnStatement(null) {
                IsAutoGenerated = true,
            };
            autoReturn.SetSpan(SourceSpan.None);
            autoReturn.SetLine(-100);

            node.Children.Add(autoReturn);

            hasReturn = true;
        }

        // If yield is required but not found, emit an error.
        if (YieldRequired && hasYield == false) {
            Messages.Add(CompilerMessage.Analyzers.NotAllPathsYieldValue(node.Line));
        }

        return (hasReturn, hasYield);
    }

    public override RetInfo? Visit (ArrowStatement node) {
        return (false, true);
    }

    public override RetInfo? Visit (ReturnStatement node) {
        return (true, false);
    }

    public override RetInfo? Visit (YieldStatement node) {
        return (false, true);
    }

    public override RetInfo? Visit (IfExpression node) {
        _returnRequiredStack.Push(false); // If expressions don't need to return.
        _yieldRequiredStack.Push(false); // If expressions don't need to yield (at least, for now).
        _returnAllowedStack.Push(ReturnAllowed); // If expressions may return if they are inside a block that can return.
        _yieldAllowedStack.Push(true); // If expressions can yield.

        RetInfo consequentRetInfo = Visit(node.Consequent)!.Value;
        RetInfo? alternateRetInfo = VisitIfNotNull(node.Alternate);

        _returnRequiredStack.Pop();
        _yieldRequiredStack.Pop();
        _returnAllowedStack.Pop();
        _yieldAllowedStack.Pop();

        // If expression doesn't have alternate, then it cannot possibly be complete.
        if (alternateRetInfo == null) return (false, false);

        // If one block yields a value and the other doesn't, that's an error.
        if (consequentRetInfo.hasYield != alternateRetInfo.Value.hasYield) {
            Messages.Add(CompilerMessage.Analyzers.NotAllPathsYieldValue(node.Line));
        }

        // an if has returns if both of its paths have returns. Same for yields.
        return (
            consequentRetInfo.hasReturn && alternateRetInfo.Value.hasReturn,
            consequentRetInfo.hasYield && alternateRetInfo.Value.hasYield
        );
    }

    public override RetInfo? Visit (WhileExpression node) {
        _returnRequiredStack.Push(false); // While expressions don't need to return.
        _yieldRequiredStack.Push(false); // While expressions don't need to yield (at least, for now).
        _returnAllowedStack.Push(ReturnAllowed); // While expressions may return if they are inside a block that can return.
        _yieldAllowedStack.Push(true); // While expressions can yield.

        RetInfo? retInfo = Visit(node.Body);

        _returnRequiredStack.Pop();
        _yieldRequiredStack.Pop();
        _returnAllowedStack.Pop();
        _yieldAllowedStack.Pop();

        // While has return and yield if its body does.
        return retInfo;
    }
}
