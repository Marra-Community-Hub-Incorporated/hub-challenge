import { useEffect, useState } from 'react';
import { Link } from 'react-router';
import { api, type WorkshopSummary } from '../lib/api.ts';
import { formatMelbourne, placesLabel } from '../lib/dates.ts';

export function WorkshopList({ tenant }: { tenant: string }) {
  const [items, setItems] = useState<WorkshopSummary[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setItems(null);
    setError(null);
    api
      .listWorkshops()
      .then((data) => !cancelled && setItems(data))
      .catch((e: Error) => !cancelled && setError(e.message));
    return () => {
      cancelled = true;
    };
  }, [tenant]);

  if (error) return <p className="error">Could not load workshops: {error}</p>;
  if (items === null) return <p className="muted">Loading…</p>;
  if (items.length === 0) return <p className="muted">No workshops yet.</p>;

  return (
    <ul className="cards">
      {items.map((w) => (
        <li key={w.id} className="card">
          <div className="card-body">
            <h2>
              <Link to={`/workshops/${w.id}`}>{w.title}</Link>
            </h2>
            <p>{formatMelbourne(w.startsAtUtc)}</p>
            <p className="muted">{w.location}</p>
          </div>
          <div className="actions">
            <span className="pill">{placesLabel(w.registered, w.capacity)}</span>
            <Link className="button" to={`/workshops/${w.id}`}>
              Register
            </Link>
          </div>
        </li>
      ))}
    </ul>
  );
}
