﻿using PT.PM.Common;
using PT.PM.Common.Nodes.TypeMembers;

namespace PT.PM.Matching.Patterns
{
    public class PatternParameterDeclaration : PatternUst<ParameterDeclaration>
    {
        public PatternUst Type { get; set; }

        public PatternUst Name { get; set; }

        public PatternUst Initializer { get; set; }

        public PatternParameterDeclaration()
        {
        }

        public PatternParameterDeclaration(PatternUst type, PatternUst name, TextSpan textSpan = default(TextSpan))
            : base(textSpan)
        {
            Type = type;
            Name = name;
        }

        public override string ToString() => Type != null ? $"{Type} {Name}" : Name.ToString();

        public override MatchingContext Match(ParameterDeclaration parameterDeclaration, MatchingContext context)
        {
            MatchingContext newContext = Type.MatchUst(parameterDeclaration.Type, context);
            if (!newContext.Success)
            {
                return newContext;
            }

            newContext = Name.MatchUst(parameterDeclaration.Name, newContext);

            return newContext.AddUstIfSuccess(parameterDeclaration);
        }
    }
}
