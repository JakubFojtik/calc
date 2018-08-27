namespace calc
{
    public abstract class Token
    {
        public abstract bool HasOperands();
        public abstract override string ToString();
    }
}