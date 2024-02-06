using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SpiritIsland.WinForms;

// new spirit Painter each time layout / size changes
public sealed class SpiritPanel : IPanel, IDisposable {

	public SpiritPanel( SharedCtx ctx ){
		_ctx = ctx;
		_innateClump = InnatePainter.GetAllInnatesClump(_ctx,_cc);
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

		Func<Bitmap> getSpiritImage = () => ResourceImages.Singleton.LoadSpiritImage(_spirit.Text);
		_spiritImageRect = new ClickableLocation(new ImgRect( getSpiritImage ), ShowSpecialRules );

		_cc.AddStatic(_spiritImageRect);
		_spiritImageBounds = regionLayout.SpiritImageBounds;

		// Update the Painters
		Dispose();
		
		_elementLayout = new ElementLayout( regionLayout.ElementRect );

	}

	IPaintableRect _innateClump;
	IPaintableRect PresenceRect => _presenceRect ??= PresenceTrackPainter.GetPaintablePresenceTracks( _cc, _ctx );
	IPaintableRect _presenceRect;

	public Rectangle Bounds => _bounds;

	public int OptionCount => _buttonContainer.ActivatedOptions;

	public void Dispose() {
	}

	void Paint_Innates( Graphics graphics ) {
		_innateClump.Paint( graphics, _innateBounds );
	}

	void Paint_Elements( Graphics graphics ) {
		// activated elements
		var visibleElements = _spirit.Elements.Elements;
		var mainElements = ActivatedElementsRect( visibleElements );
		mainElements.Paint(graphics,_elementLayout.Bounds);

		if(_spirit is IHaveSecondaryElements hasSecondaryElements){
			Rectangle secondary = _elementLayout.Bounds.OffsetBy( (int)((mainElements.WidthRatio+1)*_elementLayout.Bounds.Height), 0);
			ActivatedElementsRect( hasSecondaryElements.SecondaryElements ).Paint(graphics, secondary );
		}
	}

	public static IPaintableRect ActivatedElementsRect( CountDictionary<Element> elements ) {
		var elementRects = ElementList.AllElements
			.Where(element => 0<elements[element])
			.Select(element => {
				var img = new ImgRect( element.GetTokenImg() );
				int count = elements[element];
				return count==1 ? (IPaintableRect)img
					: (IPaintableRect)img.FloatSelf_WithRatio().Float(new SubScriptRect($"x{count}") );
			} )
			.ToArray();
		return new RowRect(FillFrom.Left, elementRects);
	}

	public void OnGameLayoutChanged() {
		_buttonContainer.Clear();
		_cc.ClearAllClickables();

		_innateClump = InnatePainter.GetAllInnatesClump(_ctx,_cc);
		_presenceRect = null;
	}
	Spirit _spirit => _ctx._spirit;

	readonly SharedCtx _ctx;

	public Rectangle _bounds;
	Rectangle _innateBounds;
	Rectangle _presenceTractBounds;

	public void Paint( Graphics graphics ) {
		if(_bounds == default)
			return;

		graphics.FillRectangle( SpiritPanelBackgroundBrush, _bounds );

		using ImgMemoryCache imgCache = new ImgMemoryCache();

		_spiritImageRect.Paint( graphics, _spiritImageBounds );

		// _presencePainter.Paint( graphics );
		PresenceRect.Paint( graphics, _presenceTractBounds.FitBoth(PresenceRect.WidthRatio.Value,Align.Near) );

		Paint_Innates( graphics );
		Paint_Elements( graphics );

		// =====  Misc  =====
		_buttonContainer.Paint( graphics );
		_cc.PaintAbove( graphics );

	}

	public void ActivateOptions( IDecision decision ) {
		_buttonContainer.EnableOptions( decision );
		_cc.ActivateOptions( decision );
	}
	public static Brush SpiritPanelBackgroundBrush => Brushes.LightYellow;

	public IClickable GetClickableAction( Point clientCoords ) {

		IClickable clickable = _cc.GetClickableAt( clientCoords );
		if(clickable is not null) return clickable;

		IOption option = _buttonContainer.FindEnabledOption( clientCoords );
		if(option != null) 
			return new GenericClickable(() => _ctx.SelectOption( option ));  // if we have option, select it

		return null;
	}

	void ShowSpecialRules() {
		string msg = _ctx._spirit.SpecialRules.Select( r => r.ToString() ).Join( "\r\n\r\n" );
		MessageBox.Show( msg );
	}

	public ElementLayout _elementLayout;

	ClickableLocation _spiritImageRect;
	Rectangle _spiritImageBounds;

	readonly ClickableContainer _cc = new();

	public readonly VisibleButtonContainer _buttonContainer = new VisibleButtonContainer();

}
