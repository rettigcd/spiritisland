using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SpiritIsland.Maui;

public class SpaceModel : ObservableModel, OptionView {

	/// <summary> The Space Name, Mods, and Tokens </summary>
	public Space Space { get; }

	/// <summary> Details about where it appears on the screen. </summary>
	public SpaceLayout Layout { get; }

	public Bounds Bounds => Layout.Bounds;
	public XY[] Corners => Layout.Corners; // for calculating optimal rotation

	#region OptionView implementation

	public OptionState State { get => _state; set => SetProp(ref _state, value ); }
	OptionState _state = OptionState.Default;

	public IOption Option => Space.SpaceSpec;

	public Action<IOption, bool>? SelectOptionCallback { set; private get; }

	#endregion OptionView implementation

	public readonly ObservableCollection<TokenLocationModel> Tokens = [];

	#region constructor

	public SpaceModel(Space space, SpaceLayout layout, OptionViewManager ovm) {
		ActionScope.ThrowIfMissingCurrent();

		Space = space;
		Layout = layout;
		_ovm = ovm;
		_ovm.Add(this);
		Sync();

		ClickCommand = new Command(
			execute: () => { if (++_tapCount == 1) HandleClick(); },
			canExecute: () => { return SelectOptionCallback != null; }
		);
	}

	#endregion constructor

	public ICommand ClickCommand { get; }

	/// <summary>
	/// Sync the ViewModel to the (GameState:) Space
	/// Adds / Removes tokens, registering and unregistering them with the OptionViewManager
	/// </summary>
	public void Sync() {
		ActionScope.ThrowIfMissingCurrent(); // !!! remove this in Release mode

		IToken[] currentTokens = [.. Space.Keys.OfType<IToken>()];

		// Calc: Remove
		IToken[] oldTokens = [.. Tokens.Select(t => t.TokenLocation.Token)];
		Dictionary<IToken, TokenLocationModel> oldModels = Tokens.ToDictionary(x => x.TokenLocation.Token, x => x);
		IToken[] toRemove = oldTokens.Except(currentTokens).ToArray();
		IToken[] toAddPre = currentTokens.Except(oldTokens).ToArray();
		IToken[] toUpdate = currentTokens.Intersect(oldTokens).ToArray();

		// Refresh tokens neither added or removed. (Do this BEFORE we Add the new ones.)
		foreach (IToken update in toUpdate)
			oldModels[update].RefreshCountAndSS();

		// Do: Remove
		foreach (IToken oldToken in toRemove) {
			TokenLocationModel tlModel = oldModels[oldToken];
			_ovm!.Remove(tlModel);
			Tokens.Remove(tlModel);
		}

		// Do: Add
		foreach (IToken newToken in toAddPre) {
			TokenLocationModel tlModel = new TokenLocationModel(new SpaceToken(Space, newToken));
			_ovm!.Add(tlModel);
			Tokens.Add(tlModel);
		}

	}

	/// <summary>
	/// Removes all tokens, triggering the release of their UI components
	/// </summary>
	public void ClearTokens() {
		_ovm.RemoveRange(Tokens);
		Tokens.Clear();
	}

	async void HandleClick() {
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

	readonly OptionViewManager _ovm;
	int _tapCount = 0;
}
