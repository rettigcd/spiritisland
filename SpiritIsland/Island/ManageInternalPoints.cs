using System.Drawing;

namespace SpiritIsland;

// Tracks unique locations internal to a polygon
public class ManageInternalPoints {

	public PointF NameLocation { get; }

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
		_internalPoints = ss.Space.Layout
			.GetInternalHexPoints( stepSize )
			.Where( p => stepSize*.6f < ss.Space.Layout.DistanceFromBorder( p ) )
			.ToArray();
		new Random( ss.Space.Text.GetHashCode() ) // use the randomizer every time so pieces don't bounce around when we resize
			.Shuffle( _internalPoints );

		// border - backup
		_orderedBorderPoints = points.Except( _internalPoints )
			.OrderByDescending( ss.Space.Layout.DistanceFromBorder )
			.ToArray();

		_internalTokens = new Token[_internalPoints.Length];
		_borderTokens = new Token[_orderedBorderPoints.Length];
		_dict = new Dictionary<Token, PointF>();
	}

	#endregion

	public PointF GetPointFor( Token token ) {
		if(_dict.ContainsKey(token)) return _dict[token];

		// This is called when we are requesting location for a token that is not currently on this space.
		// !!! Add Border options here

		// !!! what if they ask for explorer location????

		// Find a Randome fresh / unused spot
		int? pick = token.Class.Category switch {
			TokenCategory.Dahan or
			TokenCategory.Presence => FindRightOpenSlot(),
			TokenCategory.Blight or
			TokenCategory.Invader => FindLeftOpenSlot(),
			_ => FindRandomNewSlot(),
		};

		if(pick.HasValue) {
			_internalTokens[pick.Value] = token;
			_dict[token] = _internalPoints[pick.Value];
			return _dict[token];
		}

		throw new Exception("Unable find open slot.");
	}

	public ManageInternalPoints Init( SpaceState allTokens ) {

		// invader groups
		var groups = allTokens.Keys.OfType<HealthToken>()
			.Where(x=>x.Class.Category == TokenCategory.Invader)
			.GroupBy(x=>$"{x.StrifeCount}:{x.Class.Label}")
			.ToArray();
		foreach(var group in groups)
			InitInvaderGroup(group.Key,group,allTokens);

		// non-invaders
		foreach(var nonInvader in allTokens.Keys.Where(x=>x.Class.Category != TokenCategory.Invader))
			AssignPointFor(nonInvader,allTokens);

		return this;
	}

	#region private helper methods

	void AssignPointFor( Token token, SpaceState allTokens ) {

		if(token.Class.Category == TokenCategory.Invader)
			throw new Exception( "invaders not handled here" );

		// Already assigned
		if(_dict.ContainsKey( token )) return;

		// Find a Randome fresh / unused spot
		int? pick = token.Class.Category switch {
			TokenCategory.Dahan or
			TokenCategory.Presence => FindRightOpenSlot(),
			TokenCategory.Blight or
			TokenCategory.Invader => FindLeftOpenSlot(),
			_ => FindRandomNewSlot(),
		} ?? FindRandomOldSlot( allTokens );

		if(pick.HasValue) {
			_internalTokens[pick.Value] = token;
			_dict[token] = _internalPoints[pick.Value];
			return;
		}

		// Try to find on the Boarder
		for(int i = 0; i < _orderedBorderPoints.Length; i++) {
			Token slotToken = _borderTokens[i];
			if(slotToken is null || allTokens[slotToken] == 0) {
				_borderTokens[i] = token;
				_dict[token] = _orderedBorderPoints[i];
				return;
			}
		}
		throw new InvalidOperationException( "ran out of slots for tokens" );

	}

	void InitInvaderGroup( string groupKey, IEnumerable<HealthToken> tokenGroup, SpaceState allTokens ) {

		var registeredTokens = _dict.Keys
			.OfType<HealthToken>()
			.Where( x => $"{x.StrifeCount}:{x.Class.Label}" == groupKey )
			.ToArray();

		var damagedFirst = tokenGroup.OrderByDescending( x => x.Damage ).ToArray();
		foreach(var token in damagedFirst) {

			// Take the most-damaged vacated spot
			var moreDamagedRegistered = registeredTokens
				.OrderByDescending( x => x.Damage )
				.Where( t => allTokens[t] == 0 && _dict.ContainsKey( t ) )
				.FirstOrDefault();
			if(moreDamagedRegistered is not null
			) {
				_dict[token] = _dict[moreDamagedRegistered];
				_dict.Remove( moreDamagedRegistered );
				continue;
			}

			// Already assigned ()
			if(_dict.ContainsKey( token )) {
				continue;
			}

			// Only steal more-healthy spots, if this is brand new and not yet registered
			var moreHealthyRegistered = registeredTokens.OrderBy( x => x.Damage ).FirstOrDefault();
			if(moreHealthyRegistered is not null && moreHealthyRegistered.Damage < token.Damage) {
				_dict[token] = _dict[moreHealthyRegistered];
				_dict.Remove( moreHealthyRegistered );
				continue;
			}

			// Find Left-most-open or Random-old
			int? invaderPick = FindLeftOpenSlot() ?? FindRandomOldSlot( allTokens );
			if(invaderPick.HasValue) {
				_internalTokens[invaderPick.Value] = token;
				_dict[token] = _internalPoints[invaderPick.Value];
				continue;
			}

			//			return FindBorderPoint( token, allTokens );
			throw new Exception( "border points not set up." );
		}

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

	int? FindRandomNewSlot() {
		for(int i = 0; i < _internalPoints.Length; i++)
			if(_internalTokens[i] is null)
				return i;
		return null;
	}

	int? FindRandomOldSlot( SpaceState allTokens ) {
		// Find an old unused spot that is no longer used.
		for(int i = 0; i < _internalPoints.Length; i++)
			if(allTokens[_internalTokens[i]] == 0)
				return i;
		return null;
	}


	#endregion

	#region private token location fields

	readonly Token[] _internalTokens;
	readonly Token[] _borderTokens;
	readonly Dictionary<Token, PointF> _dict;

	readonly PointF[] _internalPoints;    // completely inside
	readonly PointF[] _orderedBorderPoints; // inside but near border

	#endregion

}

