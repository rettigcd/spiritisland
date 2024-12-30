using SpiritIsland.An;
using System.Windows.Input;

namespace SpiritIsland.Maui;

/// <summary>
/// For Elements-as-options.  No #.,
/// Tab or Double Tap
/// </summary>
/// <param name="element"></param>
public class ElementOptionModel : ObservableModel, IOptionModel {

	public ElementOptionModel(Element element) {
		_element = element;
		Option = new ElementOption(_element, false);
		Tapped = new Command(OnTapped);
	}
	Element _element;

	public ImageSource Image => _element.GetTokenImg().ImgSource();
	// public Element Element => element

	#region Observable IOptionModel

	public OptionState State { 
		get => _state; 
		set => SetProp(ref _state, value); }
	OptionState _state = OptionState.Default;
	public IOption Option { get; }

	// OVM registeres its callback here.
	public Action<IOption, bool>? SelectOptionCallback { set; private get; }

	#endregion Observable IOptionModel

	public ICommand Tapped { get; }

	async void OnTapped(object obj) {
		if(++_tapCount != 1) return;

		if( MainThread.IsMainThread ) {
			int j=22;
		}

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