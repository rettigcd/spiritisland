using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritIsland {

	public class SelfCtx {

		public Spirit Self { get; }
		public GameState GameState { get; }
		public Cause Cause { get; }
		public Spirit Originator { get; }
		#region constructor

		public SelfCtx(Spirit self,GameState gameState, Cause cause, Spirit originator=null) {
			Self = self;
			GameState = gameState;
			Cause = cause;
			Originator = originator ?? self;
		}

		protected SelfCtx(SelfCtx src) {
			Self = src.Self;
			GameState = src.GameState;
			Cause = src.Cause;
			Originator = src.Originator;
			_terrainMapper = src._terrainMapper;
		}

		#endregion constructor

		#region Presence

		public virtual BoundPresence Presence => _presence ??= new BoundPresence(this);
		BoundPresence _presence;

		#endregion

		#region convenience Read-Only methods

		public IEnumerable<Space> AllSpaces => GameState.Island.AllSpaces;

		/// <summary> Simple wrapper around GameState.Tokens.Move </summary>
		public Task Move(Token token, Space from, Space to )
			=> Target(from).Tokens.MoveTo( token, to );

		public Task<bool> YouHave( string elementString ) => Self.HasElements( ElementList.Parse(elementString) );

		#endregion

		public virtual void AddFear( int count ) { // overriden by TargetSpaceCtx to add the location
			GameState.Fear.AddDirect( new FearArgs { 
				count = count, 
				FromDestroyedInvaders = false,
				space = null 
			} );
		}

		public Task<T> Decision<T>( Select.TypedDecision<T> originalDecision ) where T : class, IOption => Self.Action.Decision( originalDecision );


		public TargetSpaceCtx Target( Space space ) => new TargetSpaceCtx( this, space );

		public TargetSpaceCtx TargetSpace( string spaceLabel ) => Target( GameState.Island.AllSpaces.First(s=>s.Label==spaceLabel) ); // !!! Testing extension - move to testing project

		// Visually, selects the [presence] icon
		public async Task<TargetSpaceCtx> TargetDeployedPresence( string prompt ) {
			var space = await Decision( Select.DeployedPresence.All( prompt, Self,Present.Always ) );
			return new TargetSpaceCtx( this, space );
		}

		// Visually, selects the [space] which has presence.
		public async Task<TargetSpaceCtx> TargetLandWithPresence( string prompt ) {
			var space = await Decision( new Select.Space(prompt,Self.Presence.Spaces, Present.Always ) );
			return new TargetSpaceCtx( this, space );
		}

		#region Draw Cards

		public Task<DrawCardResult> Draw() => Self.Draw( GameState );
		public Task<DrawCardResult> DrawMinor() => Self.DrawMinor( GameState );
		public Task<DrawCardResult> DrawMajor( bool forgetCard, int numberToDraw = 4, int numberToKeep = 1 ) => Self.DrawMajor( GameState, forgetCard, numberToDraw, numberToKeep );


		#endregion

		#region Generic Select space / option

		public async Task<TargetSpaceCtx> SelectSpace( string prompt, IEnumerable<Space> options ) {
			var space = await Decision( new Select.Space( prompt, options, Present.Always ) );
			return space != null
				? new TargetSpaceCtx( this, space )
				: null;
		}

		// overriden by Grinning Trickster's Lets See What Happens

		public Task SelectActionOption( params IExecuteOn<SelfCtx>[] options ) => SelectActionOption( "Select Power Option", options );
		public Task SelectActionOption( string prompt, params IExecuteOn<SelfCtx>[] options )=> SelectAction_Inner( prompt, options, Present.AutoSelectSingle, this );
		public Task SelectAction_Optional( string prompt, params IExecuteOn<SelfCtx>[] options )=> SelectAction_Inner( prompt, options, Present.Done, this );

		virtual protected async Task SelectAction_Inner<T>( string prompt, IExecuteOn<T>[] options, Present present, T ctx ) {
			IExecuteOn<T>[] applicable = options.Where( opt => opt != null && opt.IsApplicable(ctx) ).ToArray();
			string text = await Self.SelectText( prompt, applicable.Select( a => a.Description ).ToArray(), present );
			if(text != null && text != TextOption.Done.Text) {
				var selectedOption = applicable.Single( a => a.Description == text );
				await selectedOption.Execute( ctx );
			}
		}

		public virtual Task Execute( IExecuteOn<SelfCtx> actionOption ) => actionOption.Execute(this);

		#endregion

		// Defer initializing this because some tests don't initialize nor depend on the GameState
		protected TerrainMapper TerrainMapper => _terrainMapper ??= GameState.Island.TerrainMapFor(Cause);
		protected TerrainMapper _terrainMapper;


		#region High level fear-specific decisions

		public async Task RemoveHealthFromOne( int healthToRemove, IEnumerable<Space> options ) {
			var spaceCtx = await SelectSpace( $"remove {healthToRemove} invader health from", options );
			await spaceCtx.RemoveHealthWorthOfInvaders( healthToRemove );
		}

		public async Task<Space> RemoveTokenFromOneSpace( IEnumerable<Space> spaceOptions, int count, params TokenCategory[] removables ) {
			var spaceCtx = await SelectSpace( "Remove invader from", spaceOptions );
			if(spaceCtx != null)
				while(count-->0)
					spaceCtx.Invaders.Remove( removables );
			return spaceCtx?.Space;
		}

		public async Task GatherExplorerToOne( IEnumerable<Space> spaceOptions, int count, params TokenCategory[] typeToGather ) {
			var spaceCtx = await SelectSpace( "Gather Invader to", spaceOptions );
			if(spaceCtx != null)
				await spaceCtx.GatherUpTo(count,typeToGather);
		}


		#endregion

	}


}