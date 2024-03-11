using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SpiritIsland.WinForms.Panels.Island;

class PaintableSpace {

	#region Static Resrouces

	static Color SpacePerimeterColor => Color.Black;
	static Brush SpaceLabelBrush => Brushes.White;

	static Color SacredSiteColor => Color.Yellow;

	#endregion

	public RectangleF Bounds => _layout.Bounds.ToRectangleF();

	public PointMapper Mapper { 
		get => _mapper;
		set { _mapper = value; _iconWidth = (int)(Mapper.UnitLength * .075f); }
	}

	/// <summary>
	/// Must be set before PaintAbove is called.
	/// </summary>
	public SpaceState Tokens { get; set; }

	public Dictionary<SpaceToken, Rectangle> Locations = [];

	#region constructor

	public PaintableSpace( Space space, SpaceLayout layout	) {
		_space = space;
		_layout = layout;
		_insidePoints = new ManageInternalPoints( _layout );
	}

	#endregion constructor

	public void Paint( Graphics graphics ) {
		using Brush brush = ResourceImages.Singleton.UseSpaceBrush( _space );
		PointF[] points = _layout.Corners.Select( Mapper.Map ).Select(XY_Extensions.ToPointF).ToArray();

		// Draw smoothy
		graphics.FillClosedCurve( brush, points, FillMode.Alternate, .25f );
		using Pen perimeterPen = new Pen( SpacePerimeterColor, 5f );
		graphics.DrawClosedCurve( perimeterPen, points, .25f, FillMode.Alternate );

		// Draw Label
		PointF nameLocation = Mapper.Map( _insidePoints.NameLocation ).ToPointF();
		graphics.DrawString( _space.Text, SystemFonts.MessageBoxFont, SpaceLabelBrush, nameLocation );
	}

	public void PaintAbove( Graphics graphics ) {
		// Init the Inside points and Locations
		_insidePoints.Init( Tokens );
		Locations.Clear();
		// Do Paint
		DoPaintAbove( graphics );
	}

	#region protected / private methods

	protected virtual void DoPaintAbove( Graphics graphics ) {
		var orderedTokens = Tokens.Keys.OfType<IToken>()
			.OrderBy( OrderTokens );

		foreach(var token in orderedTokens)
			PaintToken( graphics, token );

		static int OrderTokens( IToken t ) => t is not HumanToken human ? 0
			: -human.FullHealth * 100       // healthy first
				+ human.StrifeCount * 10   // minimum strife
				+ human.RemainingHealth;     // most damaged
	}

	public void PaintAbove_Debug( Graphics graphics ) {
		int iconWidth = (int)(Mapper.UnitLength * .075f); // !!! make this 

		var ghosts = ModCache.Ghosts( ResourceImages.Singleton );

		foreach(var (token, point) in _insidePoints.Assignments()) {
			using Image img = ghosts.GetImage( token.Img );
			var clientPoint = Mapper.Map( point );
			Rectangle rect = new Rectangle(
				new Point(
					(int)(clientPoint.X - iconWidth / 2),
					(int)(clientPoint.Y - iconWidth / 2)
				),
				img.Size.FitWidth( (int)iconWidth )
			);
			graphics.DrawImage( img, rect );
		}
	}

	void PaintToken( Graphics graphics, IToken token ) {
		// Separate Strife
		(IToken baseToken,int strifeCount) = token is HumanToken human 
			? (human.HavingStrife(0),human.StrifeCount) 
			: (token,0);
		using var imgMgr = ImageSpec.From( baseToken ).GetResourceMgr();
		Bitmap img = imgMgr.Resource;

		// calc rect
		Point center = Mapper.Map( _insidePoints.GetPointFor( token ) ).ToInts();
		Point topLeft = new Point( center.X - _iconWidth/2, center.Y - _iconWidth/2 );
		Rectangle rect = new Rectangle( topLeft.X, topLeft.Y, _iconWidth, _iconWidth )
			.FitBoth( img.Size );

		// Strife
		if(0 < strifeCount) {
			using var strifeMgr = ((ImageSpec)Img.Strife).GetResourceMgr();
			Rectangle strifeRect = new Rectangle( topLeft, strifeMgr.Resource.Size.FitWidth( _iconWidth ) );
			graphics.DrawImage( strifeMgr.Resource, strifeRect );
			if(1 < strifeCount)
				graphics.DrawSuperscript( strifeRect, "x" + strifeCount );
		}

		// Sacred Site
		if(token is SpiritPresenceToken presenceToken && presenceToken.Self.Presence.IsSacredSite( Tokens )) {
			const int inflationSize = 10;
			rect.Inflate( inflationSize, inflationSize );
			using var brush = new SolidBrush( Color.FromArgb( 100, SacredSiteColor ) );
			graphics.FillEllipse( brush, rect );
			rect.Inflate( -inflationSize, -inflationSize );
		}

		// Draw: Base Image
		graphics.DrawImage( img, rect );

		// Count
		PaintCount( graphics, token, rect );

		// Record Location
		Locations[token.On( Tokens )] = rect;
	}

	void PaintCount( Graphics graphics, IToken token, Rectangle rect ) {
		int count = Tokens[token];
		if(1 < count)
			new SubScriptRect( "x" + count ).Paint( graphics, rect );
	}

	#endregion protected / private methods

	#region private fields

	PointMapper _mapper;
	int _iconWidth;

	readonly Space _space;
	readonly SpaceLayout _layout;
	readonly public ManageInternalPoints _insidePoints;

	#endregion private fields
}
