import { useState } from 'react';
import { api } from '../api';
import type { AuthResponse } from '../types';

type Props = {
  onAuthenticated: (auth: AuthResponse) => void;
};

export function LoginPanel({ onAuthenticated }: Props) {
  const [mode, setMode] = useState<'login' | 'signup'>('login');
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const auth =
        mode === 'login'
          ? await api.auth.login({ Email: email, Password: password })
          : await api.users.create({ Name: name, Email: email, Password: password });

      if (auth) onAuthenticated(auth);
    } catch (err) {
      setError((err as Error).message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container">
      <div className="card" style={{ maxWidth: 360, margin: '64px auto' }}>
        <h2>{mode === 'login' ? 'Entrar' : 'Criar conta'}</h2>
        {error && <div className="error">{error}</div>}
        <form onSubmit={submit}>
          {mode === 'signup' && (
            <input placeholder="Nome" value={name} onChange={(e) => setName(e.target.value)} required />
          )}
          <input
            type="email"
            placeholder="E-mail"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
          <input
            type="password"
            placeholder="Senha"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
          <button type="submit" className="primary" disabled={loading}>
            {mode === 'login' ? 'Entrar' : 'Criar conta'}
          </button>
        </form>
        <p className="muted" style={{ marginTop: 12 }}>
          {mode === 'login' ? (
            <>
              Não tem conta?{' '}
              <a href="#" onClick={(e) => { e.preventDefault(); setMode('signup'); setError(null); }}>
                Criar conta
              </a>
            </>
          ) : (
            <>
              Já tem conta?{' '}
              <a href="#" onClick={(e) => { e.preventDefault(); setMode('login'); setError(null); }}>
                Entrar
              </a>
            </>
          )}
        </p>
      </div>
    </div>
  );
}
