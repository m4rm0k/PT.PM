﻿namespace PT.PM.Common.Nodes.Tokens
{
    /// <summary>
    /// TODO: possible replace with partial class. Specific constants will be defined in Specific folder.
    /// </summary>
    public enum BinaryOperator
    {
        Plus,             // +
        Minus,            // -
        Multiply,         // *
        Divide,           // /
        Mod,              // %
        BitwiseAnd,       // &
        BitwiseOr,        // |
        LogicalAnd,       // &&
        LogicalOr,        // ||
        BitwiseXor,       // ^
        ShiftLeft,        // <<
        ShiftRight,       // >>
        Equal,            // ==
        NotEqual,         // !=
        Greater,          // >
        Less,             // <
        GreaterOrEqual,   // >=
        LessOrEqual,      // <=

        NullCoalescing,   // ??
    }
}
