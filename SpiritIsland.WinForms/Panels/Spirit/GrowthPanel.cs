using System;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

public sealed class GrowthPanel : IPanel , IDisposable {

	public GrowthPanel( SharedCtx ctx ) {
		_ctx = ctx;
		InitGrowthRow();
	}

	void InitGrowthRow(){
		_growthRow = new RowRect_WithPadding( 
			[ .._ctx._spirit.GrowthTrack.Options.Select( BuildPaintable ) ]
		) {
			Padding = 0.05f,
			Separation = 0.05f
		};
	}

	// Growth-Option
	RowRect_WithPadding BuildPaintable( GrowthOption op ) {
		return new RowRect_WithPadding([ 
			..op.GrowthActions.Cast<SpiritGrowthAction>()
				.Select( BuildPaintable ) 
		]) {
			Background = Color.FromArgb( 255, 255, 220 ),
			Border = Color.FromArgb( 230, 230, 198 ),
			Padding = .05f,
			Separation = .05f
		};
	}

	// Growth-Action
	PaintableGrowthAction BuildPaintable( SpiritGrowthAction action ) {
		var paintable = new PaintableGrowthAction( action, _ctx );
		_cc.RegisterOption(action,paintable);
		return paintable;
	}

	RowRect_WithPadding _growthRow;

	public Rectangle Bounds { get; private set; }

	public int OptionCount => _cc.OptionCount;

	public bool HasFocus { get; set; }

	public int ZIndex => 2;

	public void ActivateOptions( IDecision decision ) {
		_cc.ActivateOptions( decision );
	}

	public void AssignBounds( RegionLayoutClass regionLayout ) {
		// Dispose BG image
		_staticBackgroundImage?.Dispose(); _staticBackgroundImage = null;

		Rectangle proposedBounds = regionLayout.GrowthRect;
		SizeF localSize = new SizeF( _growthRow.WidthRatio.Value, 1f );

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

	public IClickable GetClickableAction( Point clientCoords ) {
		return HasFocus ? _cc.GetClickableAt(clientCoords) : null;
	}

	public RegionLayoutClass GetLayout( Rectangle bounds ) => RegionLayoutClass.ForGrowthFocused( bounds,_ctx._spirit.Decks.Length);

	public void OnGameLayoutChanged() {
		InitGrowthRow(); // regenerate in case they got a new growth option
		// Dispose BG image
		_staticBackgroundImage?.Dispose(); _staticBackgroundImage = null;
	}

	public void Paint( Graphics graphics ) {

		_staticBackgroundImage ??= BuildBackgroundImage();

		if(HasFocus) {
			using var bgBrush = new SolidBrush( Color.FromArgb( 220, 230, 230, 198 ) );
			graphics.FillRectangle( bgBrush, Bounds );
			graphics.DrawRectangle( Pens.Black, Bounds );
		}

		graphics.DrawImage( _staticBackgroundImage, Bounds );

		foreach(var above in _cc.PaintAboves )
			above.PaintAbove( graphics );
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

	readonly SharedCtx _ctx;
	Bitmap _staticBackgroundImage;

	readonly ClickableContainer _cc = new ClickableContainer();

}
