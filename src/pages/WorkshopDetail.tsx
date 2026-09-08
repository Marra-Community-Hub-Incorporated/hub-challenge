import { useEffect, useState, type FormEvent } from 'react';
import { Link, useParams } from 'react-router';
import { api, type WorkshopDetail as Detail } from '../lib/api.ts';
import { formatMelbourne, placesLabel } from '../lib/dates.ts';

export function WorkshopDetail({ tenant }: { tenant: string }) {
  const { id = '' } = useParams();
  const [item, setItem] = useState<Detail | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [status, setStatus] = useState<'idle' | 'saving' | 'done'>('idle');
  const [formError, setFormError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    setItem(null);
    setError(null);
    api
      .getWorkshop(id)
      .then((data) => !cancelled && setItem(data))
      .catch((e: Error) => !cancelled && setError(e.message));
    return () => {
      cancelled = true;
    };
  }, [id, tenant]);

  async function submit(e: FormEvent) {
    e.preventDefault();
    setStatus('saving');
    setFormError(null);
    try {
      await api.register(id, name, email);
      setStatus('done');
      setItem(await api.getWorkshop(id));
    } catch (err) {
      setStatus('idle');
      setFormError((err as Error).message);
    }
  }

  if (error) return <p className="error">Could not load this workshop: {error}</p>;
  if (item === null) return <p className="muted">Loading…</p>;

  return (
    <article>
      <p>
        <Link to="/">← All workshops</Link>
      </p>
      <h1>{item.title}</h1>
      <p>
        <strong>{formatMelbourne(item.startsAtUtc)}</strong> · {item.location}
      </p>
      <p>{item.description}</p>
      <p className="pill">{placesLabel(item.registered, item.capacity)}</p>

      <section className="card">
        <div className="card-body">
          <h2>Register</h2>
          {status === 'done' ? (
            <p>You're registered. See you there.</p>
          ) : (
            <form onSubmit={submit}>
              <label>
                Name
                <input value={name} onChange={(e) => setName(e.target.value)} required />
              </label>
              <label>
                Email
                <input
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </label>
              {formError && <p className="error">{formError}</p>}
              <button className="button" type="submit" disabled={status === 'saving'}>
                {status === 'saving' ? 'Registering…' : 'Register'}
              </button>
            </form>
          )}
        </div>
      </section>
    </article>
  );
}
