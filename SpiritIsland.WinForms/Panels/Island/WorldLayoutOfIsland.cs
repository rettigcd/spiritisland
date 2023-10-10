using SpiritIsland.NatureIncarnate;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace SpiritIsland.WinForms.Panels.Island;

// world layout of island
class WorldLayoutOfIsland {

	public WorldLayoutOfIsland( SpiritIsland.Island island ) {

		float left = float.MaxValue;
		float top = float.MaxValue;
		float right = float.MinValue;
		float bottom = float.MinValue;

		foreach(var board in island.Boards) {
			BoardLayout boardLayout = LoadLayoutForBoard( board );
			_boardLayouts.Add( board, boardLayout );
			RectangleF e = boardLayout.CalcExtents();
			if(e.Left < left) left = e.Left;
			if(e.Top < top) top = e.Top;
			if(e.Right > right) right = e.Right;
			if(e.Bottom > bottom) bottom = e.Bottom;
		}
		const float CUSHION = .015f; // because the drawing algo is curvey and goes outside the bounds
		Bounds = new RectangleF(
			left - CUSHION,
			top - CUSHION,
			right - left + 2 * CUSHION,
			bottom - top + 2 * CUSHION
		);

		_insidePoints = new Dictionary<Space, ManageInternalPoints>();
	}

	static BoardLayout LoadLayoutForBoard( Board board ) {
		Matrix3D transform = board.Orientation.GetTransformMatrix();
		return BoardLayout.Get( board.Name )
			.ReMap( transform );
	}

	public SpaceLayout GetSpaceLayout( Space1 s1 ) {
		if(!_spaceLayouts.ContainsKey( s1 ))
			_spaceLayouts.Add(
				s1,
				_boardLayouts[s1.Board].ForSpace( s1 )
			);
		return _spaceLayouts[s1];
	}

	SpaceLayout _endlessDarkLayout;

	public SpaceLayout MySpaceLayout( Space space ) {
		if(space == EndlessDark.Space) {
			const float x = .1f;
			const float y = .9f;
			const float f = .2f;
			return _endlessDarkLayout ??= new SpaceLayout(new PointF( x+0, y ),new PointF( x+f, y ),new PointF( x+f, y-f ),new PointF( x+0, y-f ));
		}

		if(space is Space1 s1)
			return GetSpaceLayout( s1 );

		if(space is MultiSpace ms) {
			var spaces = ms.OrigSpaces;
			var merged = MySpaceLayout( spaces[0] ).Corners;
			for(int i = 1; i < spaces.Length; ++i)
				merged = Polygons.JoinAdjacentPolgons( merged, MySpaceLayout( spaces[i] ).Corners );
			return new SpaceLayout( merged );
		}
		throw new ArgumentException( "Unknown space type" );
	}

	public ManageInternalPoints InsidePoints( Space space ) {
		// In case Weave-Together has occurred and the event hasn't propogated here yet.
		if(!_insidePoints.ContainsKey( space ))
			_insidePoints.Add( space, new ManageInternalPoints( space, MySpaceLayout( space ) ) );
		return _insidePoints[space];
	}

	public PointF GetCoord( Space space, IToken visibileTokens ) {
		return visibileTokens != null
			? InsidePoints( space ).GetPointFor( visibileTokens )
			: MySpaceLayout( space ).Center;
	}


	public readonly RectangleF Bounds;
	readonly Dictionary<Space, SpaceLayout> _spaceLayouts = new Dictionary<Space, SpaceLayout>();
	readonly Dictionary<Board, BoardLayout> _boardLayouts = new Dictionary<Board, BoardLayout>();
	public Dictionary<Space, ManageInternalPoints> _insidePoints;

}
