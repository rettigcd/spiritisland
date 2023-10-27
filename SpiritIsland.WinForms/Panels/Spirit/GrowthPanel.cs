using System;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

public sealed class GrowthPanel : IPanel , IDisposable {

	public GrowthPanel( SharedCtx ctx ) {
		_ctx = ctx; 
	}

	public Rectangle Bounds { get; private set; }

	public int OptionCount => _buttonContainer.ActivatedOptions;

	public bool HasFocus { get; set; }

	public int ZIndex => 2;

	public void ActivateOptions( IDecision decision ) {
		_buttonContainer.EnableOptions( decision );
	}

	public void AssignBounds( RegionLayoutClass regionLayout ) {
		Bounds = regionLayout.GrowthRect;
		_growthPainter = new GrowthPainter( new GrowthLayout( _ctx._spirit, _buttonContainer, Bounds ) );
	}

	public Action GetClickableAction( Point clientCoords ) {
		IOption option = _buttonContainer.FindEnabledOption( clientCoords );
		return option != null ? (() => _ctx.SelectOption( option ))  // if we have option, select it
			: null;
	}

	public RegionLayoutClass GetLayout( Rectangle bounds ) => RegionLayoutClass.ForGrowthFocused( bounds,_ctx._spirit.Decks.Length);

	public void OnGameLayoutChanged() {
		_buttonContainer.Clear();
		foreach(GrowthActionFactory action in _ctx._spirit.GrowthTrack.Options.SelectMany( optionGroup => optionGroup.GrowthActions ))
			_buttonContainer.Add( action, new GrowthButton() );
	}

	public void Paint( Graphics graphics ) {
		_growthPainter.Paint( graphics, HasFocus );
		_buttonContainer.Paint( graphics );
	}

	public void Dispose() {
		_growthPainter?.Dispose();
	}

	readonly SharedCtx _ctx;
	GrowthPainter _growthPainter;
	readonly VisibleButtonContainer _buttonContainer = new VisibleButtonContainer();

}