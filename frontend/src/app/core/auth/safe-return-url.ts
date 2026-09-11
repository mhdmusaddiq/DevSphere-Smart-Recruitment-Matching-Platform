export function safeInternalReturnUrl(value: string | null | undefined): string {
  if (!value || !value.startsWith('/') || value.startsWith('//')) {
    return '/jobs';
  }

  if (value.includes('\\') || /^[a-z][a-z\d+.-]*:/i.test(value.slice(1))) {
    return '/jobs';
  }

  return value;
}
