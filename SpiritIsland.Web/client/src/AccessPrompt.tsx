import { useState } from 'preact/hooks';
import { saveToken } from './api/access';

/** Asks for the shared access token, keeps it for next time, and starts the app again with it. */
export function AccessPrompt({ rejected }: { rejected: boolean }) {
  const [token, setToken] = useState('');

  const submit = (e: Event) => {
    e.preventDefault();
    if (token.trim() === '') return;
    saveToken(token.trim());
    window.location.reload(); // start again, so whatever was being asked for is asked for again with the token
  };

  return (
    <main class="lobby">
      <h1>Spirit Island</h1>
      <form class="panel" onSubmit={submit}>
        <h2>Access token</h2>
        <p class="muted">Enter the access token to use this server.</p>
        {rejected && <p class="error" role="alert">That access token was not accepted.</p>}
        <label>Access token
          <input type="password" value={token} autofocus autocomplete="off" onInput={e => setToken(e.currentTarget.value)} />
        </label>
        <button type="submit" disabled={token.trim() === ''}>Continue</button>
      </form>
    </main>
  );
}
