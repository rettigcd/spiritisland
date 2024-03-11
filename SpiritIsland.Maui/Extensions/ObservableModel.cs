//using Android.OS;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SpiritIsland.Maui;

public class ObservableModel : INotifyPropertyChanged {

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void SetProp<T>( ref T field, T newValue, [CallerMemberName] string propName = "" ) {
		if(Object.ReferenceEquals( field, newValue ) 
			|| field is not null && field.Equals(newValue) 
		) return;

		field = newValue;
		PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
	}

}

public class ObservableModel1 : INotifyPropertyChanged {

	public event PropertyChangedEventHandler? PropertyChanged;

	protected void SetProp<T>(T newValue, [CallerMemberName] string propName = "") {
		if (_values.TryGetValue(propName, out object? t) && t is not null && t.Equals(newValue)) return;
		_values[propName] = newValue;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
	}

	protected T GetProp<T>([CallerMemberName] string propName = "") where T : class {
#pragma warning disable IDE0046 // Convert to conditional expression
		if (_values.TryGetValue(propName, out object? t) && t is not null)
			return (T)t;
#pragma warning restore IDE0046 // Convert to conditional expression
		throw new Exception($"non null property {propName} not initialied");
	}

	protected T? GetNullableProp<T>([CallerMemberName] string propName = "") where T : class {
		_values.TryGetValue(propName, out object? t);
		return (T?)t;
	}

	protected T GetStruct<T>([CallerMemberName] string propName = "") where T : struct {
		return (_values.TryGetValue(propName, out object? t) && t is not null) ? (T)t : default;
	}

	protected int GetInt([CallerMemberName] string propName = "") {
		return (_values.TryGetValue(propName, out object? t) && t is not null) ? (int)t : default;
	}


	readonly Dictionary<string, object?> _values = [];
}
