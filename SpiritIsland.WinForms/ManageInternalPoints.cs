using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;

namespace SpiritIsland.WinForms;

class ManageInternalPoints {

	public readonly PointF[] _internalPoints;

	public readonly PointF[] _orderedBorderPoints;

	readonly Token[] _internalTokens;
	readonly Token[] _borderTokens;

	readonly Dictionary<Token, PointF> dict;

	public ManageInternalPoints( SpaceState ss, float stepSize ) {

		var points = ss.Space.Layout
			.GetInternalHexPoints( stepSize )
			.ToArray();

		// internal - prefered
		_internalPoints = ss.Space.Layout
			.GetInternalHexPoints( stepSize )
			.Where( p => stepSize*.75f < ss.Space.Layout.DistanceFromBorder( p ) )
			.ToArray();
		new Random( ss.Space.Text.GetHashCode() ) // use the randomizer every time so pieces don't bounce around when we resize
			.Shuffle( _internalPoints );

		// border - backup
		_orderedBorderPoints = points.Except( _internalPoints )
			.OrderByDescending( ss.Space.Layout.DistanceFromBorder )
			.ToArray();

		_internalTokens = new Token[_internalPoints.Length];
		_borderTokens = new Token[_orderedBorderPoints.Length];
		dict = new Dictionary<Token, PointF>();
	}
	public PointF GetPointFor( Token token, SpaceState allTokens ) {

		// !! Maybe we should sweep border slots and clear out any not used
		// so that once something uses the border slot, it isn't stuck there

		// Already assigned
		if(dict.ContainsKey( token )) return dict[token];

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
			return dict[token] = _internalPoints[pick.Value];
		}

		// Find an old unused spot that is no longer used.
		for(int i = 0; i < _internalPoints.Length; i++) {
			Token slotToken = _internalTokens[i];
			if(allTokens[slotToken] == 0) {
				_internalTokens[i] = token;
				dict[token] = _internalPoints[i];
				return _internalPoints[i];
			}
		}

		// BORDER - Find an old unused spot that is no longer used.
		for(int i = 0; i < _orderedBorderPoints.Length; i++) {
			Token slotToken = _borderTokens[i];
			if(slotToken is null || allTokens[slotToken] == 0) {
				_borderTokens[i] = token;
				dict[token] = _orderedBorderPoints[i];
				return _orderedBorderPoints[i];
			}
		}

		throw new InvalidOperationException( "ran out of slots for tokens" );
	}

	int? FindRightOpenSlot() {
		int? bestIndex = 0;
		float mostRight = float.MinValue;
		for(int i = 0; i < _internalPoints.Length; i++)
			if(_internalTokens[i] is null
				&& mostRight < _internalPoints[i].X
			) {
				bestIndex = i;
				mostRight = _internalPoints[i].X;
			}
		return bestIndex;
	}

	int? FindLeftOpenSlot() {
		int? bestIndex = 0;
		float mostLeft = float.MaxValue;
		for(int i = 0; i < _internalPoints.Length; i++)
			if(_internalTokens[i] is null
				&& _internalPoints[i].X < mostLeft
			) {
				bestIndex = i;
				mostLeft = _internalPoints[i].X;
			}
		return bestIndex;
	}


	int? FindRandomOpenSlot() {
		for(int i = 0; i < _internalPoints.Length; i++)
			if(_internalTokens[i] is null)
				return i;
		return null;
	}
}

