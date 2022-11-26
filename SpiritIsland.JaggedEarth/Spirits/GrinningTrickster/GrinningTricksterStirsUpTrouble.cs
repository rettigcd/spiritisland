namespace SpiritIsland.JaggedEarth;

public class GrinningTricksterStirsUpTrouble : Spirit {

	public const string Name = "Grinning Trickster Stirs Up Trouble";
	public override string Text => Name;

	public override SpecialRule[] SpecialRules => new SpecialRule[] {  ARealFlairForDiscord,  CleaningUpMessesIsADrag };

	static readonly SpecialRule ARealFlairForDiscord = new SpecialRule("A Real Flair for Discord", "After one of your Powers adds strife in a land, you may pay 1 Energy to add 1 strife within Range-1 of that land.");
	static readonly SpecialRule CleaningUpMessesIsADrag = new SpecialRule("Cleaning up Messes is a Drag", "After one of your Powers Removes blight, Destroy 1 of your presence.  Ignore this rule for Let's See What Happens.");

	public GrinningTricksterStirsUpTrouble()
		:base(
			new SpiritPresence(
				new PresenceTrack(Track.Energy1,Track.MoonEnergy,Track.Energy2,Track.AnyEnergy,Track.FireEnergy,Track.Energy3),
				new PresenceTrack(Track.Card2,Track.Push1Dahan,Track.Card3,Track.Card3,Track.Card4,Track.AirEnergy,Track.Card5)
			)
			,PowerCard.For<ImpersonateAuthority>()
			,PowerCard.For<InciteTheMob>()
			,PowerCard.For<OverenthusiasticArson>()
			,PowerCard.For<UnexpectedTigers>()
		)
	{
		// Growth
		this.GrowthTrack = new GrowthTrack( 2,
			new GrowthOption(new GainEnergy(-1),new ReclaimAll(), new MovePresence(1) ){ GainEnergy = -1 },
			new GrowthOption(new PlacePresence(2)),
			new GrowthOption(new DrawPowerCard()),
			new GrowthOption(new GainEnergyEqualToCardPlays() )
		);

		// Innates
		InnatePowers = new InnatePower[] {
			InnatePower.For<LetsSeeWhatHappens>(),
			InnatePower.For<WhyDontYouAndThemFight>()
		};
	}

	protected override void InitializeInternal( Board board, GameState gs ) {
		// Place presence on highest numbered land with dahan
		Presence.Adjust( gs.Tokens[board.Spaces.Where(s=>gs.Tokens[s].Dahan.Any).Last()], 1 );
		// and in land #4
		Presence.Adjust( gs.Tokens[board[4]], 1 );

	}

	// A Real Flair for Discord
	// After one of your Powers adds strife in a land, you may pay 1 Energy to add 1 strife within Range-1 of that land."
	//public override async Task AddStrife( TargetSpaceCtx ctx, Token invader ) {
	//	await base.AddStrife( ctx, invader );
	//	if(Energy==0) return;
	//	var nearbyInvaders = ctx.Space.Range(1)
	//		.SelectMany(s=>ctx.Target(s).Tokens.Invaders().Select(t=>new SpaceToken(s,t)))
	//		.ToArray();
	//	var invader2 = await Action.Decision(new Select.TokenFromManySpaces("Add additional strife for 1 energy",nearbyInvaders,Present.Done));
	//	if(invader2 == null) return;
	//	Energy--;
	//	await base.AddStrife( ctx.Target(invader2.Space), invader2.Token);
	//}
	// !!!!! was this AddStrife.. thing added to derived context ????


	// Cleanup Up Messes is such a drag
	public override async Task RemoveBlight( TargetSpaceCtx ctx, int count=1 ) {
		await CleaningUpMessesIsSuckADrag( ctx );
		await base.RemoveBlight( ctx,count );
	}

	public async Task CleaningUpMessesIsSuckADrag( TargetSpaceCtx ctx ) {
		if(ctx.Blight.Any)
			await PickPresenceToDestroy( ctx );
	}

	async Task PickPresenceToDestroy( TargetSpaceCtx ctx ) {
		var space = await this.Gateway.Decision( Select.DeployedPresence.ToDestroy( $"{CleaningUpMessesIsADrag.Title} Destroy presence for blight cleanup", ctx.Presence ) );
		await ctx.Presence.Destroy( space, DestoryPresenceCause.SpiritPower );
	}

	public override SelfCtx BindMyPower( GameState gameState, Guid actionId=default ) 
		=> new TricksterCtx(this,gameState,actionId!=default?actionId:Guid.NewGuid());

}

// Only use this when Trickster is using their own Powers
class TricksterCtx : SelfCtx {
	public TricksterCtx(Spirit spirit, GameState gs, Guid actionId) : base( spirit, gs, Cause.MyPowers, actionId ) { }
	public override TargetSpaceCtx Target( Space space ) => new TricksterSpaceCtx( this, space );
}

public class TricksterSpaceCtx : TargetSpaceCtx {

	public TricksterSpaceCtx(SelfCtx ctx, Space space):base( ctx, space ) {}

	public override BlightTokenBinding Blight => new TricksterBlight( this, CurrentActionId );

	public override async Task AddStrife( params TokenClass[] groups ) {
		await base.AddStrife( groups );

		if( Self.Energy == 0 ) return;

		var nearbyInvaders = Tokens.Range( 1 )
			.SelectMany( s => s.InvaderTokens().Select( t => new SpaceToken( s.Space, t ) ) )
			.ToArray();
		var invader2 = await Self.Gateway.Decision( new Select.TokenFromManySpaces( "Add additional strife for 1 energy", nearbyInvaders, Present.Done ) );
		if(invader2 == null) return;
		--Self.Energy;
		await Target( invader2.Space ).AddStrifeTo( (HealthToken)invader2.Token );

	}

}

// !!! merge this into TricksterSpaceCtx
public class TricksterBlight : BlightTokenBinding {

	readonly TricksterSpaceCtx ctx;
	readonly GrinningTricksterStirsUpTrouble trickster;

	public TricksterBlight( TricksterSpaceCtx ctx, Guid actionId ) :base( ctx.Tokens, actionId ) {
		this.ctx = ctx;
		trickster = (GrinningTricksterStirsUpTrouble)ctx.Self;
	}

	public override async Task Remove( int count, RemoveReason reason = RemoveReason.Removed ) {
		await trickster.CleaningUpMessesIsSuckADrag( ctx ); // feature envy?
		await base.Remove( count, reason );
	}
}