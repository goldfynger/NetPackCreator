using System;

namespace NetPackCreator
{
    /// <summary></summary>
    /// <typeparam name="TData"></typeparam>
    public sealed class DataEventArgs<TData> : EventArgs
    {
        /// <summary></summary>
        /// <param name="value"></param>
        public DataEventArgs(TData value) { this.Value = value; }

        /// <summary></summary>
        public TData Value { get; private set; }
    }
}