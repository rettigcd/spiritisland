namespace SpiritIsland.NatureIncarnate;

class RoilingWaters : IHealingCard {
	const string Name = "Roiling Waters";
	public string Text => Name;

	/// <summary> internal, not private - WoundedWatersBleeding.RestoreCustomStateFromJson re-adds this
	/// directly (without replaying Claim()'s whole effect, which would double-add the island Mod below)
	/// when restoring a spirit that already claimed this card. </summary>
	internal static readonly SpecialRule Rule = new SpecialRule(Name, "When your Powers add or move Beast into a land, you may do 1 Damage there per added or moved Beast. When your Powers add or move any number of Dahan into a land, you may do 1 Damage there (max once per Power)" );

	public bool MeetsRequirement( WoundedWatersBleeding spirit )
		=> 2 <= spirit.HealingMarkers[Element.Animal]
		&& 3 <= spirit.HealingMarkers.Total
		&& !spirit.HealingCardClaimed;

	public void Claim( WoundedWatersBleeding spirit ) {
		GameState.Current.AddIslandMod( new Mod(spirit) );
		spirit.AddSpecialRule( Rule );
	}

	public bool IsClaimed( WoundedWatersBleeding spirit ) => spirit.SpecialRules.Any(r=>r.Title==Name);

	public class Mod( Spirit spirit ) : BaseModEntity, IHandleTokenAdded, ISerializableSpaceEntity {
		readonly Spirit _spirit = spirit;

		async Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
			// When your powers...
			if(!_spirit.ActionIsMyPower) return;

			var scope = ActionScope.Current;

			// ...add or move Beast into a land
			const string beastDamageKey = Name+":BeastDamage";

			if(args.Added == Token.Beast && !scope.ContainsKey( beastDamageKey )) {
				// you may do 1 Damage there per added or moved Beast.
				await _spirit.Target(to).DamageInvaders( args.Count );
				scope[ beastDamageKey ] = true;
			}

			// add or move any number of Dahan into a land
			const string dahanDamageKey = Name + ":DahanDamage";
			if(args.Added.HasTag(TokenCategory.Dahan) && !scope.ContainsKey( dahanDamageKey )) {
				// you may do 1 Damage there (max once per Power)
				await _spirit.Target(to).DamageInvaders(1);
				scope[ dahanDamageKey ] = true;
			}
		}

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray( Tag, ctx.IndexOf( _spirit ) );

		const string Tag = "RoilingWatersMod";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => new Mod( ctx.SpiritAt( (int)json[1]! ) ) );

	}

}
