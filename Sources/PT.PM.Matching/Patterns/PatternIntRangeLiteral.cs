﻿using PT.PM.Common;
using PT.PM.Common.Nodes.Tokens.Literals;

namespace PT.PM.Matching.Patterns
{
    public class PatternIntRangeLiteral : PatternUst<IntLiteral>
    {
        public long MinValue { get; set; }

        public long MaxValue { get; set; }

        public PatternIntRangeLiteral()
            : this(long.MinValue, long.MaxValue)
        {
        }

        public PatternIntRangeLiteral(long value, TextSpan textSpan = default(TextSpan))
            : this(value, value, textSpan)
        {
        }

        public PatternIntRangeLiteral(long minValue, long maxValue, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public override string ToString()
        {
            if (MinValue == MaxValue)
            {
                return MinValue.ToString();
            }

            string lowerBound = MinValue == long.MinValue ? "" : MinValue.ToString();
            string upperBound = MaxValue == long.MaxValue ? "" : MaxValue.ToString();
            return $"<({lowerBound}..{upperBound})>";
        }

        public override MatchContext Match(IntLiteral intLiteral, MatchContext context)
        {
            return intLiteral.Value >= MinValue && intLiteral.Value < MaxValue
                ? context.AddMatch(intLiteral)
                : context.Fail();
        }
    }
}
