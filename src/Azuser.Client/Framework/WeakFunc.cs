using System;
using System.Reflection;

namespace Azuser.Client.Framework
{
    public class WeakFunc<TResult>
    {
        private readonly Func<TResult> _staticFunc;

        private MethodInfo Method { get; }

        public bool IsStatic => _staticFunc != null;

        private WeakReference FuncReference { get; }

        private WeakReference Reference { get; }

        public WeakFunc(Func<TResult> func)
        {
            if (func.Method.IsStatic)
            {
                _staticFunc = func;

                if (func.Target != null)
                {
                    Reference = new WeakReference(func.Target);
                }

                return;
            }

            Method = func.Method;
            FuncReference = new WeakReference(func.Target);

            Reference = new WeakReference(func.Target);
        }

        public bool IsAlive
        {
            get
            {
                if (_staticFunc == null && Reference == null)
                {
                    return false;
                }

                if (_staticFunc != null)
                {
                    return Reference == null || Reference.IsAlive;
                }
                
                return Reference != null && Reference.IsAlive;
            }
        }

        private object FuncTarget => FuncReference?.Target;

        public TResult Execute()
        {
            if (_staticFunc != null)
            {
                return _staticFunc();
            }

            var funcTarget = FuncTarget;

            if (IsAlive)
            {
                if (Method != null
                    && FuncReference != null
                    && funcTarget != null)
                {
                    return (TResult) Method.Invoke(funcTarget, null);
                }
            }

            return default;
        }
    }
}