import { useEffect, useState } from 'react';
import { api } from '../api';
import type { Customer, Quote, Service } from '../types';
import { QuoteStatus, QuoteStatusLabel } from '../types';

type ItemDraft = { ServiceId: number | null; Description: string; Quantity: number; UnitPrice: number };

export function QuotesPanel() {
  const [quotes, setQuotes] = useState<Quote[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [services, setServices] = useState<Service[]>([]);
  const [customerId, setCustomerId] = useState<number | ''>('');
  const [items, setItems] = useState<ItemDraft[]>([
    { ServiceId: null, Description: '', Quantity: 1, UnitPrice: 0 },
  ]);
  const [error, setError] = useState<string | null>(null);

  const load = async () => {
    try {
      const [q, c, s] = await Promise.all([api.quotes.list(), api.customers.list(), api.services.list()]);
      setQuotes(q ?? []);
      setCustomers(c ?? []);
      setServices(s ?? []);
      if (!customerId && c && c.length > 0) setCustomerId(c[0].Id);
    } catch (e) {
      setError((e as Error).message);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const updateItem = (idx: number, patch: Partial<ItemDraft>) => {
    setItems((prev) => prev.map((it, i) => (i === idx ? { ...it, ...patch } : it)));
  };

  const pickService = (idx: number, serviceIdStr: string) => {
    const serviceId = serviceIdStr ? Number(serviceIdStr) : null;
    const svc = services.find((s) => s.Id === serviceId);
    updateItem(idx, {
      ServiceId: serviceId,
      Description: svc?.Name ?? items[idx].Description,
      UnitPrice: svc?.Price ?? items[idx].UnitPrice,
    });
  };

  const addItem = () =>
    setItems((prev) => [...prev, { ServiceId: null, Description: '', Quantity: 1, UnitPrice: 0 }]);

  const removeItem = (idx: number) =>
    setItems((prev) => (prev.length > 1 ? prev.filter((_, i) => i !== idx) : prev));

  const create = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    if (customerId === '') {
      setError('Cliente é obrigatório.');
      return;
    }
    try {
      await api.quotes.create({
        CustomerId: customerId,
        ValidUntil: null,
        Items: items,
      });
      setItems([{ ServiceId: null, Description: '', Quantity: 1, UnitPrice: 0 }]);
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const remove = async (id: number) => {
    if (!confirm(`Remover orçamento ${id}?`)) return;
    try {
      await api.quotes.remove(id);
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const changeStatus = async (id: number, status: QuoteStatus) => {
    try {
      await api.quotes.updateStatus(id, status);
      await load();
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const openWhatsApp = async (id: number) => {
    try {
      const link = await api.quotes.whatsappLink(id);
      if (link) window.open(link.Url, '_blank');
    } catch (err) {
      setError((err as Error).message);
    }
  };

  const total = items.reduce((acc, it) => acc + Number(it.Quantity) * Number(it.UnitPrice), 0);

  return (
    <>
      <div className="card">
        <h2>Novo orçamento</h2>
        {error && <div className="error">{error}</div>}
        <form onSubmit={create} className="wide">
          <select
            value={customerId}
            onChange={(e) => setCustomerId(e.target.value ? Number(e.target.value) : '')}
            required
          >
            <option value="">Cliente...</option>
            {customers.map((c) => (
              <option key={c.Id} value={c.Id}>
                {c.Name}
              </option>
            ))}
          </select>

          <h3 style={{ margin: '12px 0 4px 0', fontSize: 14 }}>Itens</h3>
          {items.map((it, idx) => (
            <div key={idx} className="item-row">
              <select value={it.ServiceId ?? ''} onChange={(e) => pickService(idx, e.target.value)}>
                <option value="">— livre —</option>
                {services.map((s) => (
                  <option key={s.Id} value={s.Id}>
                    {s.Name}
                  </option>
                ))}
              </select>
              <input
                placeholder="Descrição"
                value={it.Description}
                onChange={(e) => updateItem(idx, { Description: e.target.value })}
                required
              />
              <input
                type="number"
                step="0.01"
                placeholder="Qtd"
                value={it.Quantity}
                onChange={(e) => updateItem(idx, { Quantity: Number(e.target.value) })}
              />
              <input
                type="number"
                step="0.01"
                placeholder="Preço unit."
                value={it.UnitPrice}
                onChange={(e) => updateItem(idx, { UnitPrice: Number(e.target.value) })}
              />
              <button type="button" className="danger" onClick={() => removeItem(idx)}>
                ×
              </button>
            </div>
          ))}

          <div className="row" style={{ justifyContent: 'space-between', marginTop: 8 }}>
            <button type="button" onClick={addItem}>
              + item
            </button>
            <div>
              <strong>Total: R$ {total.toFixed(2)}</strong>
            </div>
          </div>

          <button type="submit" className="primary" style={{ marginTop: 8 }}>
            Criar orçamento
          </button>
        </form>
      </div>

      <div className="card">
        <h2>Orçamentos</h2>
        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Cliente</th>
              <th>Total</th>
              <th>Status</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {quotes.map((q) => (
              <tr key={q.Id}>
                <td>{q.Id}</td>
                <td>{q.CustomerName}</td>
                <td>R$ {q.Total.toFixed(2)}</td>
                <td>
                  <select
                    value={q.Status}
                    onChange={(e) => changeStatus(q.Id, Number(e.target.value) as QuoteStatus)}
                  >
                    {Object.entries(QuoteStatusLabel).map(([k, label]) => (
                      <option key={k} value={k}>
                        {label}
                      </option>
                    ))}
                  </select>
                </td>
                <td>
                  <div className="row">
                    <button className="primary" onClick={() => openWhatsApp(q.Id)}>
                      WhatsApp
                    </button>
                    <button className="danger" onClick={() => remove(q.Id)}>
                      Remover
                    </button>
                  </div>
                </td>
              </tr>
            ))}
            {quotes.length === 0 && (
              <tr>
                <td colSpan={5} className="muted">
                  Nenhum orçamento.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </>
  );
}
