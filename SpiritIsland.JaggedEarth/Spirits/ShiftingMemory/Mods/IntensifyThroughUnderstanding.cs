namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Intensify Through Understanding - Shifting Memory of Ages' special rule, Island Mod half
/// (Moon/Sun/Fire/Plant/Animal/Earth). Split from the Air/Water half (see IntensifyAirWater)
/// because they live in different registries: this one is an Island Mod, dispatched via the
/// Island Mods token dictionary, so it's a plain Spirit reference and can be serialized like
/// any other single-Spirit ISpaceEntity. Air/Water modify action availability/movement, which
/// are only dispatched via Spirit.Mods.OfType&lt;T&gt;() - there's no restoration mechanism for
/// Spirit.Mods today (same gap as MarkedBeastMover), so that half stays out of this catalog.
/// </summary>
public class IntensifyThroughUnderstanding(ShiftingMemoryOfAges smoa)
	: BaseModEntity
	, IModifyRemovingToken						// Moon
	, IModifyAddingToken						// Sun (badland), Plant, Animal
	, IAdjustDamageToInvaders_FromSpiritPowers	// Fire
	, IHandleTokenAdded							// Sun (strife)
	, IHandleSpaceDefended						// Earth
{

	// https://spiritislandwiki.com/index.php?title=Intensify

	#region Rule

	public const string Name = "Intensify Through Understanding";
	const string Description = "You may spend Element Markers to modify your Actions."
		+" • Sun: Add +1 Strife or +1 Badlands"
		+" • Moon: Remove/Replace +1 piece"
		+" • Fire: +1 Damage"
		+" • Air: Use Minor Power Fast"
		+" • Water: Move +1 piece"
		+" • Earth: Defend +2"
		+" • Plant: Add +2 Wilds or +2 Destroyed Presence"
		+" • Animal: Add +1 Disease or +1 Beasts"
		+"Bonus: 1 Moon, 1 Any";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	#endregion Rule

	static public void InitAspect(Spirit spirit) {
		spirit.Elements = new PreparedElementMgr(spirit);

		// You may spend Element Markers to modify your Actions (max. 1 of each Marker per Action)
		spirit.Mods.Add(new IntensifyAirWater((ShiftingMemoryOfAges)spirit));
	}

	readonly protected ShiftingMemoryOfAges _spirit = smoa;

	async Task IModifyAddingToken.ModifyAddingAsync(AddingTokenArgs args) {
		if( (args.Reason == AddReason.AddedToCard || args.Reason == AddReason.AsReplacement)
			&& _spirit.ActionIsMyPower
		) {
			//Animal: Add +1 Disease or +1 Beasts
			if( args.Token.HasAny(Token.Disease, Token.Beast) )
				args.Count += await DoBoost(Element.Animal, $"{args.Reason} {args.Token.Text}", args.Count);

			//Sun: +1 Badlands    (1 of 2)
			if( args.Token.HasTag(Token.Badlands) )
				args.Count += await DoBoost(Element.Sun, $"{args.Reason} {args.Token.Text}", args.Count);

			//Plant: Add +2 Wilds or +2 Destroyed Presence
			else if( args.Token.HasTag(Token.Wilds) )
				args.Count += await DoBoost(Element.Plant, $"{args.Reason} {args.Token.Text}", args.Count, 2);
			else if( args.Token is SpiritPresenceToken spt && args.Reason == AddReason.MovedTo ) {
				var destroyed = spt.Self.Presence.Destroyed;
				if( 0 < destroyed.Count ) {
					int boost = await DoBoost(Element.Plant, "Restored Presence", args.Count);
					args.Count += boost;
					spt.Self.Presence.Destroyed.Count -= boost;
				}
			}

		}
	}

	async Task IAdjustDamageToInvaders_FromSpiritPowers.ModifyDamage(DamageFromSpiritPowers args) {
		//Fire: +1 Damage
		args.Damage += await DoBoost(Element.Fire, "Damage", args.Damage);
	}

	async Task IModifyRemovingToken.ModifyRemovingAsync(RemovingTokenArgs args) {
		//Moon: Remove/Replace +1 piece
		if( args.Reason == RemoveReason.Removed || args.Reason == RemoveReason.Replaced )
			args.Count += await DoBoost(Element.Moon, $"{args.Reason} {args.Token.Text}", args.Count);
	}

	//Sun: +1 Strife    (2 of 2)
	async Task IHandleTokenAdded.HandleTokenAddedAsync(Space to, ITokenAddedArgs args) {
		if( args.IsStrifeAdded() && _spirit.ActionIsMyPower ) {
			int boost = await DoBoost(Element.Sun, "Strife", 1);
			if( boost != 0 )
				await to.SourceSelector.UseQuota(new Quota().AddGroup(1, Human.Invader)).StrifeAll(_spirit); // Marking Sun Element as used will prevent looping.
		}
	}

	// Max 1 / Action
	async Task<int> DoBoost(Element el, string itemToBoost, int start, int delta = 1) {
		if( _spirit.ActionIsMyPower
			&& HasPreparedElement(el)
			&& !IsUsed(el)
			&& await _spirit.UserSelectsFirstText($"Boost {itemToBoost} from {start} to {start + delta}? (costs 1 {el})", "Yes, boost it!", "No thanks")
		) {
			--_spirit.PreparedElementMgr.PreparedElements[el];
			MarkUsed(el);
			return delta;
		}
		return 0;
	}


	static bool IsUsed(Element el) {
		var scope = ActionScope.Current;
		return scope.ContainsKey(Key) && ((HashSet<Element>)scope[Key]).Contains(el);
	}

	static void MarkUsed(Element el) {
		ActionScope.Current.SafeGet<HashSet<Element>>(Key, () => []).Add(el);
	}

	const string Key = "Used-Elements-Intensify";

	bool HasPreparedElement(Element el) => 0 < _spirit.PreparedElementMgr.PreparedElements[el];

	void IHandleSpaceDefended.OnDefend(Space space, int defendCount) {
		//Earth: Defend +2								MOD - with Defend help
		if( _spirit.ActionIsMyPower && 0 < _spirit.PreparedElementMgr.PreparedElements[Element.Earth] ) {
			ActionScope.Current.AtEndOfThisAction(async scope => {
				if( HasPreparedElement(Element.Earth) ) {
					int boost = await DoBoost(Element.Earth, "Defend", space.Defend.Count, 2);
					space.Adjust(Token.Defend, boost);
				}
			});
		}
	}

}
