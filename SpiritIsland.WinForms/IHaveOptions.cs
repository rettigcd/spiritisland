using System;

namespace SpiritIsland.WinForms; 

public interface IHaveOptions {
	event Action<IDecision> NewDecision;
}