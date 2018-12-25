using Bridge;
using Bridge.Html5;
using System;

namespace MVVM
{
    public class ObservableArgs
    {
        public object NewData { get; internal set; }
        public object OldData { get; internal set; }
    }

    public class ObservableArgs<T>
    {
        public T NewData { get; internal set; }
        public T OldData { get; internal set; }
    }

    public class ObservableArrayArgs<T>
    {
        public T[] Array { get; set; }
        public T Item { get; set; }
        public int Index { get; set; }
        public ObservableAction? Action { get; set; }
        internal Element Element { get; set; }
        internal Action<T, int> Renderer { get; set; }
    }
}
