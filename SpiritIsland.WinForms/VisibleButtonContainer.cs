using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms;

public class VisibleButtonContainer {

	public void Add(IOption option, IButton btn ) {
		_disabled.Add( option, btn );
	}

	public IButton this[ IOption option] {
		get {
			return _disabled.ContainsKey( option ) 
				? _disabled[option] 
				: _enabled[option];
		}
	}

	public void Clear() {
		_disabled.Clear();
		_enabled.Clear();
	}

	public void DisableAll() {
		foreach(var pair in _enabled)
			_disabled.Add(pair.Key,pair.Value);
		_enabled.Clear();
	}

	public void Enable(IOption option ) {
		if(_disabled.ContainsKey( option )) {
			_enabled.Add( option, _disabled[option]);
			_disabled.Remove( option );
		}
	}

	public IOption FindEnabledOption( Point clientCoords ) {
		foreach(var pair in _enabled )
			if(pair.Value.Bounds.Contains( clientCoords )) return pair.Key;
		return null;
	}

	public void Paint( Graphics graphics ) {
		static void PaintButtonDict( Graphics graphics, Dictionary<IOption, IButton> dict, bool enabled ) {
			foreach(IButton btn in dict.Values)
				btn.Paint( graphics, enabled );
		}
		PaintButtonDict( graphics, _disabled, false );
		PaintButtonDict( graphics, _enabled, true );
	}

	readonly Dictionary<IOption, IButton> _enabled = new Dictionary<IOption, IButton>();
	readonly Dictionary<IOption, IButton> _disabled = new Dictionary<IOption, IButton>();

}