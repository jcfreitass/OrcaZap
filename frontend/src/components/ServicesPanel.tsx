import { useEffect, useState } from 'react';
import { api } from '../api';
import type { Service } from '../types';

export function ServicesPanel() {
  const [services, setServices] = useState<Service[]>([]);
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [price, setPrice] = useState<number | ''>('');
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    try {
      const s = await api.services.list();
      setServices(s ?? []);
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
    if (price === '') {
      setError('Preço é obrigatório.');
      return;
    }
    try {
      await api.services.create({
        Name: name,
        Description: description,
        Price: Number(price),
      });
      setName('');
      setDescription('');
      setPrice('');
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const remove = async (id: number) => {
    if (!confirm(`Remover serviço ${id}?`)) return;
    try {
      await api.services.remove(id);
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  };

  return (
    <>
      <div className="card">
        <h2>Novo serviço</h2>
        {error && <div className="error">{error}</div>}
        <form onSubmit={create}>
          <input placeholder="Nome" value={name} onChange={(e) => setName(e.target.value)} required />
          <input
            placeholder="Descrição"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <input
            type="number"
            step="0.01"
            placeholder="Preço"
            value={price}
            onChange={(e) => setPrice(e.target.value ? Number(e.target.value) : '')}
            required
          />
          <button type="submit" className="primary">
            Criar
          </button>
        </form>
      </div>

      <div className="card">
        <h2>Serviços cadastrados</h2>
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Nome</th>
              <th>Descrição</th>
              <th>Preço</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {services.map((s) => (
              <tr key={s.Id}>
                <td>{s.Id}</td>
                <td>{s.Name}</td>
                <td>{s.Description ?? '-'}</td>
                <td>R$ {s.Price.toFixed(2)}</td>
                <td>
                  <button className="danger" onClick={() => remove(s.Id)}>
                    Remover
                  </button>
                </td>
              </tr>
            ))}
            {services.length === 0 && (
              <tr>
                <td colSpan={5} className="muted">
                  Nenhum serviço cadastrado.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </>
  );
}
