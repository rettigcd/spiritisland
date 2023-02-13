using System;

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
		board.Spaces.Tokens().Where( s => s.Dahan.Any ).Last().Adjust(Presence.Token, 1);
		// and in land #4
		board[4].Tokens.Adjust(Presence.Token, 1);

	}

	// Cleanup Up Messes is such a drag
	public override async Task RemoveBlight( TargetSpaceCtx ctx, int count=1 ) {
		await CleaningUpMessesIsSuckADrag( ctx.Self, ctx.Tokens );
		await base.RemoveBlight( ctx,count );
	}

	static public async Task CleaningUpMessesIsSuckADrag( Spirit spirit, SpaceState tokens ) {
		if(tokens.Blight.Any)
			await spirit.PickPresenceToDestroy( $"{CleaningUpMessesIsADrag.Title} Destroy presence for blight cleanup" );
	}

	public override SelfCtx BindMyPowers( Spirit spirit ) {
		ActionScope.Current.Upgrader = (x) => new TrixterTokens( x );
		return new SelfCtx( spirit );
	}

	public override async Task AddStrife( SpaceState tokens, params HumanTokenClass[] groups ) {

		// ! Maybe this hould be in Trixter, and not in the Tokens...

		var st = await Gateway.Decision( Select.Invader.ForStrife( tokens, groups ) );
		if(st == null) return;
		var invader = (HumanToken)st.Token;
		await tokens.AddRemoveStrifeTo( invader );

		if(Energy == 0) return;

		var nearbyInvaders = PowerRangeCalc.GetTargetOptionsFromKnownSource( tokens.Adjacent, new TargetCriteria( 1 ) )
			.SelectMany( s => s.InvaderTokens().Select( t => new SpaceToken( s.Space, t ) ) )
			.ToArray();
		var invader2 = await Gateway.Decision( new Select.TokenFromManySpaces( "Add additional strife for 1 energy", nearbyInvaders, Present.Done ) );
		if(invader2 == null) return;
		--Energy;
		await invader2.Space.Tokens.AddRemoveStrifeTo( (HumanToken)invader2.Token );

	}

}