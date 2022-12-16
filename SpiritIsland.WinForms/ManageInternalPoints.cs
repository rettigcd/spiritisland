using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

// Tracks unique locations internal to a polygon
class ManageInternalPoints {

	public PointF NameLocation { get; }
	public PointF[] InternalPoints { get; }			// completely inside
	public PointF[] OrderedBorderPoints { get; }	// inside but near border

	#region constructor

	public ManageInternalPoints( SpaceState ss ) {
		const float stepSize = .07f;

		NameLocation = ss.Space.Layout.GetInternalHexPoints( .02f )
			.Where( p => .015f < ss.Space.Layout.DistanceFromBorder(p) )
			.OrderBy( p => { 
				var bounds = ss.Space.Layout.Bounds;
				float dx = p.X-bounds.X;
				float dy = p.Y-bounds.Bottom;
				return dx*dx+dy*dy;
			} )
			.First();

		var points = ss.Space.Layout
			.GetInternalHexPoints( stepSize )
			.ToArray();

		// internal - prefered
		InternalPoints = ss.Space.Layout
			.GetInternalHexPoints( stepSize )
			.Where( p => stepSize*.6f < ss.Space.Layout.DistanceFromBorder( p ) )
			.ToArray();
		new Random( ss.Space.Text.GetHashCode() ) // use the randomizer every time so pieces don't bounce around when we resize
			.Shuffle( InternalPoints );

		// border - backup
		OrderedBorderPoints = points.Except( InternalPoints )
			.OrderByDescending( ss.Space.Layout.DistanceFromBorder )
			.ToArray();

		_internalTokens = new Token[InternalPoints.Length];
		_borderTokens = new Token[OrderedBorderPoints.Length];
		_dict = new Dictionary<Token, PointF>();
	}
	#endregion

	public PointF GetPointFor( Token token, SpaceState allTokens ) {

		// !! Maybe we should sweep border slots and clear out any not used
		// so that once something uses the border slot, it isn't stuck there

		// Already assigned
		if(_dict.ContainsKey( token )) return _dict[token];

		// Find a Randome fresh / unused spot
		int? pick = token.Class.Category switch {
			TokenCategory.Dahan or
			TokenCategory.Presence => FindRightOpenSlot(),
			TokenCategory.Blight or
			TokenCategory.DreamingInvader or
			TokenCategory.Invader  => FindLeftOpenSlot(),
			_                      => FindRandomOpenSlot(),
		};

		if(pick.HasValue) {
			_internalTokens[pick.Value] = token;
			return _dict[token] = InternalPoints[pick.Value];
		}

		// Find an old unused spot that is no longer used.
		for(int i = 0; i < InternalPoints.Length; i++) {
			Token slotToken = _internalTokens[i];
			if(allTokens[slotToken] == 0) {
				_internalTokens[i] = token;
				_dict[token] = InternalPoints[i];
				return InternalPoints[i];
			}
		}

		// BORDER - Find an old unused spot that is no longer used.
		for(int i = 0; i < OrderedBorderPoints.Length; i++) {
			Token slotToken = _borderTokens[i];
			if(slotToken is null || allTokens[slotToken] == 0) {
				_borderTokens[i] = token;
				_dict[token] = OrderedBorderPoints[i];
				return OrderedBorderPoints[i];
			}
		}

		throw new InvalidOperationException( "ran out of slots for tokens" );
	}

	#region private helper methods

	int? FindRightOpenSlot() {
		int? bestIndex = 0;
		float mostRight = float.MinValue;
		for(int i = 0; i < InternalPoints.Length; i++)
			if(_internalTokens[i] is null
				&& mostRight < InternalPoints[i].X
			) {
				bestIndex = i;
				mostRight = InternalPoints[i].X;
			}
		return bestIndex;
	}

	int? FindLeftOpenSlot() {
		int? bestIndex = 0;
		float mostLeft = float.MaxValue;
		for(int i = 0; i < InternalPoints.Length; i++)
			if(_internalTokens[i] is null
				&& InternalPoints[i].X < mostLeft
			) {
				bestIndex = i;
				mostLeft = InternalPoints[i].X;
			}
		return bestIndex;
	}

	int? FindRandomOpenSlot() {
		for(int i = 0; i < InternalPoints.Length; i++)
			if(_internalTokens[i] is null)
				return i;
		return null;
	}

	#endregion

	#region private token location fields

	readonly Token[] _internalTokens;
	readonly Token[] _borderTokens;
	readonly Dictionary<Token, PointF> _dict;

	#endregion

}

