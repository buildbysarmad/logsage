/**
 * Decode JWT and extract expiry timestamp
 * Returns expiry in milliseconds, or null if invalid
 */
export function getTokenExpiry(token: string): number | null {
  try {
    const payload = token.split('.')[1];
    if (!payload) return null;

    const decoded = JSON.parse(atob(payload));
    if (!decoded.exp) return null;

    // JWT exp is in seconds, convert to milliseconds
    return decoded.exp * 1000;
  } catch {
    return null;
  }
}
