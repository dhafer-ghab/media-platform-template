export type TopArtworkView = {
  slug: string;
  viewCount: number;
};

const apiUrl = import.meta.env.PUBLIC_API_BASE_URL?.replace(/\/$/, '');
const inFlightViews = new Set<string>();
const recordedViews = new Set<string>();

export async function recordArtworkViewOnce(slug: string): Promise<void> {
  if (!apiUrl) return;

  const storageKey = `verso:artwork-view:${slug}`;
  if (recordedViews.has(slug)) return;
  try {
    if (sessionStorage.getItem(storageKey)) return;
    sessionStorage.setItem(storageKey, 'pending');
  } catch {
    // Tracking should never interfere with the artwork page.
  }
  if (inFlightViews.has(slug)) return;
  inFlightViews.add(slug);

  try {
    const response = await fetch(`${apiUrl}/api/artwork-views`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ slug }),
      keepalive: true,
    });
    if (response.ok) {
      recordedViews.add(slug);
      try {
        sessionStorage.setItem(storageKey, '1');
      } catch {
        // The request was accepted even when browser storage is unavailable.
      }
    } else {
      try { sessionStorage.removeItem(storageKey); } catch { /* ignore */ }
    }
  } catch {
    try { sessionStorage.removeItem(storageKey); } catch { /* ignore */ }
    // View counts are best-effort analytics.
  } finally {
    inFlightViews.delete(slug);
  }
}

export async function getTopArtworkViews(limit: number): Promise<TopArtworkView[]> {
  if (!apiUrl) return [];

  const controller = new AbortController();
  const timeout = window.setTimeout(() => controller.abort(), 3000);
  try {
    const response = await fetch(`${apiUrl}/api/artwork-views/top?limit=${limit}`, {
      signal: controller.signal,
    });
    if (!response.ok) return [];

    const payload = await response.json() as unknown;
    if (!payload || typeof payload !== 'object' || !Array.isArray((payload as { items?: unknown }).items)) {
      return [];
    }

    return (payload as { items: unknown[] }).items.filter((item): item is TopArtworkView => {
      if (!item || typeof item !== 'object') return false;
      const candidate = item as Partial<TopArtworkView>;
      return typeof candidate.slug === 'string' &&
        typeof candidate.viewCount === 'number' &&
        Number.isSafeInteger(candidate.viewCount) &&
        candidate.viewCount >= 0;
    });
  } catch {
    return [];
  } finally {
    window.clearTimeout(timeout);
  }
}
