import { useEffect } from 'preact/hooks';
import { currentGameId, useGame } from './game';
import { GameScreen } from './GameScreen';
import { Lobby } from './Lobby';

export function App() {
  const [state, actions] = useGame();

  // Pick up where the last visit left off: the saved picks are replayed by the server.
  useEffect(() => {
    const id = currentGameId();
    if (id) void actions.open(id);
  }, []);

  return state.gameId && state.response
    ? <GameScreen state={state} actions={actions} />
    : <Lobby busy={state.busy} error={state.error} onStart={r => void actions.start(r)} onOpen={id => void actions.open(id)} />;
}
