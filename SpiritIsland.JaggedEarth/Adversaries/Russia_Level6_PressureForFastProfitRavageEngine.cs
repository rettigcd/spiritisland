namespace SpiritIsland.JaggedEarth;

/// <summary>
/// Adds ravage spaces where 3 or more explorers
/// Level-6 => CheckForPressureForFastProfit - Adds Explorer/Town to each board where no blight was added
/// </summary>
class Russia_Level6_PressureForFastProfitRavageEngine : Russia_Level3_CompetitionAmongHuntersRavageEngine {

	public Russia_Level6_PressureForFastProfitRavageEngine( GameState gameState ) {
		_token = new RecordBlightAdded();
		gameState.AddIslandMod( _token );
	}

	// After Ravage, on each board where it added no Blight: In the land with the most Explorer( min. 1), add 1 Explorer and 1 Town.
	public override async Task ActivateCard( InvaderCard card ) {
		_token.PreRavage();
		await base.ActivateCard( card );
		await _token.PressureForFastProfit();
	}

	/// <summary>
	/// Fixes the reference-identity bug described in docs/GameSerialization-Roadmap.md's Adversary
	/// section: replaying Init on a restored GameState reconstructs this engine and a fresh
	/// RecordBlightAdded together (see the constructor), but Tokens_ForIsland.FromJson then
	/// wipes/replaces island mods with whatever the JSON snapshot says - discarding that fresh instance
	/// as an island mod without updating _token, which would otherwise keep pointing at an instance no
	/// longer registered as a live mod (so it stops receiving HandleTokenAddedAsync callbacks for any
	/// board, making PressureForFastProfit wrongly treat every board as having received no Ravage
	/// Blight from then on). Re-points _token at whichever instance actually ended up live.
	/// </summary>
	public override void ResolveAfterTokensRestored( ISerializationContext ctx )
		=> _token = ctx.ExistingSpaceEntity<RecordBlightAdded>( ctx.SpaceSpecOrFakeByLabel( "Island-Mods" ) );

	RecordBlightAdded _token;

	public class RecordBlightAdded : BaseModEntity, IHandleTokenAdded, ISerializableSpaceEntity {

		public void PreRavage() {
			_receivedRavageBlight.Clear();
		}

		/// <summary>
		/// Tracks which boards received blight during Ravage - this turn only
		/// </summary>
		readonly HashSet<Board> _receivedRavageBlight = [];

		Task IHandleTokenAdded.HandleTokenAddedAsync( Space to, ITokenAddedArgs args ) {
			if(args.Added == Token.Blight
				&& args.Reason == AddReason.Ravage
			) {
				_receivedRavageBlight.UnionWith( to.SpaceSpec.Boards ); // log
			}
			return Task.CompletedTask;
		}

		public async Task PressureForFastProfit() {
			// Level 6
			// After the Ravage Step of turn 2+,
			// on each board where it added no Blight:
			var boardsWithNoNewBlight = GameState.Current.Island.Boards.Except( _receivedRavageBlight );

			// In the land with the most Explorer
			static Space? PickSpaceWithMostExplorers( Board board ) => board.Spaces.ScopeTokens()
				.Where( ss => 0 < ss.Sum( Human.Explorer ) ) //  (min. of 1)
				.OrderByDescending( ss => ss.Sum( Human.Explorer ) )
				.FirstOrDefault();
			Space[] landsWithMostExplorers = boardsWithNoNewBlight
				.Select( PickSpaceWithMostExplorers )
				.OfType<Space>() // filters nulls
				.ToArray();

			foreach(var land in landsWithMostExplorers) {
				// add 1 Explorer and 1 Town.
				await land.AddDefaultAsync( Human.Explorer, 1 );
				await land.AddDefaultAsync( Human.Town, 1 );
			}
			if(landsWithMostExplorers.Length != 0)
				ActionScope.Current.LogDebug( "Pressure for Fast Profit: Added 1T+1E to "
					+ landsWithMostExplorers.SelectLabels().OrderBy(x=>x).Join( "," )
				);
		}

		JsonArray ISerializableSpaceEntity.ToJson( ISerializationContext ctx ) => new JsonArray(
			Tag, new JsonArray( _receivedRavageBlight.Select( b => (JsonNode)b.Name ).ToArray() )
		);

		const string Tag = "RecordBlightAdded";

		[ModuleInitializer]
		internal static void RegisterSerialization()
			=> SpaceEntitySerialization.Register( Tag, ( json, ctx ) => {
				var result = new RecordBlightAdded();
				foreach( JsonNode? name in json[1]!.AsArray() )
					result._receivedRavageBlight.Add( ctx.BoardByName( name!.GetValue<string>() ) );
				return result;
			} );

	}

}
