﻿using System;
using System.Threading.Tasks;

namespace SpiritIsland.BranchAndClaw.Minor {

	// Change Speed - delayed.  They don't have to pick it immediately - similar to Lightning
	public class ChangeSpeed : IActionFactory {

		public bool IsActiveDuring( Speed speed ) => speed == Speed.Fast || speed == Speed.Slow;
		public bool IsInactiveAfter( Speed speed ) => speed == Speed.Slow;

		public SpeedOverride OverrideSpeed { get => null; set => throw new InvalidOperationException(); }

		public string Name => "Change Speed";

		public string Text => Name;

		public Task ActivateAsync( Spirit spirit, GameState gameState ) {
			return new SpeedChanger( spirit, gameState, Speed.Fast, 2 ).Exec();
		}

		public void UpdateFromSpiritState( CountDictionary<Element> elements ) {} // no effect
	}
}
