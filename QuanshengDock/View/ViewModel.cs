using QuanshengDock.General;
using QuanshengDock.User;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace QuanshengDock.View
{
    public class VMEquality<T>
    {
        private readonly T value;
        public VMEquality(T value) => this.value = value;
        public bool this[T key] => key?.Equals(value) ?? false;
    }

    public delegate void CommandReceivedEventHandler(object sender, CommandReceievedEventArgs e);

    public class CommandReceievedEventArgs : EventArgs
    {
        public object? Parameter { get; private set; }
        public CommandReceievedEventArgs(object? parameter) => Parameter = parameter;        
    }

    public abstract class VM : ICommand
    {
        private static readonly SavedDictionary backing = new(UserFolder.File("app.config"));
        protected static SavedDictionary Backing => backing;

        protected abstract bool IsSetting { get; set; }
        private static readonly Dictionary<string, VM> viewModels = new();
        public static ViewModel<T> Get<T>(string name) => (ViewModel<T>)viewModels[name];
        public static VM Get(string name) => viewModels[name];
        protected static void Add(VM vm)
        {
            if(!viewModels.ContainsKey(vm.Name))
                viewModels.Add(vm.Name, vm);
        }

        public event EventHandler? CanExecuteChanged;
        public event CommandReceivedEventHandler? CommandReceived;

        protected virtual void OnCanExecuteChanged() => (_ = CanExecuteChanged)?.Invoke(this, EventArgs.Empty);

        public abstract string Name { get; }

        protected abstract void Save();

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            (_ = CommandReceived)?.Invoke(this, new(parameter));
        }

        public static void SaveBacking()
        {
            foreach (var vm in viewModels.Values)
                vm.Save();
            backing.Save();
        }
    }

    public class ViewModel<T> : VM, INotifyPropertyChanged
    {
        private static readonly PropertyChangedEventArgs eventArgsV = new(nameof(Value));
        private static readonly PropertyChangedEventArgs eventArgsE = new(nameof(EqualTo));

        public event PropertyChangedEventHandler? PropertyChanged;

        private Func<T>? converter = null;
        private Action<T>? convertBack = null;
        private T value_;
        private readonly string name;
        protected override bool IsSetting { get; set; }

        public override string Name => name;
        public VMEquality<T> EqualTo => new(value_);
        public T Value 
        {
            get => converter == null ? value_ : converter();
            set
            {
                if (convertBack == null)
                {
                    if (converter != null)
                        throw new NotSupportedException("No back conversion for this view model has been set");
                    if (value_ == null || !value_.Equals(value))
                    {
                        value_ = value;
                        OnPropertyChanged();
                    }
                }
                else
                    convertBack(value);
            }
        }
        public int ForceUpdate
        {
            get => 0;
            set => OnPropertyChanged();
        }

        public ViewModel(T initialValue, string name, bool setting = false)
        {
            IsSetting = setting;
            this.name = name;
            value_ = GetSaved(initialValue);
            Add(this);
        }
        public ViewModel(string name, bool setting = false)
        {
            IsSetting = setting;
            this.name = name;
            value_ = GetSaved(value_!);
            Add(this);
        }

        protected override void Save()
        {
            if (IsSetting)
            {
                if (value_ is string s)
                {
                    Backing[name] = s;
                }
                else
                if (value_ is IConvertible i)
                {
                    Backing[name] = (string)Convert.ChangeType(i, typeof(string));
                }
                else
                if (value_ is Color c)
                {
                    Backing[name] = c.ToString();
                }
                else
                if(value_ is ColorBrushPen p)
                {
                    Backing[name] = p.ToString();
                }
            }
        }

        private T GetSaved(T deflt)
        {
            if (IsSetting && Backing.TryGetValue(name, out var saved))
            {
                Type t = typeof(T);
                if (t == typeof(string))
                {
                    return (T)(object)saved;
                }
                else
                if (typeof(IConvertible).IsAssignableFrom(t))
                {
                    return (T)Convert.ChangeType(saved, t);
                }
                else
                if (t == typeof(Color))
                {
                    return (T)ColorConverter.ConvertFromString(saved);
                }
                else
                if (t == typeof(ColorBrushPen))
                {
                    return (T)(object)ColorBrushPen.FromString(saved);
                }
            }
            return deflt;
        }

        public void SetConversion(Func<T> converter, Action<T> convertBack, params INotifyPropertyChanged[] parents)
        {
            SetConverter(converter, parents);
            SetConvertBack(convertBack);
        }
        public void SetConverter(Func<T> converter, params INotifyPropertyChanged[] parents)
        {
            this.converter = converter;
            foreach(var parent in parents)
                parent.PropertyChanged += (object? sender, PropertyChangedEventArgs e) => OnPropertyChanged();
        }
        public void SetConvertBack(Action<T> convertBack) => this.convertBack = convertBack;

        protected virtual void OnPropertyChanged()
        {
            (_ = PropertyChanged)?.Invoke(this, eventArgsV);
            (_ = PropertyChanged)?.Invoke(this, eventArgsE);
        }

    }


}
