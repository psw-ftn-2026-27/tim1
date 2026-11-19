const SHORT_DATE = new Intl.DateTimeFormat('en-GB', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
});

// Datum sa servera (ISO tekst) u obliku "15 Nov 2026", u lokalnom vremenu korisnika.
// Šablon ga koristi kao {{ formatDate(createdAt) }}.
export function formatDate(value: string): string {
  return SHORT_DATE.format(new Date(value));
}
