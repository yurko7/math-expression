using System;

namespace YuKu.MathExpression
{
    public interface ICompiler
    {
        ICompiler AddParameter(String parameterName);

        ICompiler AddModule(Type module);

        TDelegate Compile<TDelegate>();
    }
}
