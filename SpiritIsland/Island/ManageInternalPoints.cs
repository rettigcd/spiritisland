using System.Drawing;

namespace SpiritIsland;

// Tracks unique locations internal to a polygon
public class ManageInternalPoints {

	public PointF NameLocation { get; }

	#region constructor

	public ManageInternalPoints( SpaceState ss ) {
		const float stepSize = .06f; // .07
		const float minDistanceFromBoarder = .012f; //.015

		NameLocation = ss.Space.Layout.GetInternalHexPoints( .02f )
			.Where( p => minDistanceFromBoarder < ss.Space.Layout.DistanceFromBorder(p) )
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
		_borderPoints = points.Except( _internalPoints )
			.OrderByDescending( ss.Space.Layout.DistanceFromBorder )
			.ToArray();

		_internalTokens = new IVisibleToken[_internalPoints.Length];
		_borderTokens = new IVisibleToken[_borderPoints.Length];
		_dict = new Dictionary<IVisibleToken, PointF>();
	}

	#endregion

	public PointF GetPointFor( IVisibleToken token ) {

		if(_dict.ContainsKey(token)) return _dict[token];

		// This is called when we are requesting location for a token that is not currently on this space.
		// !!! Add Border options here

		// !!! what if they ask for explorer location????

		// Find a Randome fresh / unused spot
		int? pick = token.Class.Category switch {
			TokenCategory.Dahan or
			TokenCategory.Presence => FindRightOpenInternalSlot(),
			TokenCategory.Blight or
			TokenCategory.Invader => FindLeftOpenInternalSlot(),
			_ => FindRandomNewSlot(), // !!! this needs to use the border also.
		};

		if(pick.HasValue) {
			AssignInternalToken( token, pick.Value );
			return _dict[token];
		}

		throw new Exception("Unable find open slot.");
	}

	static void Validate( IToken token ) {
		if(token is not IVisibleToken)
			throw new InvalidOperationException( "Token {token} does not appear on screen." );
	}

	void AssignInternalToken( IVisibleToken token, int index ) {
		Validate( token );
		lock(_locker) {
			if( _internalTokens[index] != null )
				throw new InvalidOperationException($"Internal Token Slot {index} already assigned.");
			_internalTokens[index] = token;
			Validate( token );
			_dict[token] = _internalPoints[index];
		}
	}
	readonly object _locker = new object();

	public ManageInternalPoints Init( SpaceState allTokens ) {

		// invader groups
		var groups = allTokens.Keys.OfType<HumanToken>()
			.Where(x=>x.Class.Category == TokenCategory.Invader)
			.GroupBy(x=>x.Class)
			.ToArray();
		foreach(var group in groups)
			InitInvaderGroup(group.Key,group,allTokens);

		// non-invaders
		var nonInvaders = allTokens.Keys
			.Where( x => x.Class.Category != TokenCategory.Invader )
			.OfType<IVisibleToken>();
		foreach(IVisibleToken nonInvader in nonInvaders )
			AssignPointFor(nonInvader,allTokens);

		return this;
	}

	#region private helper methods

	void AssignPointFor( IVisibleToken token, SpaceState spaceState ) {

		if(token.Class.Category == TokenCategory.Invader)
			throw new Exception( "invaders not handled here" );

		// Already assigned
		if(_dict.ContainsKey( token )) return;

		// --------------
		// -- Internal --
		// --------------
		// Find a Randome fresh / unused spot
		int? pick = token.Class.Category switch {
			TokenCategory.Dahan or
			TokenCategory.Presence => FindRightOpenInternalSlot(),
			TokenCategory.Blight or
			TokenCategory.Invader => FindLeftOpenInternalSlot(),
			_ => FindRandomNewSlot(),
		};

		if(pick.HasValue) {
			AssignInternalToken(token, pick.Value);
			return;
		}

		pick = FindRandomOldSlot( spaceState ); // can be combined with switch above once we solve what is picking used spots.
		if(pick.HasValue) {
			AssignInternalToken( token, pick.Value );
			return;
		}

		// --------------
		// -- Border ----
		// --------------
		// Try to find on the Boarder
		for(int i = 0; i < _borderPoints.Length; i++) {
			IToken slotToken = _borderTokens[i];
			if(slotToken is null || spaceState[slotToken] == 0) {
				_borderTokens[i] = token;
				Validate( token );
				_dict[token] = _borderPoints[i];
				return;
			}
		}
		throw new InvalidOperationException( $"Ran out of slots for tokens." );

	}

	void InitInvaderGroup( HumanTokenClass tokenClass, IEnumerable<HumanToken> tokenGroup, SpaceState allTokens ) {

		HumanToken[] GetRegisteredTokens() => _dict.Keys
			.OfType<HumanToken>()
			.Where( x => x.Class == tokenClass )
			.ToArray();

		// Order: damaged first
		var orderedTokens = tokenGroup
			.OrderByDescending( x => x.StrifeCount )
			.ThenByDescending( x => x.Damage )
			.ToArray();
		foreach(var token in orderedTokens) {

			var registeredTokens = GetRegisteredTokens();

			// Take the most-damaged (closest to the begining) vacated spot
			var moreDamagedRegistered = registeredTokens
				.OrderByDescending( x => x.StrifeCount )
				.ThenByDescending( x => x.Damage )
				.Where( t => allTokens[t] == 0 && _dict.ContainsKey( t ) )
				.FirstOrDefault();
			if( moreDamagedRegistered is not null ) {
				Validate( token );
				_dict[token] = _dict[moreDamagedRegistered];
				_dict.Remove( moreDamagedRegistered );
				continue;
			}

			// Already assigned ()
			if(_dict.ContainsKey( token ))
				continue;

			// Only steal more-healthy (closest to the end) spots, if this is brand new and not yet registered
			var moreHealthyRegistered = registeredTokens
				.OrderBy( x => x.StrifeCount )
				.ThenBy( x => x.Damage )
				.FirstOrDefault();
			if(moreHealthyRegistered is not null && moreHealthyRegistered.Damage < token.Damage) {
				Validate( token );
				_dict[token] = _dict[moreHealthyRegistered];
				_dict.Remove( moreHealthyRegistered );
				continue;
			}

			// Find Left-most-open
			int? invaderPick = FindLeftOpenInternalSlot();
			if(invaderPick.HasValue) {
				AssignInternalToken(token,invaderPick.Value);
				continue;
			}

			invaderPick = FindRandomOldSlot( allTokens );
			if(invaderPick.HasValue) {
				AssignInternalToken( token, invaderPick.Value );
				continue;
			}


			// return FindBorderPoint( token, allTokens );
			throw new Exception( "border points not set up." );
		}

	}

	int? FindRightOpenInternalSlot() {
		int? bestIndex = null;
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

	int? FindLeftOpenInternalSlot() {
		int? bestIndex = null;
		float mostLeft = float.MaxValue;
		for(int i = 0; i < _internalPoints.Length; i++)
			if(_internalTokens[i] is null && _internalPoints[i].X < mostLeft ) {
				bestIndex = i;
				mostLeft = _internalPoints[i].X;
			}
		return bestIndex;
	}

	//int? FindLeftOpenBoarderSlot() {
	//	int? bestIndex = 0;
	//	float mostLeft = float.MaxValue;
	//	for(int i = 0; i < _borderPoints.Length; i++)
	//		if(_borderTokens[i] is null && _borderPoints[i].X < mostLeft) {
	//			bestIndex = i;
	//			mostLeft = _borderPoints[i].X;
	//		}
	//	return bestIndex;
	//}

	int? FindRandomNewSlot() {
		for(int i = 0; i < _internalPoints.Length; i++)
			if(_internalTokens[i] is null)
				return i;
		return null;
	}

	int? FindRandomOldSlot( SpaceState spaceState ) {
		// Find an old unused spot that is no longer used.
		for(int i = 0; i < _internalPoints.Length; i++) {
			var token = _internalTokens[i];
			if(spaceState[ token ] == 0) {
				_internalTokens[i] = null;
				return i;
			}
		}
		return null;
	}


	#endregion

	#region private token location fields

	// These are the points available
	readonly PointF[] _internalPoints;    // completely inside
	readonly PointF[] _borderPoints; // inside but near border

	// Token assignments
	public readonly Dictionary<IVisibleToken, PointF> _dict; // !!! only temp public

	// ??? What the hell are these?
	readonly IVisibleToken[] _internalTokens;
	readonly IVisibleToken[] _borderTokens;

	#endregion

}

