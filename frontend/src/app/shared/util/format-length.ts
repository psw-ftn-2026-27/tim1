const SERBIAN_LENGTH = new Intl.NumberFormat('sr-Latn-RS', { maximumFractionDigits: 1 });

// Dužina u srpskom formatu, sa jednom decimalom i decimalnim zarezom (3.24 → "3,2", 42 → "42").
// Jedinicu dodaje šablon, npr. {{ formatLength(tour.length) }} km.
export function formatLength(kilometres: number): string {
  return SERBIAN_LENGTH.format(kilometres);
}
