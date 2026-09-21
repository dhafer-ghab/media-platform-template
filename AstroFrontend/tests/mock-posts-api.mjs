import http from 'node:http';

const port = 5199;
const categories = ['Painting', 'Digital', 'Photography', 'Sculpture'];
const tags = ['abstract', 'portrait', 'minimal', 'color'];
const posts = Array.from({ length: 24 }, (_, index) => ({
  id: `10000000-0000-4000-8000-${String(index + 1).padStart(12, '0')}`,
  authorId: `20000000-0000-4000-8000-${String((index % 5) + 1).padStart(12, '0')}`,
  title: index === 0 ? 'Market at Noon' : `Studio Work ${index + 1}`,
  description: index === 0 ? 'An abstract market study.' : `Published studio work number ${index + 1}.`,
  mediaUrl: 'https://images.unsplash.com/photo-1541961017774-22349e4a1262?auto=format&fit=crop&w=900&q=75',
  category: categories[index % categories.length],
  tags: [tags[index % tags.length]],
  status: 'Published',
  createdDate: new Date(Date.UTC(2026, 6, index + 1)).toISOString(),
  updateDate: null,
  publishedAt: new Date(Date.UTC(2026, 6, index + 1)).toISOString(),
  views: 100 - index,
}));

function json(response, status, value) {
  response.writeHead(status, { 'Content-Type': 'application/json' });
  response.end(JSON.stringify(value));
}

function facets(items, selector) {
  const counts = new Map();
  for (const item of items.flatMap(selector)) counts.set(item, (counts.get(item) ?? 0) + 1);
  return [...counts.entries()]
    .map(([name, count]) => ({ name, count }))
    .sort((a, b) => a.name.localeCompare(b.name));
}

const server = http.createServer((request, response) => {
  const url = new URL(request.url ?? '/', `http://127.0.0.1:${port}`);
  if (url.pathname === '/health') return json(response, 200, { status: 'ok' });

  if (url.pathname === '/api/posts/facets' && request.method === 'GET') {
    return json(response, 200, {
      categories: facets(posts, (post) => [post.category]),
      tags: facets(posts, (post) => post.tags),
    });
  }

  if (url.pathname === '/api/posts' && request.method === 'GET') {
    const q = (url.searchParams.get('q') ?? '').toLowerCase();
    const category = (url.searchParams.get('category') ?? '').toLowerCase();
    const tag = (url.searchParams.get('tag') ?? '').toLowerCase();
    const sort = url.searchParams.get('sort') ?? 'newest';
    const page = Math.max(1, Number(url.searchParams.get('page') ?? 1));
    const pageSize = Math.max(1, Number(url.searchParams.get('pageSize') ?? 20));

    let filtered = posts.filter((post) =>
      (!q || `${post.title} ${post.description} ${post.category}`.toLowerCase().includes(q)) &&
      (!category || post.category.toLowerCase() === category) &&
      (!tag || post.tags.includes(tag)));

    filtered = [...filtered].sort((a, b) => {
      if (sort === 'popular') return b.views - a.views;
      if (sort === 'oldest') return a.publishedAt.localeCompare(b.publishedAt);
      if (sort === 'title') return a.title.localeCompare(b.title);
      return b.publishedAt.localeCompare(a.publishedAt);
    });

    const totalCount = filtered.length;
    const items = filtered.slice((page - 1) * pageSize, page * pageSize)
      .map(({ views, ...post }) => post);
    return json(response, 200, {
      items,
      page,
      pageSize,
      totalCount,
      totalPages: totalCount === 0 ? 0 : Math.ceil(totalCount / pageSize),
    });
  }

  const postMatch = url.pathname.match(/^\/api\/posts\/([0-9a-f-]+)$/i);
  if (postMatch && request.method === 'GET') {
    const post = posts.find((candidate) => candidate.id === postMatch[1]);
    if (!post) return json(response, 404, { code: 'Post.NotFound' });
    const { views, ...body } = post;
    return json(response, 200, body);
  }

  const viewMatch = url.pathname.match(/^\/api\/posts\/([0-9a-f-]+)\/views$/i);
  if (viewMatch && request.method === 'POST') {
    const post = posts.find((candidate) => candidate.id === viewMatch[1]);
    if (!post) return json(response, 404, { code: 'Post.NotFound' });
    post.views++;
    response.writeHead(204);
    return response.end();
  }

  return json(response, 404, { code: 'NotFound' });
});

server.listen(port, '127.0.0.1', () => {
  console.log(`Mock Posts API listening on http://127.0.0.1:${port}`);
});

for (const signal of ['SIGINT', 'SIGTERM']) {
  process.on(signal, () => server.close(() => process.exit(0)));
}
