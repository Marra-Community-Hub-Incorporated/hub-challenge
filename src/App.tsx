import { useState } from 'react';
import { BrowserRouter, Link, Route, Routes } from 'react-router';
import { currentTenant, setTenant } from './lib/api.ts';
import { WorkshopDetail } from './pages/WorkshopDetail.tsx';
import { WorkshopList } from './pages/WorkshopList.tsx';

const TENANTS = [
  { slug: 'demo', name: 'Demo Community Hub' },
  { slug: 'other-org', name: 'Some Other Organisation' },
];

export function App() {
  const [tenant, setTenantState] = useState(currentTenant);

  function switchTenant(slug: string) {
    setTenant(slug);
    setTenantState(slug);
  }

  return (
    <BrowserRouter>
      <header className="topbar">
        <Link to="/" className="brand">
          Mini Hub
        </Link>
        <label className="tenant-picker">
          Organisation
          <select value={tenant} onChange={(e) => switchTenant(e.target.value)}>
            {TENANTS.map((t) => (
              <option key={t.slug} value={t.slug}>
                {t.name}
              </option>
            ))}
          </select>
        </label>
      </header>
      <main>
        <Routes>
          <Route path="/" element={<WorkshopList tenant={tenant} />} />
          <Route path="/workshops/:id" element={<WorkshopDetail tenant={tenant} />} />
        </Routes>
      </main>
    </BrowserRouter>
  );
}
