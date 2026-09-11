export function initials(value: string | null | undefined): string {
  const words = (value ?? '')
    .trim()
    .split(/\s+/)
    .filter(Boolean);

  if (words.length === 0) {
    return '?';
  }

  if (words.length === 1) {
    return words[0].slice(0, 2).toLocaleUpperCase();
  }

  return `${words[0][0]}${words[words.length - 1][0]}`.toLocaleUpperCase();
}
