using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SpiritIsland.WinForms;

public class SpaceButton( SpaceLayout layout, Func<XY, XY> mapWorldToClient ) : IButton {

	public bool Enabled { private get; set; }

	bool IButton.Contains( Point clientCoords ) 
		// => Bounds.Contains( clientCoords );
		=> Polygons.IsInside( ClientXY, clientCoords.ToXY() );

	public void PaintAbove( Graphics graphics ) {
		if(Enabled) {
			// Draw smoothy
			// using Brush yellowBrush = new SolidBrush(Color.FromArgb(64,Color.Yellow));
			// graphics.FillClosedCurve( yellowBrush, Inner, FillMode.Alternate, .25f );

			using Pen yellowPen = new Pen(Color.FromArgb(128,Color.Yellow), INSET_PEN_WIDTH );
			graphics.DrawClosedCurve( yellowPen, ClientPointF, .25f, FillMode.Alternate );

			//using Pen highlightPen = new( Color.Aquamarine, 5 );
			//graphics.DrawEllipse( highlightPen, Bounds.InflateBy( 2 ) );
		}
	}

	#region private readonly fields

	PointF[] ClientPointF => Polygons.InnerPoints( ClientXY, OFFSET )
		.Select( XY_Extensions.ToPointF )
		.ToArray(); // don't cache, this is used so infrenquently

	XY[] ClientXY => !CacheIsStale ? _clientXYCache 
		: (_clientXYCache = _corners.Select( _worldToClient ).ToArray());

	bool CacheIsStale => _clientXYCache == null
		|| _worldToClient( _corners[0] ) != _clientXYCache[0]
		|| _worldToClient( _corners[1] ) != _clientXYCache[1];

	XY[] _clientXYCache;			// Client XY - dynamic, changes when mapper changes
	readonly XY[] _corners = layout.Corners;
	readonly Func<XY, XY> _worldToClient = mapWorldToClient;

	const float INSET_PEN_WIDTH = 30f;
	
	// this is negative because Screen's Y-flip caused the polygon to flip CCW <=> CW
	const float OFFSET = -INSET_PEN_WIDTH / 2;

	#endregion private readonly fields

}
