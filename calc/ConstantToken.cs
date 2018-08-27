using System;
using System.Collections.Generic;

namespace calc
{
    public class ConstantToken : NumberToken
    {
        public enum ConstantType
        {
            Pi, E
        }

        public static Dictionary<ConstantType, decimal> constantValues = new Dictionary<ConstantType, decimal>
        {
            { ConstantType.Pi, (decimal)Math.PI },
            { ConstantType.E, (decimal)Math.E },
        };

        public ConstantType Constant { get; private set; }

        public ConstantToken(ConstantType cons) : base(constantValues[cons])
        {
            Constant = cons;
        }

        public override string ToString()
        {
            return string.Format("Constant({0})", Constant);
        }
    }
}
