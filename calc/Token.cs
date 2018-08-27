namespace calc
{
    public abstract class Token
    {
        public abstract int NumOperands();
        public abstract override string ToString();
    }
}