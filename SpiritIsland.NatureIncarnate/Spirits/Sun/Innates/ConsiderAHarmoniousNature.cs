namespace SpiritIsland.NatureIncarnate;

[InnatePower(Name)]
[Fast,Yourself]
public class ConsiderAHarmoniousNature {

	public const string Name = "Consider a Harmonious Nature";

	[InnateOption("3 sun,1 moon","When your Powers would add Blight, you may Destory 1 Presence instead.",0)]
	static public Task Option1(SelfCtx ctx ) {
		// When your Powers would add Blight, you may Destory 1 Presence instead.
		GameState.Current.Tokens.AddIslandMod(new DestroyPresenceInsteadOfAddingBlight(ctx.Self,Name));
		return Task.CompletedTask;
	}

	[InnateOption("3 sun,1 water","Your Powers don't damage or destroy Dahan.",1)]
	static public Task Option2( SelfCtx ctx ) {
		// Your Powers don't damage or destroy Dahan.
		GameState.Current.Tokens.AddIslandMod(new MyPowersDontDamageDahanThisRound(ctx.Self,Name));
		return Task.CompletedTask;
	}

	[InnateOption("3 sun,1 plant","Choose another Spirit. They Add 1 DestroyedPresence to one of your lands.",2)]
	static public async Task Option3( SelfCtx ctx ) {
		// Choose another Spirit.
		// They Add 1 DestroyedPresence to one of your lands.
		await AddAnotherSpiritsDestroyedPresenceToYourLand( ctx );
	}

	[InnateOption( "3 sun,1 water,1 plant", "Give up to 3 of your Energy to the chosen Spirit.", 2 )]
	static public async Task Option4( SelfCtx ctx ) {
		Spirit? otherSpirit = await AddAnotherSpiritsDestroyedPresenceToYourLand( ctx ); // Override above tier and do here
		// Give up to 3 of your Energy to the chosen Spirit.
		if(otherSpirit is not null)
			await GiveUpToNEnergyToSpirit( ctx.Self, otherSpirit, 3);

	}

	/// <returns>Selected Spirit</returns>
	static async Task<Spirit?> AddAnotherSpiritsDestroyedPresenceToYourLand( SelfCtx ctx ) {
		Spirit other = await ctx.Self.Select(new A.TypedDecision<Spirit>(
			"Choose spirit to Add Destroyed Presence to one of your lands.", 
			GameState.Current.Spirits.Where(s=>s!=ctx.Self),
			Present.Done
		));
		if(other != null)
			await new AddDestroyedPresence( 0 ).RelativeTo( ctx.Self ).ActAsync( other.BindSelf() );
		return other;
	}

	static async Task GiveUpToNEnergyToSpirit( Spirit from, Spirit to, int max ) {
		int delta = await from.SelectNumber($"Give {to.Text} Energy:", Math.Min(3,from.Energy) );
		from.Energy -= delta;
		to.Energy += delta;
	}

}

class MyPowersDontDamageDahanThisRound 
	: BaseModEntity
	, IModifyRemovingToken
	, IModifyDahanDamage
	, IEndWhenTimePasses
{
	readonly Spirit _spirit;
	readonly string _source;
	public MyPowersDontDamageDahanThisRound( Spirit spirit, string source ){
		_spirit = spirit;
		_source = source;
	}

	void IModifyDahanDamage.Modify( DamagingTokens args ) {
		if(_spirit.ActionIsMyPower) {
			GameState.Current.Log( new Log.Debug( $"{_source} prevented {args.TokenCountToReceiveDamage} Dahan from receiving {args.DamagePerToken} on {args.On.Space.Text}." ) );
			args.TokenCountToReceiveDamage = 0;
		}
	}

	void IModifyRemovingToken.ModifyRemoving( RemovingTokenArgs args ) {
		if(args.Token == Human.Dahan 
			&& args.Reason == RemoveReason.Destroyed 
			&& _spirit.ActionIsMyPower
		) {
			GameState.Current.Log( new Log.Debug( $"{_source} prevented {args.Count} Dahan Destruction on {args.From.Space.Text}." ) );
			args.Count = 0;
		}
	}
}

class DestroyPresenceInsteadOfAddingBlight : BaseModEntity, IModifyAddingTokenAsync, IEndWhenTimePasses {
	readonly Spirit _spirit;
	readonly string _source;
	public DestroyPresenceInsteadOfAddingBlight( Spirit spirit, string source ) {
		_spirit = spirit;
		_source = source;
	}
	public async Task ModifyAddingAsync( AddingTokenArgs args ) {
		if(args.Token == Token.Blight 
			&& await _spirit.UserSelectsFirstText($"Destroy 1 presence instead of adding {args.Count} of blight to {args.To.Space.Text}?", "Yes, destroy my presence instead", "No, bring on the blight!")
		) {
			GameState.Current.Log(new Log.Debug($"{_source} stopped blight on {args.To.Space.Text} by destroying presence."));
			await Cmd.DestroyPresence().ActAsync(_spirit.BindSelf());
			args.Count = 0;
		}
	}
}