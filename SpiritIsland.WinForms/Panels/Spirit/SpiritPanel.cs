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

	void SetLayout() {
		Dispose();

		_spiritLayout = new SpiritLayout( _ctx._spirit, _bounds, 10, _buttonContainer );
		_growthPainter = new GrowthPainter( _spiritLayout.growthLayout );
		_presencePainter = new PresenceTrackPainter( _spirit, _spiritLayout.trackLayout, _ctx._tip );
		_innatePainters = _spirit.InnatePowers
			.Select( power => new InnatePainter( power, _spiritLayout.findLayoutByPower[power] ) )
			.ToArray();
	}

	public Rectangle Bounds {
		get {
			return _bounds;
		}
		set {
			_bounds = value;
			_spiritLayout = null; // invalidate
		}
	}

	public void Dispose() {
		_growthPainter?.Dispose();

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
		Rectangle bounds = _spiritLayout.imgBounds;
		var image = _spiritImage ??= ResourceImages.Singleton.LoadSpiritImage(_spirit.Text);
		graphics.DrawImageFitBoth(image,bounds);
	}

	void Paint_Elements( Graphics graphics ) {
		// activated elements
		DrawActivatedElements( graphics, _spirit.Elements, _spiritLayout.Elements );
		int skip = _spirit.Elements.Keys.Count; 
		if(skip>1) skip++; // add a space
		if(_spirit is IHaveSecondaryElements hasSecondaryElements)
			DrawActivatedElements( graphics, hasSecondaryElements.SecondaryElements, _spiritLayout.Elements, skip );
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

		foreach(InnatePower power in _ctx._spirit.InnatePowers) {
			_buttonContainer.Add( power, new InnateButton() );
			foreach(IDrawableInnateOption innatePowerOption in power.DrawableOptions)
				_buttonContainer.Add( innatePowerOption, new InnateOptionsBtn( _ctx._spirit, innatePowerOption ) );
		}
		var presenceImg = _ctx._tip._presenceImg;
		foreach(Track energySlot in _ctx._spirit.Presence.Energy.Slots)
			_buttonContainer.Add( energySlot, new PresenceSlotButton( _ctx._spirit.Presence.Energy, energySlot, presenceImg ) );

		foreach(Track cardSlot in _ctx._spirit.Presence.CardPlays.Slots)
			_buttonContainer.Add( cardSlot, new PresenceSlotButton( _ctx._spirit.Presence.CardPlays, cardSlot, presenceImg ) );

		foreach(GrowthActionFactory action in _ctx._spirit.GrowthTrack.Options.SelectMany( optionGroup => optionGroup.GrowthActions ))
			_buttonContainer.Add( action, new GrowthButton() );

	}


	Image GetElementImage( Element element ) => _ctx._tip.GetElementImage( element );
	Spirit _spirit => _ctx._spirit;

	readonly SharedCtx _ctx;
	Image _spiritImage;
	InnatePainter[] _innatePainters;
	PresenceTrackPainter _presencePainter;
	GrowthPainter _growthPainter;
	public readonly VisibleButtonContainer _buttonContainer = new VisibleButtonContainer();

	public Rectangle _bounds;

	public void Paint( Graphics graphics ) {
		if(_spiritLayout == null) {
			if(_bounds == default)
				return;
			SetLayout();
		}
		graphics.FillRectangle( SpiritPanelBackgroundBrush, _bounds );

		using CachedImageDrawer imageDrawer = new CachedImageDrawer();
		PaintSpiritImage( graphics );
		_growthPainter.Paint( graphics );
		_presencePainter.Paint( graphics, imageDrawer );
		Paint_Innates( graphics, imageDrawer );
		Paint_Elements( graphics );

		// =====  Misc  =====
		_buttonContainer.Paint( graphics );
	}

	public void ActivateOptions( IDecision decision ) {
		_buttonContainer.EnableOptions( decision );
		options_GrowthActions = decision.Options.OfType<GrowthActionFactory>().ToArray(); // Need to update when starlight adds new
	}

	static Brush SpiritPanelBackgroundBrush => Brushes.LightYellow;

	public GrowthActionFactory[] options_GrowthActions;

	public Action GetClickableAction( Point clientCoords ) {
		IOption option = _buttonContainer.FindEnabledOption( clientCoords );
		return option != null
				? (() => _ctx.SelectOption( option ))
			: _spiritLayout != null && _spiritLayout.imgBounds.Contains( clientCoords )
				? ShowSpecialRules
			: null;
	}
	void ShowSpecialRules() {
		string msg = _ctx._spirit.SpecialRules.Select( r => r.ToString() ).Join( "\r\n\r\n" );
		MessageBox.Show( msg );
	}

	public SpiritLayout _spiritLayout;

}