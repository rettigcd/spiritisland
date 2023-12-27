using System;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

public sealed class GrowthPanel : IPanel , IDisposable {

	public GrowthPanel( SharedCtx ctx ) {
		_ctx = ctx;
		_growthRow = new PaintableRow( _ctx._spirit.GrowthTrack.Options.Select(BuildPaintable).ToArray() );
		_growthRow.Padding = 0.05f;
		_growthRow.Separation = 0.05f;
	}

	PaintableRow BuildPaintable( GrowthOption op ) {
		var actionRects = op.GrowthActions.Cast<SpiritGrowthAction>().Select( BuildPaintable ).ToArray();
		var paintable = new PaintableRow( actionRects );
		paintable.BackgroundColor = Color.FromArgb( 255, 255, 220 );
		paintable.BorderColor = Color.FromArgb( 230, 230, 198 );
		paintable.Padding = .05f;
		paintable.Separation = .05f;
		return paintable;
	}

	PaintableGrowthAction BuildPaintable( SpiritGrowthAction action ) {
		var paintable = new PaintableGrowthAction( action );
		paintable.BoundsChanged += Paintable_BoundsChanged;
		return paintable;
	}

	void Paintable_BoundsChanged( PaintableGrowthAction paintable ) {
		((GrowthButton)_buttonContainer[paintable.Action]).Bounds = paintable.Bounds;
	}

	readonly PaintableRow _growthRow;

	public Rectangle Bounds { get; private set; }

	public int OptionCount => _buttonContainer.ActivatedOptions;

	public bool HasFocus { get; set; }

	public int ZIndex => 2;

	public void ActivateOptions( IDecision decision ) {
		_buttonContainer.EnableOptions( decision );
	}

	public void AssignBounds( RegionLayoutClass regionLayout ) {
		// Dispose BG image
		_staticBackgroundImage?.Dispose(); _staticBackgroundImage = null;

		Rectangle proposedBounds = regionLayout.GrowthRect;
		SizeF localSize = new SizeF( _growthRow.WidthRatio, 1f );

		// Scale to Fit to Bounds
		float scaler = Math.Min( 
			proposedBounds.Width / localSize.Width, 
			proposedBounds.Height / localSize.Height 
		);
		var size = localSize.Scale( scaler ).ToSize();

		// Alignment
		int leftSpacer = proposedBounds.Width - size.Width; // right-aligning put all left-over space on the left
		// For Align-top,  topSpacer = 0

		// Update Bounds so we don't use realestate we don't need.
		Bounds = new Rectangle( proposedBounds.Left+ leftSpacer, proposedBounds.Top, size.Width, size.Height );

	}

	public Action GetClickableAction( Point clientCoords ) {
		if(!HasFocus) return null;

		IOption option = _buttonContainer.FindEnabledOption( clientCoords );
		return option != null ? (() => _ctx.SelectOption( option ))  // if we have option, select it
			: null;
	}

	public RegionLayoutClass GetLayout( Rectangle bounds ) => RegionLayoutClass.ForGrowthFocused( bounds,_ctx._spirit.Decks.Length);

	public void OnGameLayoutChanged() {
		_buttonContainer.Clear();
		foreach(IHelpGrow action in _ctx._spirit.GrowthTrack.Options.SelectMany( optionGroup => optionGroup.GrowthActions ))
			_buttonContainer.Add( action, new GrowthButton() );
	}

	public void Paint( Graphics graphics ) {

		_staticBackgroundImage ??= BuildBackgroundImage();

		if(HasFocus) {
			using var bgBrush = new SolidBrush( Color.FromArgb( 220, 230, 230, 198 ) );
			graphics.FillRectangle( bgBrush, Bounds );
			graphics.DrawRectangle( Pens.Black, Bounds );
		}

		graphics.DrawImage( _staticBackgroundImage, Bounds );

		_buttonContainer.Paint( graphics );
	}

	Bitmap BuildBackgroundImage() {
		using var optionPen = new Pen( Color.Blue, 6f );

		var cachedImageLayer = new Bitmap( Bounds.Width, Bounds.Height );
		using var g = Graphics.FromImage( cachedImageLayer );
		g.TranslateTransform( -Bounds.X, -Bounds.Y );
		g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

		_growthRow.Paint( g, Bounds );

		return cachedImageLayer;
	}

	public void Dispose() {
		_staticBackgroundImage?.Dispose();
	}

	readonly VisibleButtonContainer _buttonContainer = new VisibleButtonContainer();
	readonly SharedCtx _ctx;
	Bitmap _staticBackgroundImage;

}
