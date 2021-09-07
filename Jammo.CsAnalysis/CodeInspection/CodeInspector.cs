using System;
using System.Collections.Generic;
using System.Linq;
using Jammo.CsAnalysis.CodeInspection.Rules;
using Jammo.CsAnalysis.Compilation;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jammo.CsAnalysis.CodeInspection
{
    public class CodeInspector
    {
        private readonly List<InspectionRule> rules = new();
        private readonly List<Inspection> inspections = new();

        public IEnumerable<Inspection> Inspections => inspections;
        
        public void AddInspection(Inspection inspection)
        {
            inspections.Add(inspection);
        }

        public void Inspect(string text, CompilationWrapper context)
        {
            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetRoot();
            var walker = new RuleWalker(rules, context);

            walker.Visit(root);

            inspections.AddRange(walker.Result);
        }

        public void WithRules(params IEnumerable<InspectionRule>[] sets)
        {
            foreach (var ruleSet in sets)
                rules.AddRange(ruleSet);
        }

        private class RuleWalker : CSharpSyntaxWalker
        {
            private readonly CompilationWrapper context;
            
            private readonly IEnumerable<InspectionRule> rules;
            private readonly List<Inspection> result = new();
            
            public IEnumerable<Inspection> Result => result;

            public RuleWalker(IEnumerable<InspectionRule> rules, CompilationWrapper context) : base(SyntaxWalkerDepth.Trivia)
            {
                this.rules = rules;
                this.context = context;
            }

            public override void VisitUsingDirective(UsingDirectiveSyntax node)
            {
                InvokeRule(r => r.TestUsingDirective(node, context));
                
                base.VisitUsingDirective(node);
            }

            public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
            {
                InvokeRule(r => r.TestNamespaceDeclaration(node, context));
                
                base.VisitNamespaceDeclaration(node);
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                InvokeRule(r => r.TestClassDeclaration(node, context));
                
                base.VisitClassDeclaration(node);
            }

            public override void VisitStructDeclaration(StructDeclarationSyntax node)
            {
                InvokeRule(r => r.TestStructDeclaration(node, context));
                
                base.VisitStructDeclaration(node);
            }

            public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            {
                InvokeRule(r => r.TestInterfaceDeclaration(node, context));
                
                base.VisitInterfaceDeclaration(node);
            }

            public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
            {
                InvokeRule(r => r.TestRecordDeclaration(node, context));
                
                base.VisitRecordDeclaration(node);
            }

            public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                InvokeRule(r => r.TestFieldDeclaration(node, context));
                
                base.VisitFieldDeclaration(node);
            }

            public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                InvokeRule(r => r.TestPropertyDeclaration(node, context));
                
                base.VisitPropertyDeclaration(node);
            }

            public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                InvokeRule(r => r.TestMethodDeclaration(node, context));
                
                base.VisitMethodDeclaration(node);
            }

            public override void VisitVariableDeclaration(VariableDeclarationSyntax node)
            {
                InvokeRule(r => r.TestVariableDeclaration(node, context));
                
                base.VisitVariableDeclaration(node);
            }

            public override void VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                InvokeRule(r => r.TestVariableAssignment(node, context));
                
                base.VisitVariableDeclarator(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                InvokeRule(r => r.TestMethodInvocation(node, context));
                
                base.VisitInvocationExpression(node);
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                InvokeRule(r => r.TestMemberAccess(node, context));
                
                base.VisitMemberAccessExpression(node);
            }

            public override void VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                Action<InspectionRule> flag;

                switch (node.Kind())
                {
                    case SyntaxKind.EqualsExpression:
                        flag = r => r.TestValueEquals(node, context);
                        break;
                    case SyntaxKind.NotEqualsExpression:
                        flag = r => r.TestValueNotEqual(node, context);
                        break;
                    case SyntaxKind.ExclamationToken:
                        flag = r => r.TestNot(node, context);
                        break;
                    case SyntaxKind.LessThanExpression:
                        flag = r => r.TestLessThan(node, context);
                        break;
                    case SyntaxKind.GreaterThanExpression:
                        flag = r => r.TestGreaterThan(node, context);
                        break;
                    case SyntaxKind.LessThanOrEqualExpression:
                        flag = r => r.TestLessThanOrEqual(node, context);
                        break;
                    case SyntaxKind.GreaterThanOrEqualExpression:
                        flag = r => r.TestMoreThanOrEqual(node, context);
                        break;
                    default:
                        return;
                }
                
                InvokeRule(flag);
                
                base.VisitExpressionStatement(node);
            }

            public override void VisitLiteralExpression(LiteralExpressionSyntax node)
            {
                Action<InspectionRule> flag;

                switch (node.Kind())
                {
                    case SyntaxKind.StringLiteralExpression:
                        flag = r => r.TestStringLiteral(node, context);
                        break;
                    case SyntaxKind.NumericLiteralExpression:
                        flag = r => r.TestNumericLiteral(node, context);
                        break;
                    default:
                        return;
                }
                
                InvokeRule(flag);
                
                base.VisitLiteralExpression(node);
            }

            public override void VisitTrivia(SyntaxTrivia trivia)
            {
                Action<InspectionRule> flag;

                switch (trivia.Kind())
                {
                    case SyntaxKind.SingleLineCommentTrivia:
                        flag = r => r.TestSingleLineComment(trivia, context);
                        break;
                    case SyntaxKind.MultiLineCommentTrivia:
                        flag = r => r.TestMultiLineComment(trivia, context);
                        break;
                    case SyntaxKind.SingleLineDocumentationCommentTrivia:
                        flag = r => r.TestDocumentationComment(trivia, context);
                        break;
                    case SyntaxKind.MultiLineDocumentationCommentTrivia:
                        flag = r => r.TestDocumentationComment(trivia, context);
                        break;
                    default:
                        return;
                }
                
                InvokeRule(flag);
                
                base.VisitTrivia(trivia);
            }
            
            private void InvokeRule(Action<InspectionRule> factory)
            {
                foreach (var rule in rules)
                    factory.Invoke(rule);
            }
        }
    }
}