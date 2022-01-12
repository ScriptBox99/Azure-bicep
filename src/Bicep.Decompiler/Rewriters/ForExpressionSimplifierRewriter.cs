// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Bicep.Core.Semantics;
using Bicep.Core.Syntax;
using Bicep.Core.Syntax.Rewriters;
using Bicep.Core.Syntax.Visitors;
using Bicep.Core.TypeSystem;

namespace Bicep.Core.Decompiler.Rewriters
{
    /// <summary>
    /// Looks for syntax of form (commonly generated by the decompiler):
    ///   for {index} in range(0, length({array}))
    /// Where elements are being accessed via:
    ///   {array}[{index}]
    ///
    /// Rewrites this syntax instead to:
    ///   for {item} in {array}
    /// Where elements are accessed via:
    ///   {item}
    /// </summary>
    public class ForExpressionSimplifierRewriter : SyntaxRewriteVisitor
    {
        private readonly SemanticModel semanticModel;

        public ForExpressionSimplifierRewriter(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        private static bool IsLoopIteratorExpression(SemanticModel semanticModel, SyntaxBase syntax, [NotNullWhen(true)] out VariableAccessSyntax? arrayVariable)
        {
            arrayVariable = null;

            // look for a range() function with 2 args
            if (SemanticModelHelper.TryGetFunctionInNamespace(semanticModel, "sys", syntax) is not FunctionCallSyntaxBase rangeFunction ||
                !LanguageConstants.IdentifierComparer.Equals(rangeFunction.Name.IdentifierName, "range") ||
                rangeFunction.Arguments.Length != 2)
            {
                return false;
            }

            // first range() arg must be 0
            if (rangeFunction.Arguments[0].Expression is not IntegerLiteralSyntax startRange ||
                startRange.Value != 0)
            {
                return false;
            }

            // look for a length() function with 1 arg
            if (SemanticModelHelper.TryGetFunctionInNamespace(semanticModel, "sys", rangeFunction.Arguments[1].Expression) is not FunctionCallSyntaxBase lengthFunction ||
                !LanguageConstants.IdentifierComparer.Equals(lengthFunction.Name.IdentifierName, "length") ||
                lengthFunction.Arguments.Length != 1)
            {
                return false;
            }

            // first length() arg must be a variable
            if (lengthFunction.Arguments[0].Expression is not VariableAccessSyntax variableAccess)
            {
                return false;
            }

            arrayVariable = variableAccess;
            return true;
        }

        protected override SyntaxBase ReplaceForSyntax(ForSyntax syntax)
        {
            syntax = (ForSyntax)base.ReplaceForSyntax(syntax);

            // look for range(0, length(<variable>))
            if (!IsLoopIteratorExpression(semanticModel, syntax.Expression, out var arrayVariable))
            {
                return syntax;
            }

            if (syntax.VariableSection is not LocalVariableSyntax indexVariable)
            {
                return syntax;
            }

            var arraySymbol = semanticModel.GetSymbolInfo(arrayVariable);
            var arrayIndexSymbol = semanticModel.GetSymbolInfo(indexVariable);

            if (arraySymbol is null || arrayIndexSymbol is null)
            {
                return syntax;
            }

            var arrayAccesses = new HashSet<ArrayAccessSyntax>();
            var independentIndexAccesses = new HashSet<VariableAccessSyntax>();
            CallbackVisitor.Visit(syntax, child =>
            {
                if (child is ArrayAccessSyntax arrayAccess)
                {
                    if (semanticModel.GetSymbolInfo(arrayAccess.BaseExpression) == arraySymbol &&
                        semanticModel.GetSymbolInfo(arrayAccess.IndexExpression) == arrayIndexSymbol)
                    {
                        arrayAccesses.Add(arrayAccess);

                        // we don't want to count the VariableAccessSyntax under this particular node,
                        // so return false to skip visiting children.
                        return false;
                    }
                }

                if (child is VariableAccessSyntax variableAccess)
                {
                    var accessSymbol = semanticModel.GetSymbolInfo(variableAccess);
                    if (accessSymbol == arrayIndexSymbol)
                    {
                        independentIndexAccesses.Add(variableAccess);
                    }
                }

                return true;
            });

            if (!arrayAccesses.Any())
            {
                // nothing to really simplify here
                return syntax;
            }

            var itemVarName = GetUniqueVariableNameForNewScope(syntax, "item");
            var forBody = CallbackRewriter.Rewrite(syntax.Body, child =>
            {
                if (arrayAccesses.Contains(child))
                {
                    return new VariableAccessSyntax(SyntaxFactory.CreateIdentifier(itemVarName));
                }

                return child;
            });

            SyntaxBase forVariableBlockSyntax;
            if (independentIndexAccesses.Any())
            {
                forVariableBlockSyntax = new ForVariableBlockSyntax(
                    SyntaxFactory.LeftParenToken,
                    new LocalVariableSyntax(SyntaxFactory.CreateIdentifier(itemVarName)),
                    SyntaxFactory.CommaToken,
                    new LocalVariableSyntax(SyntaxFactory.CreateIdentifier(arrayIndexSymbol.Name)),
                    SyntaxFactory.RightParenToken);
            }
            else
            {
                forVariableBlockSyntax = new LocalVariableSyntax(SyntaxFactory.CreateIdentifier(itemVarName));
            }

            var forExpression = new VariableAccessSyntax(SyntaxFactory.CreateIdentifier(arraySymbol.Name));

            return new ForSyntax(
                syntax.OpenSquare,
                syntax.ForKeyword,
                forVariableBlockSyntax,
                syntax.InKeyword,
                forExpression,
                syntax.Colon,
                forBody,
                syntax.CloseSquare);
        }

        private static string GetUniqueVariableNameForNewScope(SyntaxBase syntax, string name)
        {
            var variableAccesses = new HashSet<string>(LanguageConstants.IdentifierComparer);
            CallbackVisitor.Visit(syntax, child =>
            {
                if (child is VariableAccessSyntax variableAccess)
                {
                    variableAccesses.Add(variableAccess.Name.IdentifierName);
                }

                return true;
            });

            if (!variableAccesses.Contains(name))
            {
                return name;
            }

            var index = 1;
            while (true)
            {
                var newName = $"{name}_{index}";

                if (!variableAccesses.Contains(newName))
                {
                    return newName;
                }
            }
        }
    }
}
