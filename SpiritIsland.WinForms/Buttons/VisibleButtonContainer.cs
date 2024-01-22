using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms;

/// <summary>
/// Binds Buttons to Options and manages their visibility.
/// </summary>
public class VisibleButtonContainer {

	/// <summary> Binds a button to a static Option </summary>
	/// <remarks>When button is clicked, bound option will be selected.</remarks>
	/// <param name="boundStaticOption"></param>
	/// <param name="btn"></param>
	public void Add( IOption boundStaticOption, IButton btn ) {
		_staticLookupButtonByOption.Add( boundStaticOption, btn );
	}

	/// <summary> Binds a button to a transient option, enabling the button. </summary>
	public void AddTransientEnabled( IOption option, IButton btn ) {
		_enabled.Add( option, btn );
		_transient.Add( option );
	}

	public IButton this[IOption option] => _staticLookupButtonByOption.TryGetValue( option, out IButton button ) ? button 
		: _enabled[option];						// enabled (static + transient)

	public int ActivatedOptions => _enabled.Count;

	public void ClearTransient() {
		foreach(IOption x in _transient) {
			_enabled.Remove( x );
			_staticLookupButtonByOption.Remove( x );
		}
		_transient.Clear();
	}

	public void Clear() {
		_staticLookupButtonByOption.Clear();
		_enabled.Clear();
	}

	public void DisableAll() {
		foreach(var pair in _enabled)
			_staticLookupButtonByOption.Add( pair.Key, pair.Value );
		_enabled.Clear();
	}

	public void EnableOptions( IDecision decision ) {
		DisableAll();
		foreach(IOption option in decision.Options)
			if(_staticLookupButtonByOption.ContainsKey( option ))
				EnableSingle( option, decision );
	}

	void EnableSingle( IOption option, IDecision decision ) {
		var button = _staticLookupButtonByOption[option]; // grab old
		_staticLookupButtonByOption.Remove( option );

		_enabled.Add( option, button ); // add to new
		button.SyncDataToDecision( decision );
	}

	public IOption FindEnabledOption( Point clientCoords ) {
		foreach(var pair in _enabled)
			if(pair.Value.Contains( clientCoords )) return pair.Key;
		return null;
	}

	public void Paint( Graphics graphics ) {
		static void PaintButtonDict( Graphics graphics, Dictionary<IOption, IButton> dict, bool enabled ) {
			foreach(IButton btn in dict.Values)
				btn.Paint( graphics, enabled );
		}
		PaintButtonDict( graphics, _staticLookupButtonByOption, false );
		PaintButtonDict( graphics, _enabled, true );
	}

	readonly Dictionary<IOption, IButton> _enabled = [];
	readonly Dictionary<IOption, IButton> _staticLookupButtonByOption = [];
	readonly List<IOption> _transient = [];

}