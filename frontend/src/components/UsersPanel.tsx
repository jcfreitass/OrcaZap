import { useEffect, useState } from 'react';
import { api } from '../api';
import type { User } from '../types';

export function UsersPanel() {
  const [users, setUsers] = useState<User[]>([]);
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    try {
      const list = await api.users.list();
      setUsers(list ?? []);
    } catch (e) {
      setError((e as Error).message);
    }
  };

  useEffect(() => {
    load();
  }, []);

  return (
    <div className="card">
      <h2>Usuários cadastrados</h2>
      {error && <div className="error">{error}</div>}
      <table>
        <thead>
          <tr>
            <th>ID</th>
            <th>Nome</th>
            <th>E-mail</th>
            <th>Criado em</th>
          </tr>
        </thead>
        <tbody>
          {users.map((u) => (
            <tr key={u.Id}>
              <td>{u.Id}</td>
              <td>{u.Name}</td>
              <td>{u.Email}</td>
              <td>{new Date(u.CreatedAt).toLocaleString('pt-BR')}</td>
            </tr>
          ))}
          {users.length === 0 && (
            <tr>
              <td colSpan={4} className="muted">
                Nenhum usuário cadastrado.
              </td>
            </tr>
          )}
        </tbody>
      </table>
    </div>
  );
}
