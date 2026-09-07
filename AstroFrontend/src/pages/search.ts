import type { APIRoute } from 'astro';
import { explorePath } from '@/lib/routes';

export const prerender = false;

export const GET: APIRoute = ({ redirect, url }) =>
  redirect(explorePath({ q: url.searchParams.get('q')?.trim() || undefined }), 308);
