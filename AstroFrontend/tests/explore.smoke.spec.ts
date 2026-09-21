import { expect, test } from '@playwright/test';

test('home renders popular and newest posts from the API', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('heading', { name: 'Discover work worth returning to.' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Most viewed' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Newest uploads' })).toBeVisible();
  await expect(page.getByRole('link', { name: 'View Market at Noon' }).first()).toBeVisible();
});

test('theme toggle switches modes and remembers the selection', async ({ page }) => {
  await page.goto('/explore');
  await page.evaluate(() => localStorage.setItem('verso:theme', 'light'));
  await page.reload();
  const toggle = page.getByRole('button', { name: 'Switch to dark mode' });

  await expect(toggle).toBeVisible();
  await toggle.click();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
  await expect(page.getByRole('button', { name: 'Switch to light mode' })).toBeVisible();

  await page.reload();
  await expect(page.locator('html')).toHaveAttribute('data-theme', 'dark');
});

test('Explore renders a clean feed with a reserved ad placement', async ({ page }) => {
  await page.goto('/explore');

  await expect(page.getByRole('heading', { name: 'Explore the collection.' })).toBeVisible();
  await expect(page.getByText('24', { exact: true })).toBeVisible();
  await expect(page.getByLabel('Advertisement')).toBeVisible();
  const filters = page.getByRole('form', { name: 'Explore filters' });
  await expect(filters).toBeVisible();
  await expect(filters.getByLabel('Category')).toContainText('Painting');
  await expect(filters.getByLabel('Tag')).toContainText('#abstract');
});

test('tag page renders a dedicated server-side result page', async ({ page }) => {
  await page.goto('/tags/abstract');

  await expect(page.getByRole('heading', { name: '#abstract' })).toBeVisible();
  await expect(page.getByText('6 published works')).toBeVisible();
});

test('tag index lists available tags and links to their result pages', async ({ page }) => {
  await page.goto('/tags');

  await expect(page.getByRole('heading', { name: 'Follow an idea.' })).toBeVisible();
  await expect(page.locator('.tag-card', { hasText: '#abstract' })).toBeVisible();
  await page.locator('.tag-card', { hasText: '#abstract' }).click();
  await expect(page).toHaveURL(/\/tags\/abstract$/);
});

test('tag index can be filtered by tag name', async ({ page }) => {
  await page.goto('/tags');
  await page.getByRole('searchbox', { name: 'Search tags' }).fill('mini');
  await page.getByRole('button', { name: 'Find tags' }).click();

  await expect(page).toHaveURL(/\/tags\?q=mini$/);
  await expect(page.locator('.tag-card', { hasText: '#minimal' })).toBeVisible();
  await expect(page.locator('.tag-card', { hasText: '#abstract' })).toHaveCount(0);
  await expect(page.getByText('1 tag matching “mini”')).toBeVisible();
});

test('search form redirects to a keyword URL and renders results', async ({ page }) => {
  await page.goto('/search');
  const searchForm = page.locator('main').getByRole('search');
  await searchForm.getByLabel('Keyword').fill('market');
  await searchForm.getByRole('button', { name: 'Search' }).click();

  await expect(page).toHaveURL(/\/search\/market$/);
  await expect(page.getByRole('heading', { name: '“market”' })).toBeVisible();
  await expect(page.getByText('1 result')).toBeVisible();
});

test('navbar keyword and sort filter create shareable search results', async ({ page }) => {
  await page.goto('/explore');
  await page.getByRole('searchbox', { name: 'Keyword' }).fill('studio');
  await page.getByLabel('Filter and sort').click();
  await page.getByRole('radio', { name: 'Most viewed' }).check();
  await page.getByRole('button', { name: /View results/ }).click();

  await expect(page).toHaveURL(/\/search\/studio\?sort=popular$/);
  await expect(page.getByRole('heading', { name: '“studio”' })).toBeVisible();
  await expect(page.getByLabel('Filter and sort')).toHaveClass(/is-active/);
});

test('public SSR pages send cache headers and only use theme scripts', async ({ page, request }) => {
  const publicPages = [
    { path: '/', maxAge: 60, sharedMaxAge: 300 },
    { path: '/explore', maxAge: 120, sharedMaxAge: 600 },
    { path: '/tags', maxAge: 120, sharedMaxAge: 600 },
    { path: '/tags/abstract', maxAge: 120, sharedMaxAge: 600 },
    { path: '/search', maxAge: 120, sharedMaxAge: 600 },
    { path: '/search/market', maxAge: 120, sharedMaxAge: 600 },
  ];

  for (const { path, maxAge, sharedMaxAge } of publicPages) {
    const response = await request.get(path);
    const cacheControl = response.headers()['cache-control'];
    expect(cacheControl).toContain(`max-age=${maxAge}`);
    expect(cacheControl).toContain(`s-maxage=${sharedMaxAge}`);
    expect(cacheControl).toContain('stale-while-revalidate=3600');

    await page.goto(path);
    await expect(page.locator('script:not([data-theme-script])')).toHaveCount(0);
  }

  const detailResponse = await request.get(
    '/posts/10000000-0000-4000-8000-000000000001',
  );
  expect(detailResponse.headers()['cache-control']).toBe('no-store');

  const missingResponse = await request.get('/posts/not-a-guid');
  expect(missingResponse.status()).toBe(404);
  expect(missingResponse.headers()['cache-control']).toBe('no-store');
});

test('Explore remains usable on a mobile viewport', async ({ page }) => {
  await page.goto('/explore');

  await expect(page.getByRole('heading', { name: 'Explore the collection.' })).toBeVisible();
  await expect(page.getByLabel('Advertisement')).toBeVisible();

  const hasHorizontalOverflow = await page.evaluate(
    () => document.documentElement.scrollWidth > document.documentElement.clientWidth,
  );
  expect(hasHorizontalOverflow).toBe(false);
});
