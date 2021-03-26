using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JT100.Wish.Core
{
    public class WeakFunc<TResult>
    {
        private Func<TResult> _staticFunc;

        protected MethodInfo Method
        {
            get;
            set;
        }

        public bool IsStatic => _staticFunc != null;

        public virtual string MethodName
        {
            get
            {
                if (_staticFunc != null)
                {
                    return _staticFunc.Method.Name;
                }

                return Method.Name;
            }
        }

        protected WeakReference FuncReference
        {
            get;
            set;
        }

        protected WeakReference Reference
        {
            get;
            set;
        }

        public virtual bool IsAlive
        {
            get
            {
                if (_staticFunc == null && Reference == null)
                {
                    return false;
                }

                if (_staticFunc != null)
                {
                    if (Reference != null)
                    {
                        return Reference.IsAlive;
                    }

                    return true;
                }

                return Reference.IsAlive;
            }
        }

        public object Target
        {
            get
            {
                if (Reference == null)
                {
                    return null;
                }

                return Reference.Target;
            }
        }

        protected object FuncTarget
        {
            get
            {
                if (FuncReference == null)
                {
                    return null;
                }

                return FuncReference.Target;
            }
        }

        protected WeakFunc()
        {
        }

        public WeakFunc(Func<TResult> func)
            : this(func?.Target, func)
        {
        }

        public WeakFunc(object target, Func<TResult> func)
        {
            if (func.Method.IsStatic)
            {
                _staticFunc = func;
                if (target != null)
                {
                    Reference = new WeakReference(target);
                }
            }
            else
            {
                Method = func.Method;
                FuncReference = new WeakReference(func.Target);
                Reference = new WeakReference(target);
            }
        }

        public TResult Execute()
        {
            if (_staticFunc != null)
            {
                return _staticFunc();
            }

            object funcTarget = FuncTarget;
            if (IsAlive && Method != null && FuncReference != null && funcTarget != null)
            {
                return (TResult)Method.Invoke(funcTarget, null);
            }

            return default(TResult);
        }

        public void MarkForDeletion()
        {
            Reference = null;
            FuncReference = null;
            Method = null;
            _staticFunc = null;
        }
    }
    public class WeakFunc<T, TResult> : WeakFunc<TResult>, IExecuteWithObjectAndResult
    {
        private Func<T, TResult> _staticFunc;

        public override string MethodName
        {
            get
            {
                if (_staticFunc != null)
                {
                    return _staticFunc.Method.Name;
                }

                return base.Method.Name;
            }
        }

        public override bool IsAlive
        {
            get
            {
                if (_staticFunc == null && base.Reference == null)
                {
                    return false;
                }

                if (_staticFunc != null)
                {
                    if (base.Reference != null)
                    {
                        return base.Reference.IsAlive;
                    }

                    return true;
                }

                return base.Reference.IsAlive;
            }
        }

        public WeakFunc(Func<T, TResult> func)
            : this(func?.Target, func)
        {
        }

        public WeakFunc(object target, Func<T, TResult> func)
        {
            if (func.Method.IsStatic)
            {
                _staticFunc = func;
                if (target != null)
                {
                    base.Reference = new WeakReference(target);
                }
            }
            else
            {
                base.Method = func.Method;
                base.FuncReference = new WeakReference(func.Target);
                base.Reference = new WeakReference(target);
            }
        }

        public new TResult Execute()
        {
            return Execute(default(T));
        }

        public TResult Execute(T parameter)
        {
            if (_staticFunc != null)
            {
                return _staticFunc(parameter);
            }

            object funcTarget = base.FuncTarget;
            if (IsAlive && base.Method != null && base.FuncReference != null && funcTarget != null)
            {
                return (TResult)base.Method.Invoke(funcTarget, new object[1]
                {
                    parameter
                });
            }

            return default(TResult);
        }

        public object ExecuteWithObject(object parameter)
        {
            T parameter2 = (T)parameter;
            return Execute(parameter2);
        }

        public new void MarkForDeletion()
        {
            _staticFunc = null;
            base.MarkForDeletion();
        }
    }
}
