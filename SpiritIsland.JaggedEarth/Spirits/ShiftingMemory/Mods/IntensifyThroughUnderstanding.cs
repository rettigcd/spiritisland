namespace SpiritIsland.JaggedEarth;

class IntensifyThroughUnderstanding(ShiftingMemoryOfAges smoa)
	// Island Mods
	: BaseModEntity
	, IModifyRemovingToken						// Moon
	, IModifyAddingToken						// Sun (badland), Plant, Animal
	, IAdjustDamageToInvaders_FromSpiritPowers	// Fire
	, IHandleTokenAdded							// Sun (strife)
	, IHandleSpaceDefended						// Earth
	// Spirit actions
	, IModifyAvailableActions					// Air
	, IHandleActivatedActions					// Air
	// Action-Components
	, IMoverFactory								// Water
	// Init
	, IInitializeSpirit
	, IConfigureMyActions
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
		+" • Animal: Add +1 Disease or +1 Beasts";
	static public SpecialRule Rule => new SpecialRule(Name, Description);

	#endregion Rule

	static public void InitAspect(Spirit spirit) {
		spirit.Elements = new PreparedElementMgr(spirit);

		// You may spend Element Markers to modify your Actions (max. 1 of each Marker per Action)
		spirit.Mods.Add(new IntensifyThroughUnderstanding((ShiftingMemoryOfAges)spirit));
	}

	#region Water

	DestinationSelector IMoverFactory.PushDestinations => _default.PushDestinations;

	TokenMover IMoverFactory.Gather(Spirit self, Space space) {
		var mover = _default.Gather(self, space);
		mover.DoEndStuff += Mover_DoEndStuff;
		return mover;
	}

	TokenMover IMoverFactory.Pusher(Spirit self, SourceSelector sourceSelector, DestinationSelector? dest) {
		var mover = _default.Pusher(self, sourceSelector, dest);
		mover.DoEndStuff += Mover_DoEndStuff;
		return mover;
	}

	async Task Mover_DoEndStuff(bool ranOutOfOption, Move[] arg2, Spirit arg3) {
		if( !ranOutOfOption ) return;
		if( _spirit.PreparedElementMgr.PreparedElements[Element.Water] == 0 ) return;

		var options = arg2.Where(m => 0 < m.Source.Count).ToArray();
		if( options.Length == 0 ) return;

		var move = await _spirit.Select("Boost 1 more move with Water element?", options, Present.Done);
		if( move is null ) return;
		_spirit.PreparedElementMgr.PreparedElements[Element.Water]--;
		await move.Apply();
	}
	readonly DefaultMoverFactory _default = new();

	#endregion Water

	#region Air
	void IModifyAvailableActions.Modify(List<IActionFactory> orig, Phase phase) {
		bool canMakeSlowFast = phase == Phase.Fast && _usedCount < Math.Min(_spirit.PreparedElementMgr.PreparedElements[Element.Air], 1);
		if( !canMakeSlowFast ) return;

		_slowAsFast = _spirit.AllActions.Where(slowAction => slowAction.CouldActivateDuring(Phase.Slow, _spirit))
			.Except(orig) // in case another mod added slow-as-fast, don't re-add it.
			.ToArray();
		orig.AddRange(_slowAsFast);
	}

	void IHandleActivatedActions.ActionActivated(IActionFactory factory) {
		if( _slowAsFast.Contains(factory) ) {
			++_usedCount;
			--_spirit.PreparedElementMgr.PreparedElements[Element.Air];
		}
	}

	readonly protected ShiftingMemoryOfAges _spirit = smoa;


	IActionFactory[] _slowAsFast = [];

	int _usedCount {
		get => GameState.Current.RoundScope.TryGetValue(UsedKey, out object? val) ? (int)val! : 0;
		set => GameState.Current.RoundScope[UsedKey] = value;
	}
	string UsedKey => _usedKey ??= "SlowAsFast:" + Guid.NewGuid().ToString(); string? _usedKey;
	#endregion Air

	#region Other elements
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

	#endregion Other elements

	#region Initialize Spirit (after game exists)
	void IInitializeSpirit.Initialize() {
		GameState.Current.AddIslandMod(this); // everything but Air & Water
	}
	void IConfigureMyActions.Configure(Spirit spirit, ActionScope actionScope) {
		actionScope.MoverFactory = this; // water
	}

	#endregion

}
