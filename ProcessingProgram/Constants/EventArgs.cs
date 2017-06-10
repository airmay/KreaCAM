using System;
using ProcessingProgram.Objects;

namespace ProcessingProgram.Constants
{
    /// <summary>
    /// Аргумент событий
    /// </summary>
    /// <typeparam name="T">тип аргумента</typeparam>
    public class EventArgs<T> : EventArgs
    {
        public T Data { get; private set; }
        public EventArgs(T data)
        {
            Data = data;
        }
    }

    public class ProcessObjectEventArgs : EventArgs<ProcessObject> 
    {
        public ProcessObjectEventArgs(ProcessObject processObject) : base(processObject) { }
    }

    public static class EventExtensions
    {
        public static void Raise(this EventHandler @event, object sender)
        {
            if (@event != null)
            {
                @event(sender, EventArgs.Empty);
            }
        }

        public static void Raise<T>(this EventHandler<T> @event, object sender, T args) where T : EventArgs
        {
            if (@event != null)
            {
                @event(sender, args);
            }
        }

        public static void Raise<T>(this EventHandler<EventArgs<T>> @event, object sender, T args)
        {
            if (@event != null)
            {
                @event(sender, new EventArgs<T>(args));
            }
        }

        public static void Raise(this EventHandler<ProcessObjectEventArgs> @event, object sender, ProcessObject processObject)
        {
            if (@event != null)
            {
                @event(sender, new ProcessObjectEventArgs(processObject));
            }
        }
    }
}