namespace SpiritIsland.NatureIncarnate;

[InnatePower( KeepWatchForNewIncursions.Name ), Fast]
[FromSacredSite( Target.Dahan, 1 )]
[RepeatIf( "2 sun,3 air,4 animal" )]
public class KeepWatchForNewIncursions {

	public const string Name = "Keep Watch for New Incursions";

	[InnateTier( "1 animal", "Gather up to 2 Dahan, from your lands only.", 0 )]
	static public Task Option1Async( TargetSpaceCtx ctx ) {
		return ctx.Gatherer
			.AddGroup(2,Human.Dahan)
			.FilterSource(ctx.Self.Presence.IsOn)
			.DoUpToN();
	}

	[InnateTier( "1 sun,2 air,3 animal", "Once this turn after Invaders are added or moved into target land, 1 Damage per Dahan in target land, to those added/moved Invaders only.", 1 )]
	static public Task Option2Async( TargetSpaceCtx ctx ) {

		// Once this turn, after Invaders are added or moved into target land,
		// 1 Damage per Dahan in target land, to those added/moved Invaders only
		ctx.Tokens.Adjust( new DamageNewInvadersOnce( ctx.Self ), 1 );

		return Task.CompletedTask;
	}

	class DamageNewInvadersOnce : BaseModEntity, IHandleTokenAddedAsync, IEndWhenTimePasses {
		bool _used = false;
		readonly Spirit _spirit;
		public DamageNewInvadersOnce( Spirit spirit ) {
			_spirit = spirit;
		}
		public async Task HandleTokenAddedAsync( ITokenAddedArgs args ) {
			int damage = Math.Min(args.To.Dahan.CountAll, args.Added.AsHuman().RemainingHealth * args.Count);
			if(!_used
				&& 0<damage
				&& await _spirit.UserSelectsFirstText($"Apply {damage} damage to added invaders ({args.Count} {args.Added.Text})? (Keep Watch)", "Yes, Damage them!", "No, not quite yet" )
			) { 
				await args.To.Invaders.UserSelectedDamageAsync(_spirit,damage,args.Added.Class); // !! Not 100% correct.
				_used = true;
			}
		}
	}


}
