namespace SpiritIsland;

public abstract class ActionList<T> : IHaveMemento {

	public virtual void Add(T action) => _actions.Add(action);

	public async Task Run( GameState gs ){
		for(int i = 0; i < _actions.Count; ++i ) {
			T action = _actions[i];
			if( await RunAndRemoveAsync( action, gs ))
				_actions.RemoveAt( i-- );
		}
	}

	abstract protected Task<bool> RunAndRemoveAsync( T action, GameState gs );

	protected readonly List<T> _actions = [];

	public object Memento { 
		get{
			var memento = new MyMemento( [.._actions], [] );
			memento.mementos.SaveMany(_actions);
			return memento;
		}
		set {
			MyMemento m = (MyMemento)value;
			m.mementos.Restore();
			_actions.SetItems(m.actions);
		}
	}

	record MyMemento(T[] actions, Dictionary<IHaveMemento,object> mementos );
}

public class PreInvaderPhaseActionList : ActionList<IRunBeforeInvaderPhase> {
	protected override async Task<bool> RunAndRemoveAsync( IRunBeforeInvaderPhase action, GameState gs ){
		await action.BeforeInvaderPhase(gs);
		return action.RemoveAfterRun;
	}
}

public class PostInvaderPhaseActionList : ActionList<IRunAfterInvaderPhase> {
	override protected async Task<bool> RunAndRemoveAsync( IRunAfterInvaderPhase action, GameState gs ){
		await action.AfterInvaderPhase(gs);
		return action.RemoveAfterRun;
	}
}


public class TimePassesActionList : ActionList<IRunWhenTimePasses> {

	public override void Add( IRunWhenTimePasses action ){
		int i = _actions.Count;
		while(0<i && action.Order < _actions[i-1].Order) --i;
		_actions.Insert(i,action);
	}

	override protected async Task<bool> RunAndRemoveAsync( IRunWhenTimePasses action, GameState gs ){
		await action.TimePasses(gs);
		return action.RemoveAfterRun;
	}
}
