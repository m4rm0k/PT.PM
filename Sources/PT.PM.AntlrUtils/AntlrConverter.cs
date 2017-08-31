﻿using PT.PM.Common;
using PT.PM.Common.Exceptions;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using PT.PM.Common.Nodes.Tokens;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;

namespace PT.PM.AntlrUtils
{
    public abstract class AntlrConverter : IParseTreeToUstConverter, IParseTreeVisitor<UstNode>, ILoggable
    {
        protected static readonly Regex RegexHexLiteral = new Regex(@"^0[xX]([a-fA-F0-9]+)([uUlL]{0,2})$", RegexOptions.Compiled);
        protected static readonly Regex RegexOctalLiteral = new Regex(@"^0([0-7]+)([uUlL]{0,2})$", RegexOptions.Compiled);
        protected static readonly Regex RegexBinaryLiteral = new Regex(@"^0[bB]([01]+)([uUlL]{0,2})$", RegexOptions.Compiled);
        protected static readonly Regex RegexDecimalLiteral = new Regex(@"^([0-9]+)([uUlL]{0,2})$", RegexOptions.Compiled);

        protected RootNode root;

        public abstract Language Language { get; }

        public HashSet<Language> AnalyzedLanguages { get; set; }

        public IList<IToken> Tokens { get; set; }

        public Parser Parser { get; set; }

        public ILogger Logger { get; set; } = DummyLogger.Instance;

        public RootNode ParentRoot { get; set; }

        public AntlrConverter()
        {
            AnalyzedLanguages = Language.GetSelfAndSublanguages();
        }

        public RootNode Convert(ParseTree langParseTree)
        {
            var antlrParseTree = (AntlrParseTree)langParseTree;
            ParserRuleContext tree = antlrParseTree.SyntaxTree;
            ICharStream inputStream = tree.start.InputStream ?? tree.stop?.InputStream;
            string filePath = inputStream != null ? inputStream.SourceName : "";
            RootNode result = null;
            if (tree != null && inputStream != null)
            {
                try
                {
                    Tokens = antlrParseTree.Tokens;
                    root = new RootNode(langParseTree.SourceCodeFile, Language);
                    UstNode visited = Visit(tree);
                    if (visited is RootNode rootUstNode)
                    {
                        result = rootUstNode;
                    }
                    else
                    {
                        result = root;
                        result.Node = visited;
                    }
                    result.FillAscendants();
                }
                catch (Exception ex)
                {
                    Logger.LogError(new ConversionException(filePath, ex));

                    if (result == null)
                    {
                        result = new RootNode(langParseTree.SourceCodeFile, Language);
                        result.Comments = ArrayUtils<CommentLiteral>.EmptyArray;
                    }
                }
            }
            else
            {
                result = new RootNode(langParseTree.SourceCodeFile, Language);
                result.Comments = ArrayUtils<CommentLiteral>.EmptyArray;
            }
            result.Comments = antlrParseTree.Comments.Select(c => new CommentLiteral(c.Text, c.GetTextSpan(), result)).ToArray();
            return result;
        }

        public UstNode Visit(IParseTree tree)
        {
            try
            {
                if (tree == null)
                {
                    return null;
                }

                return tree.Accept(this);
            }
            catch (Exception ex)
            {
                var parserRuleContext = tree as ParserRuleContext;
                if (parserRuleContext != null)
                {
                    AntlrHelper.LogConversionError(ex, parserRuleContext, root.SourceCodeFile.FullPath, root.SourceCodeFile.Code, Logger);
                }
                return DefaultResult;
            }
        }

        public UstNode VisitChildren(IRuleNode node)
        {
            UstNode result;
            if (node.ChildCount == 0)
            {
                result = null;
            }
            else if (node.ChildCount == 1)
            {
                result = Visit(node.GetChild(0));
            }
            else
            {
                var exprs = new List<Expression>();
                for (int i = 0; i < node.ChildCount; i++)
                {
                    var child = Visit(node.GetChild(i));
                    if (child != null)
                    {
                        var childExpression = child as Expression;
                        // Ignore null.
                        if (childExpression != null)
                        {
                            exprs.Add(childExpression);
                        }
                        else
                        {
                            exprs.Add(new WrapperExpression(child));
                        }
                    }
                }
                result = new MultichildExpression(exprs, root);
            }
            return result;
        }

        public virtual UstNode VisitTerminal(ITerminalNode node)
        {
            Token result;
            string nodeText = node.GetText();
            TextSpan textSpan = node.GetTextSpan();
            if ((nodeText.StartsWith("'") && nodeText.EndsWith("'")) ||
                (nodeText.StartsWith("\"") && nodeText.EndsWith("\"")))
            {
                result = new StringLiteral(nodeText.Substring(1, nodeText.Length - 2), textSpan, root);
            }
            else if (nodeText.Contains("."))
            {
                double value;
                double.TryParse(nodeText, out value);
                return new FloatLiteral(value, textSpan, root);
            }

            var integerToken = TryParseInteger(nodeText, textSpan);
            if (integerToken != null)
            {
                return integerToken;
            }
            else
            {
                result = new IdToken(nodeText, textSpan, root);
            }
            return result;
        }

        public UstNode VisitErrorNode(IErrorNode node)
        {
            return DefaultResult;
        }

        protected Token TryParseInteger(string text, TextSpan textSpan)
        {
            Match match = RegexHexLiteral.Match(text);
            if (match.Success)
            {
                long value;
                match.Groups[1].Value.TryConvertToInt64(16, out value);
                return new IntLiteral(value, textSpan, root);
            }
            match = RegexOctalLiteral.Match(text);
            if (match.Success)
            {
                long value;
                match.Groups[1].Value.TryConvertToInt64(8, out value);
                return new IntLiteral(value, textSpan, root);
            }
            match = RegexBinaryLiteral.Match(text);
            if (match.Success)
            {
                long value;
                match.Groups[1].Value.TryConvertToInt64(2, out value);
                return new IntLiteral(value, textSpan, root);
            }
            match = RegexDecimalLiteral.Match(text);
            if (match.Success)
            {
                long value;
                match.Groups[1].Value.TryConvertToInt64(10, out value);
                return new IntLiteral(value, textSpan, root);
            }
            return null;
        }

        protected UstNode VisitShouldNotBeVisited(IParseTree tree)
        {
            var parserRuleContext = tree as ParserRuleContext;
            string ruleName = "";
            if (parserRuleContext != null)
            {
                ruleName = Parser?.RuleNames.ElementAtOrDefault(parserRuleContext.RuleIndex)
                           ?? parserRuleContext.RuleIndex.ToString();
            }

            throw new ShouldNotBeVisitedException(ruleName);
        }

        protected UstNode DefaultResult
        {
            get
            {
                return null;
            }
        }

        protected Expression CreateBinaryOperatorExpression(
            ParserRuleContext left, IToken operatorTerminal, ParserRuleContext right)
        {
            return CreateBinaryOperatorExpression(left, operatorTerminal.Text, operatorTerminal.GetTextSpan(), right);
        }

        protected Expression CreateBinaryOperatorExpression(
            ParserRuleContext left, ITerminalNode operatorTerminal, ParserRuleContext right)
        {
            return CreateBinaryOperatorExpression(left, operatorTerminal.GetText(), operatorTerminal.GetTextSpan(),  right);
        }

        protected virtual BinaryOperator CreateBinaryOperator(string binaryOperatorText)
        {
            return BinaryOperatorLiteral.TextBinaryOperator[binaryOperatorText];
        }

        private Expression CreateBinaryOperatorExpression(ParserRuleContext left, string operatorText, TextSpan operatorTextSpan, ParserRuleContext right)
        {
            BinaryOperator binaryOperator = CreateBinaryOperator(operatorText);

            var expression0 = (Expression)Visit(left);
            var expression1 = (Expression)Visit(right);
            var result = new BinaryOperatorExpression(
                expression0,
                new BinaryOperatorLiteral(binaryOperator, operatorTextSpan, root),
                expression1,
                left.GetTextSpan().Union(right.GetTextSpan()), root);

            return result;
        }
    }
}
