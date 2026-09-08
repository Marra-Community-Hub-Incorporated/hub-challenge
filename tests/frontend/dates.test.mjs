// Runs with `npm run test:frontend` (node --test with type stripping, no build step).
import { test } from 'node:test';
import assert from 'node:assert/strict';
import { formatMelbourne, placesLabel } from '../../src/lib/dates.ts';

test('formatMelbourne renders a UTC instant in Melbourne time', () => {
  // 08:00Z on 24 Sep 2026 is 6:00 pm AEST (UTC+10, before daylight saving starts in October).
  assert.equal(formatMelbourne('2026-09-24T08:00:00Z'), 'Thu, 24 Sept, 6:00 pm');
});

test('formatMelbourne applies daylight saving after the first Sunday in October', () => {
  // 08:00Z on 17 Oct 2026 is 7:00 pm AEDT (UTC+11).
  assert.equal(formatMelbourne('2026-10-17T08:00:00Z'), 'Sat, 17 Oct, 7:00 pm');
});

test('formatMelbourne tolerates garbage', () => {
  assert.equal(formatMelbourne('not a date'), 'Unknown time');
});

test('placesLabel', () => {
  assert.equal(placesLabel(0, 0), 'Unlimited places');
  assert.equal(placesLabel(3, 12), '3 of 12 places taken');
  assert.equal(placesLabel(12, 12), 'Full');
  assert.equal(placesLabel(13, 12), 'Full');
});
