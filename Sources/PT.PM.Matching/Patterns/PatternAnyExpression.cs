﻿using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Matching.Patterns
{
    public class PatternAnyExpression : PatternUst
    {
        public PatternAnyExpression()
        {
        }

        public PatternAnyExpression(TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
        }

        public override string ToString() => "#";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            if (!(ust is Expression))
            {
                return context.Fail();
            }

            return context.AddMatch(ust);
        }
    }
}