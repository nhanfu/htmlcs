using Bridge.Html5;
using System;
using System.Collections.Generic;

namespace MVVM
{
    public class Observable
    {
        protected static readonly List<Observable> _computedStack = new List<Observable>();
        protected static readonly List<Observable> _exeStack = new List<Observable>();
        protected readonly Func<object> _computedFn;
        protected object _oldValue { get; set; }
        protected object _newValue { get; set; }
        protected readonly List<Action<object>> _subscribers = new List<Action<object>>();
        protected readonly List<Observable> _dependencies = new List<Observable>();
        private int? delayTime = null;
        private int animationFrameId = -1;

        public Observable(object data)
        {
            _oldValue = data;
            _newValue = data;
        }

        public Observable(Func<object> data)
        {
            _computedFn = data;
            _computedStack.Add(this);
            _newValue = _oldValue = data();
            _computedStack.RemoveAt(_computedStack.Count - 1);
        }

        public virtual object Data
        {
            get
            {
                object res;
                if (_computedFn != null)
                {
                    _computedStack.Add(this);
                    res = _computedFn();
                    _oldValue = _newValue;
                    _newValue = res;
                    _computedStack.RemoveAt(_computedStack.Count - 1);
                }
                else
                {
                    res = _newValue;
                }
                if (_computedStack.Count > 0)
                {
                    SetDependency(_computedStack[_computedStack.Count - 1]);
                }
                return res;
            }
            set
            {
                if (_newValue == value)
                {
                    return;
                }
                _oldValue = _newValue;
                _newValue = value;
                NotifyChange();
            }
        }

        public virtual void Subscribe(Action<ObservableArgs> subscriber)
        {
            _subscribers.Add(subscriber.As<Action<object>>());
        }

        public void SetDependency(Observable dependency)
        {
            var index = _dependencies.IndexOf(dependency);
            if (index >= 0 || dependency == this) return;
            _dependencies.Add(dependency);
        }

        protected virtual void Notify()
        {
            var isBeingExecuted = _exeStack.IndexOf(this) >= 0;
            if (isBeingExecuted)
                return;
            _exeStack.Add(this);
            var newData = Data;
            _subscribers.ForEach((subscriber) => {
                subscriber(new ObservableArgs
                {
                    NewData = newData,
                    OldData = _oldValue
                });
            });
            _dependencies.ForEach((dpc) => {
                dpc.NotifyChange();
            });
            _exeStack.Remove(this);
        }

        public virtual void NotifyChange()
        {
            if (_computedFn == null && delayTime == null)
            {
                Notify();
            }
            else
            {
                Window.ClearTimeout(animationFrameId);
                animationFrameId = Window.SetTimeout(Notify, delayTime ?? 0);
            }
        }

        public void SetDelay(int delay)
        {
            delayTime = delay;
        }

        public static Observable<T> Of<T>(T value) => new Observable<T>(value);

        public static Observable<T> Of<T>(Func<T> value) => new Observable<T>(value);

        public static ObservableArray<T> Of<T>(T[] value) => new ObservableArray<T>(value);
    }

    public class Observable<T> : Observable
    {
        public Observable(T data): base(data)
        {

        }

        public Observable(Func<T> data) : base(data.As<Func<object>>())
        {

        }

        public virtual new T Data
        {
            get => (T)base.Data;
            set => base.Data = value;
        }

        public virtual void Subscribe(Action<ObservableArgs<T>> subscriber)
        {
            var action = subscriber as Action<object>;
            _subscribers.Add(action);
        }

        protected override void Notify()
        {
            var isBeingExecuted = _exeStack.IndexOf(this) >= 0;
            if (isBeingExecuted)
                return;
            _exeStack.Add(this);
            var newData = Data;
            _subscribers.ForEach((subscriber) => {
                subscriber(new ObservableArgs<T>
                {
                    NewData = newData,
                    OldData = (T)_oldValue
                });
            });
            _dependencies.ForEach((dpc) => {
                dpc.NotifyChange();
            });
            _exeStack.Remove(this);
        }
    }
}
