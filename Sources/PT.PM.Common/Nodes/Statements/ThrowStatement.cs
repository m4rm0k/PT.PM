﻿using PT.PM.Common.Nodes.Expressions;

namespace PT.PM.Common.Nodes.Statements
{
    public class ThrowStatement : Statement
    {
        public Expression ThrowExpression { get; set; }

        public ThrowStatement(Expression throwExpression, TextSpan textSpan)
            : base(textSpan)
        {
            ThrowExpression = throwExpression;
        }

        public ThrowStatement()
        {
        }

        public override Ust[] GetChildren()
        {
            return new Ust[] {ThrowExpression};
        }

        public override string ToString()
        {
            return $"throw {ThrowExpression};";
        }
    }
}
