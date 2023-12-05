namespace SpiritIsland.FeatherAndFlame;

public class CommunitiesInDisarray : FearCardBase, IFearCard {

	public const string Name = "Communities in Disarray";
	public string Text => Name;

	[FearLevel( 1, "City each deal -1 Damage during Ravage. Invaders do not heal Damage at the end of this turn." )]
	public Task Level1( GameState ctx ) 
		=> ReduceDamageAndDontHeal( 1, Human.City )
		.ActAsync(ctx);

	[FearLevel( 2, "Town/City each deal -1 Damage during Ravage. Invaders do not heal Damage at the end of this turn." )]
	public Task Level2( GameState ctx )
		=> ReduceDamageAndDontHeal( 1, Human.Town_City )
		.ActAsync( ctx );

	[FearLevel( 3, "Town/City each deal -2 Damage during Ravage. Invaders do not heal Damage at the end of this turn." )]
	public Task Level3( GameState ctx )
		=> ReduceDamageAndDontHeal( 2, Human.Town_City )
		.ActAsync( ctx );

	static BaseCmd<GameState> ReduceDamageAndDontHeal( int damagePenalty, params HumanTokenClass[] tokenClasses )	=> new BaseCmd<GameState>( 
		string.Join(" / ", tokenClasses.Select(x=>x.Label)) + $" each deal -{damagePenalty} Damage during Ravage. Invaders do not heal Damage at the end of this turn.", 
		ctx=> {
			var penalty = new ReduceInvaderAttackBy1(damagePenalty, tokenClasses );
			foreach(var space in ctx.Spaces) {
				space.Adjust( penalty, 1);
				ctx.Healer.Skip( space.Space );
			}

		}
	);


}


