using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OwnCloud.Data
{
    /// <summary>
    /// Fires a given complete routine if
    /// all registered handler are fired properly
    /// </summary>
    class EventCollector
    {
        List<object> _handler = new List<object>();

        /// <summary>
        /// The action delegate routine.
        /// </summary>
        public Action Complete
        {
            get;
            set;
        }

        /// <summary>
        /// Registers a single object to wait for.
        /// </summary>
        /// <param name="mixed">A eventhandler or any object identifying the event.</param>
        public void WaitFor(object mixed)
        {
            if (!_handler.Contains(mixed)) _handler.Add(mixed);
        }

        /// <summary>
        /// Removes a object from the wait list and fires the registered routine
        /// if all events are fired up.
        /// </summary>
        /// <param name="mixed">A eventhandler or any object identifying the event.</param>
        public void Raise(object mixed)
        {
            if (_handler.IndexOf(mixed) >= 0)
            {
                _handler.RemoveAt(_handler.IndexOf(mixed));
                if (_handler.Count == 0)
                {
                    if (Complete != null) Complete();
                }
            }
        }
    }
}
