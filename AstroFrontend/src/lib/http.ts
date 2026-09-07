export function pageParam(url: URL): number | null {
  const value = url.searchParams.get('page');
  if (value === null) return 1;
  if (!/^\d+$/.test(value)) return null;
  const page = Number(value);
  return Number.isSafeInteger(page) && page >= 1 && page <= 1_000_000 ? page : null;
}

export function applyCatalogCache(
  response: { headers: Headers },
  sharedMaxAge = 300,
): void {
  response.headers.set(
    'Cache-Control',
    `public, max-age=0, s-maxage=${sharedMaxAge}, stale-while-revalidate=${sharedMaxAge * 2}`,
  );
}

export function redirectPastLastPage(
  requestedPage: number,
  totalPages: number,
  hrefFor: (page: number) => string,
): Response | null {
  const lastPage = Math.max(1, totalPages);
  if (requestedPage <= lastPage) return null;
  return new Response(null, {
    status: 302,
    headers: { Location: hrefFor(lastPage), 'Cache-Control': 'no-store' },
  });
}
