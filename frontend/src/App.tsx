import { useEffect, useState } from 'react';
import { getToken, onUnauthorized, setToken } from './api';
import type { AuthResponse, User } from './types';
import { LoginPanel } from './components/LoginPanel';
import { UsersPanel } from './components/UsersPanel';
import { CustomersPanel } from './components/CustomersPanel';
import { ServicesPanel } from './components/ServicesPanel';
import { QuotesPanel } from './components/QuotesPanel';

type Tab = 'users' | 'customers' | 'services' | 'quotes';

const USER_KEY = 'orcazap_user';

function loadStoredUser(): User | null {
  const raw = localStorage.getItem(USER_KEY);
  return raw ? (JSON.parse(raw) as User) : null;
}

export function App() {
  const [tab, setTab] = useState<Tab>('customers');
  const [user, setUser] = useState<User | null>(() => (getToken() ? loadStoredUser() : null));

  const handleAuthenticated = (auth: AuthResponse) => {
    setToken(auth.Token);
    localStorage.setItem(USER_KEY, JSON.stringify(auth.User));
    setUser(auth.User);
  };

  const logout = () => {
    setToken(null);
    localStorage.removeItem(USER_KEY);
    setUser(null);
  };

  useEffect(() => {
    onUnauthorized(logout);
  }, []);

  if (!user) {
    return <LoginPanel onAuthenticated={handleAuthenticated} />;
  }

  return (
    <div className="container">
      <header className="top">
        <h1>OrcaZap</h1>
        <nav>
          <button className={tab === 'customers' ? 'active' : ''} onClick={() => setTab('customers')}>
            Clientes
          </button>
          <button className={tab === 'services' ? 'active' : ''} onClick={() => setTab('services')}>
            Serviços
          </button>
          <button className={tab === 'quotes' ? 'active' : ''} onClick={() => setTab('quotes')}>
            Orçamentos
          </button>
          <button className={tab === 'users' ? 'active' : ''} onClick={() => setTab('users')}>
            Usuários
          </button>
        </nav>
        <div className="row">
          <span className="muted">{user.Name}</span>
          <button onClick={logout}>Sair</button>
        </div>
      </header>

      {tab === 'users' && <UsersPanel />}
      {tab === 'customers' && <CustomersPanel />}
      {tab === 'services' && <ServicesPanel />}
      {tab === 'quotes' && <QuotesPanel />}
    </div>
  );
}
