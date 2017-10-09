﻿using PT.PM.Common;
using PT.PM.Common.Nodes;
using PT.PM.Common.Nodes.Expressions;
using System.Collections.Generic;

namespace PT.PM.Matching.Patterns
{
    public class PatternObjectCreateExpression : PatternUst
    {
        public PatternUst Type { get; set; }

        public PatternArgs Arguments { get; set; }

        public PatternObjectCreateExpression()
        {
        }

        public PatternObjectCreateExpression(PatternUst type, PatternArgs args, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Type = type;
            Arguments = args;
        }

        public override string ToString() => $"new {Type}({Arguments})";

        public override MatchingContext Match(Ust ust, MatchingContext context)
        {
            MatchingContext newContext;

            if (ust is ObjectCreateExpression objectCreateExpression)
            {
                newContext = Type.Match(objectCreateExpression.Type, context);
                if (!newContext.Success)
                {
                    return newContext;
                }

                newContext = Arguments.Match(objectCreateExpression.Arguments, newContext);
            }
            else
            {
                newContext = context.Fail();
            }

            return newContext.AddUstIfSuccess(ust);
        }
    }
}