For a list of many test methods, see:
- SpiritExtensions.cs
- SpaceExtensions.cs

# Init Tokens on a Space

a5.Given_InitSummary("2E@1") - sets all visible tokens
a5.Given_HasToken("2E@1") - sets named tokens
a5.Given_ClearTokens() - removes visible tokens


# Init Spirits presence on a Space
	spirit.Given_IsOn(space)
	spirit.Given_IsOn(space,2)

# Configure Game


var gameState = new GameConfiguration()
	.ConfigAdversary(new AdversaryConfig(Scotland.Name, level))
	.ConfigBoards("A")
	.ConfigSpirits(RiverSurges.Name)
	.BuildGame()

# Invaders Cards

## Get a Card for a specific SPACE
```
var card1 = space.BuildInvaderCard(); // card will match other spaces also.
var card2 = spaceSpec.BuildInvaderCard(); // card will match other spaces also.
```
## Get a Card for a specific TERRAIN
```
var terrainCard = InvaderDeckBuilder.Stage1( Terrain.Sand );
var coast = InvaderDeckBuilder.Stage2Coastal;
```

DO NOT
- Pull it from InvaderDeckBuilder.Level1Cards
- Search the Deck until it matches land (gs.InvaderDeck.Unreavealed.First(x=>x.Matches(a1)))
- Search the Deck until title matches

## Build

### Flow:
- BuildSlot.ActivateCard( card )
- Engine.ActivateCard( card )
- Adds Build Tokens, then.. does builds.
- Engine.DoAllBuildsInSpace( space )
- Engine.TryToDo1Build()

## Build on all spaces for a specific Invader Card
```
// see above for generating Invader Card for specific Terrain
await invaderCard.When_Building();

```
## Ravaging

Flow:
	RavageSlot.ActivateCard( card )
		> Engine.ActivateCard( card )
			> Adds Ravage Tokens, then does ravages.
				> Engine.DoAllRavagesOn1Space( space )
					> ravageSpace.Ravage();
						> RavageBehavior.Exec( this );
							> IConfigRavages(...)
							> do RavageSequence()


# Set Spirit's Elements
```
spirit.Configure().Elements("3 moon,2 animal");
```

# Test a Power Card

## Call Directly
- Don't need: scope, ActionScope.Owner, nor Spirit initialization
- Already know target (spirit or space) and want to specify via code
- Doesn't require setting up targetting Source/Range/Destination
```
await BlazingRenewal.ActAsync( spirit.Target(spirit) ).AwaitUser(...);
await InfinitVitality.ActAsync( spirit.Target(space) );
```
## Invoke through Spirit
- Use when you need ActionScope.Owner initialized and spirit initialization
- Specify target via User Selection (u.NextDecsiion.Choose(...))
- Requires setting up targetting Source/Range/Destination
```
spirit.When_ResolvingCard<T>( user=>{...} );
spirit.Task When_ResolvingInnate<T>( user=>{...} );
```

These methods come with .ShouldComplete()


