namespace SpiritIsland;

static public class Push_Extensions {

	#region Push Factory stuff

	/// <summary>
	/// Groups SourceSelector, DestinationConfig, and Bring Hook - allows to bind late
	/// </summary>
	public class MoverFactory {
		public SourceSelector Selector;
		public Action<DestinationSelector> DestinationConfigurer;
		public Func<TokenMovedArgs,Task> OnMoved;
		public MoverFactory Bring(Func<TokenMovedArgs,Task> onMoved ) {	OnMoved = onMoved; return this; }

		public TokenMover Push(Spirit self) {
			var pusher = Selector.ToPusher(self);
			if(DestinationConfigurer!=null) pusher.ConfigDestination(DestinationConfigurer);
			if(OnMoved != null) pusher.Bring(OnMoved);
			return pusher;
		}
	}

	static public MoverFactory ConfigDestination(this SourceSelector selector, Action<DestinationSelector> destinationConfigurer)
		=> new MoverFactory{Selector = selector, DestinationConfigurer = destinationConfigurer};

	static public MoverFactory Bring(this SourceSelector selector, Func<TokenMovedArgs,Task> onMoved)
		=> new MoverFactory{Selector = selector, OnMoved = onMoved};

	// !! These 2 methods could be put on MoverFactory - should they be?
	static public Task PushUpToN( this MoverFactory factory, Spirit self ) => factory.Push(self).DoUpToN();

	static public Task PushN( this MoverFactory factory, Spirit self ) => factory.Push(self).DoN();

	#endregion

	#region Simple Push - no Config / no Bring

	static public Task PushUpToN( this SourceSelector ss, Spirit self ) => ss.ToPusher(self).DoUpToN();

	static public Task PushN( this SourceSelector ss, Spirit self ) => ss.ToPusher(self).DoN();

	static public TokenMover ToPusher( this SourceSelector ss, Spirit spirit ) {
		return GameState.Current.Island.Boards[0][0].Tokens	// this part is a HACK
			.Pusher(spirit,ss);
	}

	#endregion Simple Push - no Config / no Bring

	#region Pushing 1 Token
	// Pushing 1 Token
	static public Task<TokenMovedArgs> PushAsync( this SpaceToken spaceToken, Spirit self, Action<DestinationSelector> configDestination=null ) {
		var destinations = spaceToken.Space.Tokens.PushDestinations;
		configDestination?.Invoke(destinations);
		return spaceToken.MoveToAsync("Push",destinations,self);
	}

	static public async Task<TokenMovedArgs> MoveToAsync( 
		this SpaceToken spaceToken,
		string actionWord,
		DestinationSelector destinationSelector,
		Spirit self
	) {
		Space destination = await destinationSelector.SelectDestination( self, actionWord, spaceToken );
		return destination == null ? null
			: await spaceToken.Token.Move( spaceToken.Space.Tokens, destination );
	}

	#endregion Pushing 1 Token

}