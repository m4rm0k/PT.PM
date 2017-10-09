﻿using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Tokens;
using PT.PM.Common.Nodes.Tokens.Literals;
using System.Linq;

namespace PT.PM.Matching.Patterns
{
    public class PatternBinaryOperatorLiteral : PatternUst
    {
        public BinaryOperator BinaryOperator { get; set; }

        public PatternBinaryOperatorLiteral()
            : this(BinaryOperator.Equal)
        {
        }

        public PatternBinaryOperatorLiteral(string opText, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            BinaryOperator op;
            if (!BinaryOperatorLiteral.TextBinaryOperator.TryGetValue(opText, out op))
            {
                op = BinaryOperator.Equal;
            }
            BinaryOperator = op;
        }

        public PatternBinaryOperatorLiteral(BinaryOperator op, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            BinaryOperator = op;
        }

        public override string ToString() =>
            BinaryOperatorLiteral.TextBinaryOperator.FirstOrDefault(pair => pair.Value == BinaryOperator).Key;

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is BinaryOperatorLiteral binaryOperatorLiteral &&
                BinaryOperator.Equals(binaryOperatorLiteral.BinaryOperator))
            {
                newContext = context.AddMatch(ust);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext;
        }
    }
}