namespace SpiritIsland.FeatherAndFlame;

public class CommunitiesInDisarray : FearCardBase, IFearCard {

	public const string Name = "Communities in Disarray";
	public string Text => Name;

	[FearLevel( 1, "City each deal -1 Damage during Ravage. Invaders do not heal Damage at the end of this turn." )]
	public Task Level1( GameCtx ctx ) 
		=> ReduceDamageAndDontHeal( 1, Human.City )
		.Execute(ctx);

	[FearLevel( 2, "Town/City each deal -1 Damage during Ravage. Invaders do not heal Damage at the end of this turn." )]
	public Task Level2( GameCtx ctx )
		=> ReduceDamageAndDontHeal( 1, Human.Town_City )
		.Execute( ctx );

	[FearLevel( 3, "Town/City each deal -2 Damage during Ravage. Invaders do not heal Damage at the end of this turn." )]
	public Task Level3( GameCtx ctx )
		=> ReduceDamageAndDontHeal( 2, Human.Town_City )
		.Execute( ctx );

	static DecisionOption<GameCtx> ReduceDamageAndDontHeal( int damagePenalty, params HumanTokenClass[] tokenClasses )	=> new DecisionOption<GameCtx>( 
		string.Join(" / ", tokenClasses.Select(x=>x.Label)) + $" each deal -{damagePenalty} Damage during Ravage. Invaders do not heal Damage at the end of this turn.", 
		ctx=> {
			void ReduceDamage( RavageBehavior cfg ) {
				Func<SpaceState, HumanToken, int> originalDamageFrom1 = cfg.AttackDamageFrom1;
				cfg.AttackDamageFrom1 = ( ss, ht ) => ht.Class.IsOneOf( tokenClasses ) // if has penalty
					? Math.Max( 0, originalDamageFrom1( ss, ht ) - damagePenalty ) // apply penalty
					: originalDamageFrom1( ss, ht ); // else, use original
			}
			foreach(var space in ctx.GameState.Spaces) {
				ctx.GameState.ModifyRavage( space.Space, ReduceDamage );
				ctx.GameState.Healer.Skip( space.Space );
			}

		}
	);


}


