type BrowseQuery = { q?: string; tag?: string; page?: number };

function withQuery(path: string, values: Record<string, string | number | undefined>): string {
  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(values)) {
    if (value !== undefined && value !== '' && !(key === 'page' && value === 1)) {
      params.set(key, String(value));
    }
  }
  const query = params.toString();
  return query ? `${path}?${query}` : path;
}

const segment = (value: string) => encodeURIComponent(value);

export const homePath = (page = 1) => withQuery('/', { page });
export const explorePath = ({ q, tag, page = 1 }: BrowseQuery = {}) =>
  withQuery('/explore', { q, tag, page });
export const searchPath = (q = '') => withQuery('/search', { q });
export const tagsPath = () => '/tags';
export const actorsPath = (page = 1) => withQuery('/actors', { page });
export const tagPath = (tag: string, page = 1) =>
  withQuery(`/tags/${segment(tag)}`, { page });
export const loginPath = () => '/login';
export const signupPath = () => '/signup';
export const sitemapPath = () => '/sitemap.xml';
export const legacyArtistsPath = (slug?: string) => slug ? `/artists/${segment(slug)}` : '/artists';
export const legacyCategoriesPath = (slug?: string) => slug ? `/categories/${segment(slug)}` : '/categories';
export const legacyWorkPath = (slug: string) => `/works/${segment(slug)}`;

export function actorPath(slug: string, page = 1): string {
  return withQuery(`/actors/${segment(slug)}`, { page });
}

export function albumPath(actorSlug: string, albumSlug: string, page = 1): string {
  return withQuery(`/actors/${segment(actorSlug)}/a/${segment(albumSlug)}`, { page });
}

export function postPath(actorSlug: string, albumSlug: string, postId: string): string {
  return `/actors/${segment(actorSlug)}/a/${segment(albumSlug)}/p/${segment(postId)}`;
}

export function absoluteUrl(path: string, site: URL | undefined): string {
  if (!site) {
    throw new Error(
      'astro.config `site` is not set; canonical URLs, og:url and the sitemap require an absolute origin.',
    );
  }
  return new URL(path, site).toString();
}
