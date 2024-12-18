namespace SpiritIsland.NatureIncarnate;

[InnatePower(Name)]
[Fast,Yourself]
public class ConsiderAHarmoniousNature {

	public const string Name = "Consider a Harmonious Nature";

	[InnateTier("3 sun,1 moon","When your Powers would add Blight, you may Destory 1 Presence instead.",0)]
	static public Task Option1( Spirit self ) {
		// When your Powers would add Blight, you may Destory 1 Presence instead.
		GameState.Current.Tokens.AddIslandMod(new DestroyPresenceInsteadOfAddingBlight(self,Name));
		return Task.CompletedTask;
	}

	[InnateTier("3 sun,1 water","Your Powers don't damage or destroy Dahan.",1)]
	static public Task Option2( Spirit self ) {
		// Your Powers don't damage or destroy Dahan.
		GameState.Current.Tokens.AddIslandMod(new MyPowersDontDamageDahanThisRound(self,Name));
		return Task.CompletedTask;
	}

	[InnateTier("3 sun,1 plant","Choose another Spirit. They Add 1 DestroyedPresence to one of your lands.",2)]
	static public async Task Option3( Spirit self ) {
		// Choose another Spirit.
		// They Add 1 DestroyedPresence to one of your lands.
		await AddAnotherSpiritsDestroyedPresenceToYourLand( self );
	}

	[InnateTier( "3 sun,1 water,1 plant", "Give up to 3 of your Energy to the chosen Spirit.", 2 )]
	static public async Task Option4( Spirit self ) {
		Spirit? otherSpirit = await AddAnotherSpiritsDestroyedPresenceToYourLand( self ); // Override above tier and do here
		// Give up to 3 of your Energy to the chosen Spirit.
		if(otherSpirit is not null)
			await GiveUpToNEnergyToSpirit( self, otherSpirit, 3);

	}

	/// <returns>Selected Spirit</returns>
	static async Task<Spirit?> AddAnotherSpiritsDestroyedPresenceToYourLand( Spirit self ) {
		Spirit? other = await self.SelectAsync(new A.TypedDecision<Spirit>(
			"Choose spirit to Add Destroyed Presence to one of your lands.", 
			GameState.Current.Spirits.Where(s=>s!=self),
			Present.Done
		));
		if(other is not null)
			await new AddDestroyedPresence().RelativeTo( self ).ActAsync( other );
		return other;
	}

	static async Task GiveUpToNEnergyToSpirit( Spirit from, Spirit to, int max ) {
		int delta = await from.SelectNumber($"Give {to.SpiritName} Energy:", Math.Min(max,from.Energy) );
		from.Energy -= delta;
		to.Energy += delta;
	}

}

class MyPowersDontDamageDahanThisRound( Spirit spirit, string source )
	: BaseModEntity
	, IModifyRemovingToken
	, IAdjustDamageToDahan
	, IEndWhenTimePasses
{
	readonly Spirit _spirit = spirit;
	readonly string _source = source;

	void IAdjustDamageToDahan.Modify( DamagingTokens args ) {
		if(_spirit.ActionIsMyPower) {
			ActionScope.Current.Log( new Log.Debug( $"{_source} prevented {args.TokenCountToReceiveDamage} Dahan from receiving {args.DamagePerToken} on {args.On.Label}." ) );
			args.TokenCountToReceiveDamage = 0;
		}
	}

	Task IModifyRemovingToken.ModifyRemovingAsync( RemovingTokenArgs args ) {
		if(args.Token == Human.Dahan 
			&& args.Reason == RemoveReason.Destroyed 
			&& _spirit.ActionIsMyPower
		) {
			ActionScope.Current.Log( new Log.Debug( $"{_source} prevented {args.Count} Dahan Destruction on {args.From.Label}." ) );
			args.Count = 0;
		}
		return Task.CompletedTask;
	}
}

class DestroyPresenceInsteadOfAddingBlight( Spirit spirit, string source ) 
	: BaseModEntity, IModifyAddingToken, IEndWhenTimePasses
{
	readonly Spirit _spirit = spirit;
	readonly string _source = source;

	public async Task ModifyAddingAsync( AddingTokenArgs args ) {
		if(args.Token == Token.Blight 
			&& await _spirit.UserSelectsFirstText($"Destroy 1 presence instead of adding {args.Count} of blight to {args.To.Label}?", "Yes, destroy my presence instead", "No, bring on the blight!")
		) {
			ActionScope.Current.Log(new Log.Debug($"{_source} stopped blight on {args.To.Label} by destroying presence."));
			await Cmd.DestroyPresence().ActAsync(_spirit);
			args.Count = 0;
		}
	}
}