const SERBIAN_NUMBER = new Intl.NumberFormat('sr-Latn-RS', { maximumFractionDigits: 2 });

// Iznos u srpskom formatu: tačka za hiljade, zarez za decimale (5200 → "5.200", 1200.5 → "1.200,5").
// Valutu dodaje šablon, npr. {{ formatAmount(price) }} EC.
export function formatAmount(amount: number): string {
  return SERBIAN_NUMBER.format(amount);
}
