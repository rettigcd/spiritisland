using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

// new spirit Painter each time layout / size changes
public sealed class SpiritPanel : IPanel, IDisposable {

	public SpiritPanel( SharedCtx ctx ) {
		_ctx = ctx;
	}

	public RegionLayoutClass GetLayout( Rectangle bounds ) {
		return RegionLayoutClass.ForIslandFocused( bounds, _ctx._spirit.Decks.Length+1 ); // everything else
	}

	public bool HasFocus { set { } }

	public int ZIndex => 1;

	public void AssignBounds( RegionLayoutClass regionLayout ) {
		_bounds = regionLayout.SpiritRect;
		_innateBounds = regionLayout.InnateRect;
		_presenceTractBounds = regionLayout.PresenceTractRect;
		_spiritImageBounds = regionLayout.SpiritImageBounds;

		// Update the Painters
		Dispose();
		var presenceLayout = new PresenceTrackLayout( _spirit, _buttonContainer, _presenceTractBounds );
		_presencePainter = new PresenceTrackPainter( _spirit, presenceLayout, _ctx._tip );
		var innatesLayout = new InnatesLayout( _spirit, _buttonContainer, _innateBounds );
		_innatePainters = _spirit.InnatePowers
			.Select( power => new InnatePainter( power, innatesLayout.FindLayoutByInnate[power] ) )
			.ToArray();
		_elementLayout = new ElementLayout( regionLayout.ElementRect );

	}

	public Rectangle Bounds => _bounds;

	public int OptionCount => _buttonContainer.ActivatedOptions;

	public void Dispose() {
		_presencePainter?.Dispose();

		if(_innatePainters is not null)
			foreach(var ip in _innatePainters)
				ip.Dispose();
	}

	void Paint_Innates( Graphics graphics, CachedImageDrawer imageDrawer ) {
		foreach(var painter in _innatePainters)
			painter.DrawFromLayout( graphics, imageDrawer );
	}

	void PaintSpiritImage( Graphics graphics ) {
		Image image = _spiritImage ??= ResourceImages.Singleton.LoadSpiritImage(_spirit.Text);
		graphics.DrawImageFitBoth(image, _spiritImageBounds );
	}

	void Paint_Elements( Graphics graphics ) {
		// activated elements
		DrawActivatedElements( graphics, _spirit.Elements, _elementLayout );
		int skip = _spirit.Elements.Keys.Count; 
		if(skip>1) skip++; // add a space
		if(_spirit is IHaveSecondaryElements hasSecondaryElements)
			DrawActivatedElements( graphics, hasSecondaryElements.SecondaryElements, _elementLayout, skip );
	}

	void DrawActivatedElements( Graphics graphics, ElementCounts elements, ElementLayout elLayout, int skip=0 ) {

		var orderedElements = elements.Keys.OrderBy( el => (int)el );
		int idx = skip;
		foreach(var element in orderedElements) {
			var rect = elLayout.Rect(idx++);
			graphics.DrawImage( GetElementImage( element ), rect );
			graphics.DrawCountIfHigherThan( rect, elements[element]);
		}
	}

	public void OnGameLayoutChanged() {
		_buttonContainer.Clear();

		// Innates
		foreach(InnatePower power in _ctx._spirit.InnatePowers) {
			_buttonContainer.Add( power, new InnateButton() );
			foreach(IDrawableInnateOption innatePowerOption in power.DrawableOptions)
				_buttonContainer.Add( innatePowerOption, new InnateOptionsBtn( _ctx._spirit, innatePowerOption ) );
		}
		if(0 < _innateBounds.Width) {
			var innates = new InnatesLayout( _ctx._spirit, _buttonContainer, _innateBounds );
			_innatePainters = _spirit.InnatePowers
				.Select( power => new InnatePainter( power, innates.FindLayoutByInnate[power] ) )
				.ToArray();
		}

		// Presence
		var presenceImg = _ctx._tip._presenceImg;
		foreach(Track energySlot in _ctx._spirit.Presence.Energy.Slots)
			_buttonContainer.Add( energySlot, new PresenceSlotButton( _ctx._spirit.Presence.Energy, energySlot, presenceImg ) );
		foreach(Track cardSlot in _ctx._spirit.Presence.CardPlays.Slots)
			_buttonContainer.Add( cardSlot, new PresenceSlotButton( _ctx._spirit.Presence.CardPlays, cardSlot, presenceImg ) );
		if(0 < _presenceTractBounds.Width ) {
			var presenceLayout = new PresenceTrackLayout( _spirit, _buttonContainer, _presenceTractBounds );
			_presencePainter = new PresenceTrackPainter( _spirit, presenceLayout, _ctx._tip );
		}
	}


	Image GetElementImage( Element element ) => _ctx._tip.GetElementImage( element );
	Spirit _spirit => _ctx._spirit;

	readonly SharedCtx _ctx;
	Image _spiritImage;
	InnatePainter[] _innatePainters;
	PresenceTrackPainter _presencePainter;
	public readonly VisibleButtonContainer _buttonContainer = new VisibleButtonContainer();

	public Rectangle _bounds;
	Rectangle _innateBounds;
	Rectangle _presenceTractBounds;

	public void Paint( Graphics graphics ) {
		if(_bounds == default)
			return;

		graphics.FillRectangle( SpiritPanelBackgroundBrush, _bounds );

		using CachedImageDrawer imageDrawer = new CachedImageDrawer();
		PaintSpiritImage( graphics );
		_presencePainter.Paint( graphics, imageDrawer );
		Paint_Innates( graphics, imageDrawer );
		Paint_Elements( graphics );

		// =====  Misc  =====
		_buttonContainer.Paint( graphics );

	}

	public void ActivateOptions( IDecision decision ) {
		_buttonContainer.EnableOptions( decision );
	}
	public static Brush SpiritPanelBackgroundBrush => Brushes.LightYellow;

	public Action GetClickableAction( Point clientCoords ) {
		IOption option = _buttonContainer.FindEnabledOption( clientCoords );
		if(option != null) return (() => _ctx.SelectOption( option ));  // if we have option, select it
		return _spiritImageBounds.Contains( clientCoords ) ? ShowSpecialRules // clicked image
			: null;
	}
	void ShowSpecialRules() {
		string msg = _ctx._spirit.SpecialRules.Select( r => r.ToString() ).Join( "\r\n\r\n" );
		MessageBox.Show( msg );
	}

	public ElementLayout _elementLayout;
	public Rectangle _spiritImageBounds;

}
