﻿namespace PT.PM.Common.Nodes.Expressions
{
    public class MemberReferenceExpression : Expression
    {
        public Expression Target { get; set; }

        public Expression Name { get; set; }

        public MemberReferenceExpression()
        {
        }

        public MemberReferenceExpression(Expression target, Expression name, TextSpan textSpan)
            : base(textSpan)
        {
            Target = target;
            Name = name;
        }

        public override Ust[] GetChildren() => new Ust[] { Target, Name };

        public override Expression[] GetArgs() => new Expression[] { Target, Name };

        public override string ToString()
        {
            return $"{Target}.{Name}";
        }
    }
}
