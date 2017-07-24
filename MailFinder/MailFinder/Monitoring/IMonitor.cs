using System.Collections.Generic;

namespace nucs.Monitoring {

    public delegate void EnteredHandler<in T>(T item);
    public delegate void LeftHandler<in T>(T item);

    public interface IMonitor<T> {

        /// <summary>
        ///     Core function to this pattern - fetches the items that are monitored to change.
        /// </summary>
        IEnumerable<T> FetchCurrent();

        /// <summary>
        ///     When in a new fetch, new items are entering, they will be passed in this event.
        /// </summary>
        event EnteredHandler<T> Entered;

        /// <summary>
        ///     When in a new fetch items are leaving, they will be passed in this event.
        /// </summary>
        event LeftHandler<T> Left; 
    }
}