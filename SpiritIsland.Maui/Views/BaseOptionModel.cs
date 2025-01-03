using System.Windows.Input;

namespace SpiritIsland.Maui;

public class BaseOptionModel : ObservableModel, IOptionModel {

	public BaseOptionModel(IOption option) {
		Option = option;
		Tapped = new Command(OnTapped);
	}

	#region Observable IOptionModel

	public OptionState State { get => _state; set => SetProp(ref _state, value); }
	OptionState _state = OptionState.Default;

	public IOption Option { get; }

	// OVM registeres its callback here.
	public Action<IOption, bool>? SelectOptionCallback { set; private get; }

	#endregion Observable IOptionModel

	public ICommand Tapped { get; }

	async void OnTapped(object obj) {
		if( ++_tapCount != 1 ) return;

		await Task.Delay(400);
		await MainThread.InvokeOnMainThreadAsync(() => {
			switch( _tapCount ) {
				case 1: SelectOptionCallback?.Invoke(Option, false); break;
				case 2: SelectOptionCallback?.Invoke(Option, true); break;
				default: break;
			}
			_tapCount = 0;
		});
	}

	int _tapCount = 0;
}


