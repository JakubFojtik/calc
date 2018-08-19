using System;

public enum TokenType { BraceOpen, BraceClose, Number, Operator, EOF }
public enum OperatorType { None, Plus, Minus, Star, Slash, Sin, ASin, Caret }

public class Token
{
    public TokenType Type { get; set; }
    public object Value { get; set; }

    public Token(TokenType type, object value = null)
    {
        Type = type;
        Value = value;
    }

    public override string ToString() => string.Format("{0}({1})", Type, Value);
}

public class ASTToken : Token
{
    public AST Ast { get; set; }

    public ASTToken(AST ast) : base(TokenType.Number)   //TokenType.Operator?
    {
        Ast = ast;
    }

    public override string ToString() => "AST";
}

public class AST
{
    public Token value;
    public AST left;
    public AST right;

    public AST(Token v, AST l, AST r)
    {
        value = v;
        left = l;
        right = r;
    }
    public AST(Token v) : this(v, null, null) { }

    public override string ToString() => value.Value.ToString();

    public decimal compute()
    {
        var first = left?.compute();
        var second = right?.compute();
        switch (value.Type)
        {
            case TokenType.Number:
                return (decimal)value.Value;    //todo convert or generic type
            case TokenType.Operator:
                var opType = (OperatorType)value.Value;
                decimal defaultBinary(decimal? left, decimal? right) => operatorImpls[opType].getDecimal(first.Value, second.Value);
                switch (opType)
                {
                    default: //binary op
                        return defaultBinary(first, second);
                    case OperatorType.Minus:
                        if (second != null) return defaultBinary(first, second);
                        else return -1 * first.Value;
                    case OperatorType.Sin:
                        return (decimal)Math.Sin((double)first.Value);
                    case OperatorType.ASin:
                        return (decimal)Math.Asin((double)first.Value);
                }
            default:
                throw new ArithmeticException(ERROR);
        }
    }
}
