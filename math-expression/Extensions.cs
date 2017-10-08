using System;
using System.Collections.Generic;
using System.Linq;

namespace YuKu.MathExpression
{
    public static class Extensions
    {
        public static ICompiler AddParameter(this String mathExpression, String parameterName)
        {
            var compiler = new Compiler(mathExpression)
                .AddParameter(parameterName);
            return compiler;
        }

        public static ICompiler AddParameters(this String mathExpression, params String[] parameterNames)
        {
            var compiler = new Compiler(mathExpression)
                .AddParameters(parameterNames);
            return compiler;
        }

        public static ICompiler AddMathModule(this String mathExpression)
        {
            var compiler = new Compiler(mathExpression)
                .AddMathModule();
            return compiler;
        }

        public static ICompiler AddModule(this String mathExpression, Type module)
        {
            var compiler = new Compiler(mathExpression)
                .AddModule(module);
            return compiler;
        }

        public static ICompiler AddModules(this String mathExpression, params Type[] modules)
        {
            var compiler = new Compiler(mathExpression)
                .AddModules(modules);
            return compiler;
        }

        public static ICompiler AddParameters(this ICompiler compiler, IEnumerable<String> parameterNames)
        {
            return parameterNames.Aggregate(compiler, (current, parameterName) => current.AddParameter(parameterName));
        }

        public static ICompiler AddMathModule(this ICompiler compiler)
        {
            compiler = compiler.AddModule(typeof(Math));
            return compiler;
        }

        public static ICompiler AddModules(this ICompiler compiler, IEnumerable<Type> modules)
        {
            return modules.Aggregate(compiler, (current, module) => current.AddModule(module));
        }

        public static TDelegate Compile<TDelegate>(this String mathExpression, params String[] parameterNames)
        {
            var compiler = new Compiler(mathExpression)
                .AddParameters(parameterNames)
                .AddModule(typeof(Math));
            return compiler.Compile<TDelegate>();
        }
    }
}
