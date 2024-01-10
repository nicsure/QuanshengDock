using QuanshengDock.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanshengDock.RepeaterBook
{
    public class BookContext
    {
        public static BookContext Instance { get; } = new();

        public ViewModel<object?> BookActions { get; } = new(nameof(BookActions));
        public ViewModel<string> BookCallsign { get; } = new(string.Empty, nameof(BookCallsign));
        public ViewModel<string> BookCity { get; } = new(string.Empty, nameof(BookCity));
        public ViewModel<string> BookCountry { get; } = new(string.Empty, nameof(BookCountry));
        public ViewModel<string> BookCounty { get; } = new(string.Empty, nameof(BookCounty));
        public ViewModel<string> BookState { get; } = new(string.Empty, nameof(BookState));
        public ViewModel<string> BookRegion { get; } = new(string.Empty, nameof(BookRegion));
        public ViewModel<string> BookFrequency { get; } = new(string.Empty, nameof(BookFrequency));
        public ViewModel<string> BookMode { get; } = new(string.Empty, nameof(BookMode));
        public ViewModel<bool> BookIdle { get; } = new(true, nameof(BookIdle));
        public ViewModel<ObservableCollection<Repeater>> BookResults { get; } = new(new(), nameof(BookResults));
        public ViewModel<string> BookMessage { get; } = new(string.Empty, nameof(BookMessage));
        public ViewModel<ObservableCollection<Repeater>> BookSelected { get; } = new(new(), nameof(BookSelected));

        public BookContext()
        {
            Repeater.Activator++;
        }
    }
}
