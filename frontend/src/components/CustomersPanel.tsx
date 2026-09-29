import { useEffect, useState } from 'react';
import { api } from '../api';
import type { Customer } from '../types';

export function CustomersPanel() {
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [email, setEmail] = useState('');
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    try {
      const c = await api.customers.list();
      setCustomers(c ?? []);
    } catch (e) {
      setError((e as Error).message);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const create = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    try {
      await api.customers.create({ Name: name, Phone: phone, Email: email });
      setName('');
      setPhone('');
      setEmail('');
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const remove = async (id: number) => {
    if (!confirm(`Remover cliente ${id}?`)) return;
    try {
      await api.customers.remove(id);
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  };

  return (
    <>
      <div className="card">
        <h2>Novo cliente</h2>
        {error && <div className="error">{error}</div>}
        <form onSubmit={create}>
          <input placeholder="Nome" value={name} onChange={(e) => setName(e.target.value)} required />
          <input placeholder="Telefone (com DDD)" value={phone} onChange={(e) => setPhone(e.target.value)} />
          <input
            type="email"
            placeholder="E-mail"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <button type="submit" className="primary">
            Criar
          </button>
        </form>
      </div>

      <div className="card">
        <h2>Clientes cadastrados</h2>
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Nome</th>
              <th>Telefone</th>
              <th>E-mail</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {customers.map((c) => (
              <tr key={c.Id}>
                <td>{c.Id}</td>
                <td>{c.Name}</td>
                <td>{c.Phone ?? '-'}</td>
                <td>{c.Email ?? '-'}</td>
                <td>
                  <button className="danger" onClick={() => remove(c.Id)}>
                    Remover
                  </button>
                </td>
              </tr>
            ))}
            {customers.length === 0 && (
              <tr>
                <td colSpan={5} className="muted">
                  Nenhum cliente cadastrado.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </>
  );
}
