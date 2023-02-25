using System.Drawing;

namespace SpiritIsland;

// Tracks unique locations internal to a polygon
public class ManageInternalPoints {

	public PointF NameLocation { get; }

	#region constructor

	public ManageInternalPoints( SpaceState ss, SpaceLayout layout ) {
		const float stepSize = .06f; // .07
		const float minDistanceFromBoarder = .012f; //.015

		NameLocation = layout.GetInternalHexPoints( .02f )
			.Where( p => minDistanceFromBoarder < layout.DistanceFromBorder(p) )
			.OrderBy( p => { 
				var bounds = layout.Bounds;
				float dx = p.X-bounds.X;
				float dy = p.Y-bounds.Bottom;
				return dx*dx+dy*dy;
			} )
			.First();

		var points = layout
			.GetInternalHexPoints( stepSize )
			.ToArray();

		_dict = new Dictionary<IToken, PointF>();

		// internal - prefered
		var internalPoints = layout
			.GetInternalHexPoints( stepSize )
			.Where( p => stepSize*.6f < layout.DistanceFromBorder( p ) )
			.ToArray();
		new Random( ss.Space.Text.GetHashCode() ) // use the randomizer every time so pieces don't bounce around when we resize
			.Shuffle( internalPoints );

		// border - backup
		var borderPoints = points.Except( internalPoints )
			.OrderByDescending( layout.DistanceFromBorder )
			.ToArray();

		_randomInternal = new TokenPointArray( _dict, internalPoints );
		_border = new TokenPointArray( _dict, borderPoints );

	}

	#endregion

	public PointF GetPointFor( IToken token ) {

		if(_dict.ContainsKey(token)) return _dict[token];

		// This is called when we are requesting location for a token that is not currently on this space.

		// Find a Randome fresh / unused spot
		bool assigned = token.Class.Category switch {
			TokenCategory.Dahan or
			TokenCategory.Presence => _randomInternal.AssignRightSlot( token ),
			TokenCategory.Blight or
			TokenCategory.Invader => _randomInternal.AssignLeftSlot( token ),
			_ => _randomInternal.AssignNextSlot( token ),
		}
			|| _border.AssignNextSlot( token );

		return assigned ? _dict[token] : throw new Exception("Unable find open slot.");
	}

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
			.OfType<IToken>();
		foreach(IToken nonInvader in nonInvaders )
			AssignPointFor(nonInvader,allTokens);

		return this;
	}

	public IEnumerable<(IToken Key, PointF Value)> Assignments() 
		=> _dict.Select( p => (p.Key, p.Value) );

	#region private helper methods

	void AssignPointFor( IToken token, SpaceState spaceState ) {

		if(token.Class.Category == TokenCategory.Invader)
			throw new Exception( "invaders not handled here" );

		// Already assigned
		if(_dict.ContainsKey( token )) return;

		// -- Internal --
		bool assigned = token.Class.Category switch {
			TokenCategory.Dahan or
			TokenCategory.Presence => _randomInternal.AssignRightSlot(token),
			TokenCategory.Blight or
			TokenCategory.Invader => _randomInternal.AssignLeftSlot(token),
			_ => false, // fall through
		} || AssignOldSlot( token, spaceState );

		if(!assigned)
			throw new InvalidOperationException( $"Ran out of slots for tokens." );

	}

	bool AssignOldSlot( IToken token, SpaceState spaceState )
		=> _randomInternal.AssignNextSlot(token, spaceState)
		|| _border.AssignNextSlot(token, spaceState);

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

	class TokenPointArray {

		public TokenPointArray( Dictionary<IToken, PointF> dict, PointF[] points ) { 
			_dict = dict;
			_points = points;
			_tokens = new IToken[points.Length];
		}

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

		public bool AssignNextSlot( IToken visibleToken, SpaceState spaceState= null ) {
			int? index = FindNextSlot(spaceState);
			if(!index.HasValue) return false;
			AssignToken( visibleToken, index.Value );
			return true;
		}

		int? FindLeftSlot() {
			int? bestIndex = null;
			float mostLeft = float.MaxValue;
			for(int i = 0; i < _points.Length; i++)
				if(_tokens[i] is null
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
				if(_tokens[i] is null
					&& mostRight < _points[i].X
				) {
					bestIndex = i;
					mostRight = _points[i].X;
				}
			return bestIndex;
		}

		int? FindNextSlot( SpaceState spaceState = null ) {

			// Prefer free slots
			for(int i = 0; i < _tokens.Length; i++)
				if(_tokens[i] is null)
					return i;

			if(spaceState != null)
				// Find old spot that is now available
				for(int i = 0; i < _tokens.Length; i++) {
					IToken token = _tokens[i];
					if(spaceState[token] == 0) {
						_tokens[i] = null;
						return i;
					}
				}

			return null;
		}

		void AssignToken( IToken token, int index ) {
			lock(_locker) {
				if(_tokens[index] != null)
					throw new InvalidOperationException( $"Internal Token Slot {index} already assigned." );
				_tokens[index] = token;
				_dict[token] = _points[index];
			}
		}

		readonly object _locker = new object();
		readonly IToken[] _tokens;
		readonly PointF[] _points;
		readonly Dictionary<IToken, PointF> _dict;

	}

	#region private token location fields

	readonly Dictionary<IToken, PointF> _dict;
	readonly TokenPointArray _border;
	readonly TokenPointArray _randomInternal;

	#endregion

}

