using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland;

public class InvaderCard : IOption {

	public static readonly InvaderCard Costal = new InvaderCard( s => s.IsCoastal, "Costal" );
		
	public int InvaderStage { get; }
	public string Text { get; }
	public bool Escalation { get; }
	public Func<Space,bool> Matches { get; }

	#region Constructors

	/// <summary>
	/// Stage 1 or 2 constructor
	/// </summary>
	public InvaderCard(Terrain terrain, bool escalation=false){
		if(terrain==Terrain.Ocean) throw new ArgumentException("Can't invade oceans");
		InvaderStage = escalation ? 2 : 1;
		Matches = (s) => s.Is( terrain );
		Text = escalation
			? "2" +terrain.ToString()[..1].ToLower() 
			: terrain.ToString()[..1];
		Escalation = escalation;
	}

	public InvaderCard(Space space){
		var terrain = new[] { Terrain.Wetland, Terrain.Sand, Terrain.Jungle, Terrain.Mountain }.First( space.Is );
		if(terrain==Terrain.Ocean) throw new ArgumentException("Can't invade oceans");
		InvaderStage = 1;
		Matches = (s) => s.Is( terrain );
		Text = terrain.ToString()[..1];
		Escalation = false;
	}

	InvaderCard( Func<Space, bool> matches, string text ) { // Costal
		InvaderStage = 2;
		Matches = matches;
		Text = text;
		Escalation = false;
	}


	public InvaderCard(Terrain t1, Terrain t2){
		Matches = (s) => s.IsOneOf( t1, t2 );
		Text = t1.ToString()[..1] + "+" + t2.ToString()[..1];
		InvaderStage = 3;
	}

	#endregion

	#region Ravage

	public async Task Ravage( GameState gs ) {
		gs.Log( new InvaderActionEntry( "Ravaging:" + Text ) );
		var ravageSpaces = gs.Island.AllSpaces.Where( Matches ).ToList();

		// Modify / Adjust
		await gs.PreRavaging?.InvokeAsync( new RavagingEventArgs(gs){ Spaces = ravageSpaces } );

		// find ravage spaces that have invaders
		InvaderBinding[] ravageGroups = ravageSpaces
			.Select( gs.Invaders.On )
			.Where( group => group.Tokens.HasInvaders() )
			.Cast<InvaderBinding>()
			.ToArray();

		foreach(InvaderBinding invaderBinding in ravageGroups) {
			var @event = await new RavageAction( gs, invaderBinding ).Exec();
			gs.Log( new InvaderActionEntry( @event.ToString() ) );
		}

	}

	#endregion

	#region Build

	public async Task Build( GameState gameState ) {
		gameState.Log( new InvaderActionEntry( "Building:" + Text ) );

		var buildSpaces = gameState.Island.AllSpaces.Where( Matches )
			.ToDictionary( s => s, grp => 1 )
			.ToCountDict();

		BuildEngine buildEngine = gameState.GetBuildEngine();

		// Modify
		var args = new BuildingEventArgs(gameState, new Dictionary<Space, BuildingEventArgs.BuildType>() ) {
			SpaceCounts = buildSpaces,
		};
		await gameState.PreBuilding.InvokeAsync( args );

		// Do build in each space
		var buildGroups = args.SpaceCounts.Keys
			.OrderBy( x => x.Label )
			.Select( x => gameState.Tokens[x] )
//			.Where( x => x.HasInvaders() ) // We want to log these too
			.ToArray();

		foreach(TokenCountDictionary tokens in buildGroups) {
			int count = args.SpaceCounts[tokens.Space];
			while(count -- > 0) {
				string buildResult = await buildEngine.Exec( args, tokens, gameState );
				gameState.Log( new InvaderActionEntry( tokens.Space.Label + ": gets " + buildResult ) );
			}
		}

	}

	#endregion

	#region Explore

	public async Task Explore( GameState gs ) {
		InvaderCard card = this;

		gs.Log( new InvaderActionEntry( "Exploring:" + card.Text ) );

		// Modify
		bool IsExplorerSource( Space space ) { return space.IsOcean || gs.Tokens[space].HasAny( Invader.Town, Invader.City ); }
		var args = new ExploreEventArgs( gs,
			gs.Island.AllSpaces.Where( IsExplorerSource ),
			gs.Island.AllSpaces.Where( card.Matches )
		);
		await gs.PreExplore.InvokeAsync( args );

		var tokenSpacesToExplore = args.WillExplore( gs )
			.Select( x => gs.Tokens[x] )
			.ToArray();

		// Explore
		foreach(var exploreTokens in tokenSpacesToExplore)
			await ExploreSingleSpace( exploreTokens, gs );
	}

	static async Task ExploreSingleSpace( TokenCountDictionary tokens, GameState gs ) {
		// only gets called when explorer is actually going to explore
		var wilds = tokens.Wilds;
		if(wilds == 0) { 
			gs.Log( new InvaderActionEntry(tokens.Space+":gains explorer") );
			await tokens.Add( Invader.Explorer.Default, 1, AddReason.Explore );
		}
		else
			await wilds.Remove( 1, RemoveReason.UsedUp );
	}

	#endregion

}
