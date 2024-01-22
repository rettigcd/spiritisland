namespace SpiritIsland.NatureIncarnate;

[InnatePower( KeepWatchForNewIncursions.Name ), Fast]
[FromSacredSite( Filter.Dahan, 1 )]
[RepeatIf( "2 sun,3 air,4 animal" )]
public class KeepWatchForNewIncursions {

	public const string Name = "Keep Watch for New Incursions";

	[InnateTier( "1 animal", "Gather up to 2 Dahan, from your lands only.", 0 )]
	static public Task Option1Async( TargetSpaceCtx ctx ) {
		return ctx.Gatherer
			.AddGroup(2,Human.Dahan)
			.ConfigSource(s=>s.FilterSource(ctx.Self.Presence.IsOn))
			.DoUpToN();
	}

	[InnateTier( "1 sun,2 air,3 animal", "Once this turn after Invaders are added or moved into target land, 1 Damage per Dahan in target land, to those added/moved Invaders only.", 1 )]
	static public Task Option2Async( TargetSpaceCtx ctx ) {

		// Once this turn, after Invaders are added or moved into target land,
		// 1 Damage per Dahan in target land, to those added/moved Invaders only
		ctx.Tokens.Adjust( new DamageNewInvadersOnce( ctx.Self ), 1 );

		return Task.CompletedTask;
	}

	/// <summary>
	/// Once this turn, after Invaders are added or moved into target land,
	/// 1 Damage per Dahan in target land, to those added/moved Invaders only
	/// </summary>
	class DamageNewInvadersOnce( Spirit spirit ) : BaseModEntity, IHandleTokenAddedAsync, IEndWhenTimePasses {
		bool _used = false;
		readonly Spirit _spirit = spirit;

		public async Task HandleTokenAddedAsync( SpaceState to, ITokenAddedArgs args ) {
			if(!_used
				|| !args.Added.HasTag( TokenCategory.Invader )
			) return;

			var invader = args.Added.AsHuman();
			int dahanCount = to.Dahan.CountAll;

			CombinedDamage combinedDamage = to.CombinedDamageFor_Invaders( dahanCount );
			int availableDamage = Math.Min( combinedDamage.Available, invader.RemainingHealth * args.Count );

			if(!await _spirit.UserSelectsFirstText( $"Keep Watch - Apply {availableDamage} damage to added invaders ({args.Count} {args.Added.Text})?", "Yes, Damage them!", "No, not quite yet" ))
				return;

			var (damageApplied,_) = await to.Invaders.ApplyDamageTo1( availableDamage, invader );
			combinedDamage.TrackDamageDone( damageApplied );

			_used = true;

		}
	}


}
