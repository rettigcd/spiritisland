import { useEffect, useState } from 'preact/hooks';
import { onAccessNeeded, savedToken } from './api/access';
import { AccessPrompt } from './AccessPrompt';
import { currentGameId, useGame } from './game';
import { GameScreen } from './GameScreen';
import { Lobby } from './Lobby';

export function App() {
  const [state, actions] = useGame();
  // Without a saved token there is nothing to ask the server yet; a call it turns down brings the prompt back.
  const [access, setAccess] = useState({ needed: savedToken() === null, rejected: false });
  useEffect(() => onAccessNeeded(rejected => setAccess(before => ({ needed: true, rejected: before.rejected || rejected }))), []);

  // Pick up where the last visit left off: the saved picks are replayed by the server.
  useEffect(() => {
    const id = currentGameId();
    if (id && !access.needed) void actions.open(id);
  }, []);

  if (access.needed) return <AccessPrompt rejected={access.rejected} />;

  return state.gameId && state.response
    ? <GameScreen state={state} actions={actions} />
    : <Lobby busy={state.busy} error={state.error} onStart={r => void actions.start(r)} onOpen={id => void actions.open(id)} />;
}
