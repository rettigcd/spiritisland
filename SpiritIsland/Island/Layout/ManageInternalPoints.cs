
namespace SpiritIsland;

// Tracks unique locations internal to a polygon
public class ManageInternalPoints {

	public readonly SpaceLayout SpaceLayout;

	public XY NameLocation { get; }

	#region constructor

	public ManageInternalPoints( SpaceLayout layout ) {
		SpaceLayout = layout;

		NameLocation = layout.GetInternalHexPoints( .03f )
			.OrderByDescending( layout.FindDistanceFromBorder )
			.First();

		XY[]? allPoints = null;
		XY[]? internalPoints = null;

		const float tokenSizeScale = 1.5f;
		float stepSize = .1f; // .06 is plenty
		while(true) {
			allPoints = layout
				.GetInternalHexPoints( stepSize )
				.ToArray();

			// internal - prefered
			internalPoints = allPoints
				.Where( p => (stepSize*tokenSizeScale)*.5f < layout.FindDistanceFromBorder( p ) )
				.ToArray();
			if(9 <= internalPoints.Length) break;
			stepSize -= .005f;
		}
		TokenSize = stepSize * tokenSizeScale;

		// border - backup
		XY[] borderPoints = [
			.. allPoints.Except( internalPoints ).OrderByDescending( layout.FindDistanceFromBorder )
		];

		new Random( 1 ) // use the same randomizer every time so pieces don't bounce around when we resize
			.Shuffle( internalPoints );

		_dict = [];
		_randomInternal = new TokenPointArray( _dict, internalPoints );
		_border = new TokenPointArray( _dict, borderPoints );

	}

	public float TokenSize { get; private set; }

	#endregion

	public XY GetPointFor( IToken token ) {

		if(token is null)
			return SpaceLayout.Center;

		if(_dict.TryGetValue( token, out XY storedPoint )) return storedPoint;

		// This is called when we are requesting location for a token that is not currently on this space.

		// Find a Random fresh / unused spot
		bool assigned = AssignLeftRight( token ) || _border.AssignNextSlot( token );

		//bool assigned = token.Class.Category switch {
		//	TokenCategory.Dahan or
		//	TokenCategory.Presence => _randomInternal.AssignRightSlot( token ),
		//	TokenCategory.Blight or
		//	TokenCategory.Invader => _randomInternal.AssignLeftSlot( token ),
		//	_ => _randomInternal.AssignNextSlot( token ),
		//}
		//	|| _border.AssignNextSlot( token );

		return assigned ? _dict[token] : throw new Exception( "Unable find open slot." );
	}

	bool AssignLeftRight( IToken token ) {
		ITag[] rightTags = [ TokenCategory.Incarna, TokenCategory.Dahan, TokenCategory.Presence ];
		ITag[] leftTags = [ Token.Blight, TokenCategory.Invader ];
		return (token.HasAny( rightTags ) ? _randomInternal.AssignRightSlot( token )
			: token.HasAny( leftTags ) ? _randomInternal.AssignLeftSlot( token )
			: _randomInternal.AssignNextSlot( token )
			);
	}

	public ManageInternalPoints Init( Space allTokens ) {

		// invader groups
		var groups = allTokens.AllHumanTokens()
			.Where(x=>x.HumanClass.HasTag(TokenCategory.Invader))
			.GroupBy(x=>x.HumanClass)
			.ToArray();
		foreach(var group in groups)
			InitInvaderGroup(group.Key,group,allTokens);

		// non-invaders
		var nonInvaders = allTokens.OfType<IToken>()
			.Where( x => !x.HasTag(TokenCategory.Invader) );
		foreach(IToken nonInvader in nonInvaders )
			AssignPointFor(nonInvader,allTokens);

		return this;
	}

	public IEnumerable<(IToken Key, XY Value)> Assignments() 
		=> _dict.Select( p => (p.Key, p.Value) );

	#region private helper methods

	void AssignPointFor( IToken token, Space space ) {

		if(token.HasTag(TokenCategory.Invader))
			throw new Exception( "invaders not handled here" );

		// Already assigned
		if(_dict.ContainsKey( token )) return;

		// -- Internal --
		bool assigned = AssignLeftRight( token ) || AssignOldSlot( token, space );

		if(!assigned)
			throw new InvalidOperationException( $"Ran out of slots for tokens." );

	}

	bool AssignOldSlot( IToken token, Space space )
		=> _randomInternal.AssignNextSlot(token, space)
		|| _border.AssignNextSlot(token, space);

	void InitInvaderGroup( HumanTokenClass tokenClass, IEnumerable<HumanToken> tokenGroup, Space allTokens ) {

		HumanToken[] GetRegisteredTokens() => _dict.Keys
			.OfType<HumanToken>()
			.Where( x => x.HumanClass == tokenClass )
			.ToArray();

		// Order: damaged first
		var orderedTokens = tokenGroup
			.OrderByDescending( x => x.StrifeCount )
			.ThenByDescending( x => x.Damage )
			.ToArray();

		foreach(HumanToken token in orderedTokens) {

			HumanToken[] registeredTokens = GetRegisteredTokens();

			// Take the most-damaged (closest to the begining) vacated spot
			var moreDamagedRegistered = registeredTokens
				.OrderByDescending( x => x.StrifeCount )
				.ThenByDescending( x => x.Damage )
				.Where( t => allTokens[t] == 0 && _dict.ContainsKey( t ) )
				.FirstOrDefault();
			if( moreDamagedRegistered is not null ) {
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
				_dict[token] = _dict[moreHealthyRegistered];
				_dict.Remove( moreHealthyRegistered );
				continue;
			}

			// Find Left-most-open
			if(_randomInternal.AssignLeftSlot(token))
				continue;

			// Boarders
			if(_border.AssignNextSlot( token, allTokens ) )
				continue;

			// return FindBorderPoint( token, allTokens );
			throw new Exception( "border points not set up." );
		}

	}

	#endregion

	class TokenPointArray( Dictionary<IToken, XY> _dict, XY[] _points ) {

		public bool AssignLeftSlot( IToken visibleToken ) {
			int? index = FindLeftSlot();
			if(!index.HasValue) return false;
			AssignToken( visibleToken, index.Value );
			return true;
		}

		public bool AssignRightSlot( IToken visibleToken ) {
			int? index = FindRightSlot();
			if(!index.HasValue) return false;
			AssignToken( visibleToken, index.Value );
			return true;
		}

		public bool AssignNextSlot( IToken visibleToken, Space? space= null ) {
			int? index = FindNextSlot(space);
			if(!index.HasValue) return false;
			AssignToken( visibleToken, index.Value );
			return true;
		}

		int? FindLeftSlot() {
			int? bestIndex = null;
			float mostLeft = float.MaxValue;
			for(int i = 0; i < _points.Length; i++)
				if(_myTokens[i] is null
					&& _points[i].X < mostLeft
				) {
					bestIndex = i;
					mostLeft = _points[i].X;
				}
			return bestIndex;
		}

		int? FindRightSlot() {
			int? bestIndex = null;
			float mostRight = float.MinValue;
			for(int i = 0; i < _points.Length; i++)
				if(_myTokens[i] is null
					&& mostRight < _points[i].X
				) {
					bestIndex = i;
					mostRight = _points[i].X;
				}
			return bestIndex;
		}

		int? FindNextSlot( Space? space = null ) {

			// Prefer free slots
			for(int i = 0; i < _myTokens.Length; i++)
				if(_myTokens[i] is null)
					return i;

			if(space is not null)
				// Find old spot that is now available
				for(int i = 0; i < _myTokens.Length; i++) {
					IToken? token = _myTokens[i];
					if(token is not null && space[token] == 0) {
						_myTokens[i] = null;
						return i;
					}
				}

			return null;
		}

		void AssignToken( IToken token, int index ) {
			lock(_locker) {
				if(_myTokens[index] != null)
					throw new InvalidOperationException( $"Internal Token Slot {index} already assigned." );
				_myTokens[index] = token;
				_dict[token] = _points[index];
			}
		}

		readonly Lock _locker = new Lock();
		readonly IToken?[] _myTokens = new IToken[_points.Length];

		public XY[] Points => _points;
	}

	#region private token location fields

	public XY[] BorderPoints => _border.Points;
	public XY[] InternalPoints => _randomInternal.Points;

	readonly Dictionary<IToken, XY> _dict;
	readonly TokenPointArray _border;
	readonly TokenPointArray _randomInternal;

	#endregion

}

