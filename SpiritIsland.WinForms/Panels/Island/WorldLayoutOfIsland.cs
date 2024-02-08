using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms.Panels.Island;

/// <summary>
/// Caches the Board & Space locations Moves boards around in the (0,0,1.5,.866) space so they fit together.
/// </summary>
class WorldLayoutOfIsland {

	public readonly RectangleF Bounds;

	#region constructor
	public WorldLayoutOfIsland( SpiritIsland.Island island ) {

		foreach(var board in island.Boards)
			foreach(var space in board.Spaces) {
				var spaceLayout = _islandLayout.GetLayoutFor( space );
				var paintable = space is MultiSpace ms ? new PaintableMultiSpace(ms,spaceLayout) 
					: new PaintableSpace(space,spaceLayout);
				_paintables.Add( space, paintable );
			}

		Bounds = CalcBounds( _paintables.Values.Select(ip=>ip.Bounds) )
			.InflateBy( .015f ); // because the drawing algo is curvey and goes outside the bounds
	}

	#endregion constructor

	/// <summary> Get's layout for spaces of different types (multi, endless-dark, normal) </summary>
	public ManageInternalPoints InsidePoints( Space space ) {
		return GetPaintable(space)._insidePoints;
	}

	public PaintableSpace GetPaintable( Space space ) {
		if(!_paintables.TryGetValue( space, out PaintableSpace paintable )) {
			var spaceLayout = _islandLayout.GetLayoutFor( space );
			paintable = space is MultiSpace ms ? new PaintableMultiSpace( ms, spaceLayout )
				: new PaintableSpace( space, spaceLayout );
			_paintables.Add( space, paintable );
		}
		return paintable;
	}

	#region private

	static RectangleF CalcBounds( IEnumerable<RectangleF> rects ) {
		float left = float.MaxValue;
		float top = float.MaxValue;
		float right = float.MinValue;
		float bottom = float.MinValue;

		foreach(var e in rects) {
			if(e.Left < left) left = e.Left;
			if(e.Top < top) top = e.Top;
			if(e.Right > right) right = e.Right;
			if(e.Bottom > bottom) bottom = e.Bottom;
		}
		return new RectangleF( left, top, right - left, bottom - top );
	}

	readonly Dictionary<Space, PaintableSpace> _paintables = [];

	readonly IslandLayout _islandLayout = new IslandLayout();

	#endregion private
}
