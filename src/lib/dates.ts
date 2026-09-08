// Mini Hub serves one city. Times come from the API in UTC (ISO 8601) and are shown
// in Melbourne time regardless of the viewer's device timezone.
export const MELBOURNE = 'Australia/Melbourne';

const formatter = new Intl.DateTimeFormat('en-AU', {
  timeZone: MELBOURNE,
  weekday: 'short',
  day: 'numeric',
  month: 'short',
  hour: 'numeric',
  minute: '2-digit',
  hour12: true,
});

/** "Thu, 24 Sept, 6:00 pm" for an ISO UTC instant (en-AU spelling). */
export function formatMelbourne(isoUtc: string): string {
  const d = new Date(isoUtc);
  if (Number.isNaN(d.getTime())) return 'Unknown time';
  return formatter.format(d).replace(' at ', ', ');
}

/** "3 of 12 places taken", "Full", or "Unlimited places". */
export function placesLabel(registered: number, capacity: number): string {
  if (capacity === 0) return 'Unlimited places';
  if (registered >= capacity) return 'Full';
  return `${registered} of ${capacity} places taken`;
}
