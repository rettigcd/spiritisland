using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SpiritIsland.WinForms;

public class SpaceButton : IButton {

	public bool Enabled { private get; set; }

	#region constructor

	public SpaceButton( SpaceLayout layout, Func<XY, XY> mapWorldToClient ) {
		_innerWorld = Polygons.InnerPoints( layout.Corners, .02f );
		_worldToClient = mapWorldToClient;
	}

	#endregion

	bool IButton.Contains( Point clientCoords ) 
		// => Bounds.Contains( clientCoords );
		=> Polygons.IsInside( ClientXY, clientCoords.ToXY() );

	public void PaintAbove( Graphics graphics ) {
		if(Enabled) {
			// Draw smoothy
			// using Brush yellowBrush = new SolidBrush(Color.FromArgb(64,Color.Yellow));
			// graphics.FillClosedCurve( yellowBrush, Inner, FillMode.Alternate, .25f );

			using Pen yellowPen = new Pen(Color.FromArgb(128,Color.Yellow),20f);
			graphics.DrawClosedCurve( yellowPen, ClientPointF, .25f, FillMode.Alternate );

			//using Pen highlightPen = new( Color.Aquamarine, 5 );
			//graphics.DrawEllipse( highlightPen, Bounds.InflateBy( 2 ) );
		}
	}

	#region private readonly fields

	PointF[] ClientPointF => ClientXY.Select( XY_Extensions.ToPointF ).ToArray(); // don't cache, this is used so infrenquently

	XY[] ClientXY => !CacheIsStale ? _clientXYCache 
		: (_clientXYCache = _innerWorld.Select( _worldToClient ).ToArray());

	bool CacheIsStale => _clientXYCache == null
		|| _worldToClient( _innerWorld[0] ) != _clientXYCache[0]
		|| _worldToClient( _innerWorld[1] ) != _clientXYCache[1];

	XY[] _clientXYCache;			// Client XY - dynamic, changes when mapper changes
	readonly XY[] _innerWorld;      // World coordinates - Static, don't change
	readonly Func<XY, XY> _worldToClient;

	#endregion private readonly fields

}
