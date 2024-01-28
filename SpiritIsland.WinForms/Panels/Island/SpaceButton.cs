using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace SpiritIsland.WinForms;

public class SpaceButton : IButton {

	#region constructor
	public SpaceButton( SpaceLayout layout, Func<PointF,PointF> mapWorldToClient ) {

		_worldToClient = mapWorldToClient;

		static RowVector ToRowVector(PointF p) => new RowVector( p.X, p.Y );

		_inner = new PointF[layout.Corners.Length];
		RowVector prev = ToRowVector( _inner[layout.Corners.Length-1] );
		RowVector cur = ToRowVector( layout.Corners[0] ); // new RowVector( Vector2 ;
		for(int i = 0; i < _inner.Length; ++i) {
			// points
			RowVector next = ToRowVector( layout.Corners[(i+1)%_inner.Length] );
			// arrows to next and previous points
			RowVector forward = (next-cur).ToUnitLength();
			RowVector back = (prev-cur).ToUnitLength(); 
			// direction to go to be inside
			RowVector go = (forward+back);
			// if points are in a straight line, go will be 0, correct it by turning 90 from forward direction.
			if(go.LengthSquared() < 0.00001f ) 
				go = new RowVector(forward.Y,-forward.X);
			else if(0<(cur.X - prev.X)*(next.Y - prev.Y) - (cur.Y - prev.Y)*(next.X - prev.X)) 
				go = -go;
			
			var newPoint = cur + go *.02f;
			_inner[i] = new PointF( newPoint.X, newPoint.Y );

			// advance
			prev = cur;
			cur = next;
		}

	}
	#endregion

	bool IButton.Contains( Point clientCoords) 
		// => Bounds.Contains( clientCoords );
		=> Polygons.IsInside( Inner, clientCoords );

	public void Paint( Graphics graphics, bool enabled ) {
		if(enabled) {
			// Draw smoothy
			// using Brush yellowBrush = new SolidBrush(Color.FromArgb(64,Color.Yellow));
			// graphics.FillClosedCurve( yellowBrush, Inner, FillMode.Alternate, .25f );

			using Pen yellowPen = new Pen(Color.FromArgb(128,Color.Yellow),20f);
			graphics.DrawClosedCurve( yellowPen, Inner, .25f, FillMode.Alternate );

			//using Pen highlightPen = new( Color.Aquamarine, 5 );
			//graphics.DrawEllipse( highlightPen, Bounds.InflateBy( 2 ) );
		}
	}

	#region private readonly fields

	PointF[] Inner => _inner.Select( _worldToClient ).ToArray();
	readonly PointF[] _inner;
	readonly Func<PointF,PointF> _worldToClient;

	#endregion private readonly fields

}
