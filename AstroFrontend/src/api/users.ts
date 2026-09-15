import { getToken } from '@/lib/auth';
import { ApiError } from './client';

const BASE_URL = import.meta.env.PUBLIC_API_BASE_URL ?? 'http://localhost:5000';

export type UserRole = 'Admin' | 'User' | 'PremiumUser';

export interface UserDto {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  createdDate: string;
  updateDate: string | null;
}

function authHeaders(): Record<string, string> {
  const token = getToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export const usersApi = {
  /** List all users (GET /api/users). Requires an authenticated admin. */
  async list(): Promise<UserDto[]> {
    const res = await fetch(`${BASE_URL}/api/users`, {
      headers: authHeaders(),
    });
    if (!res.ok) {
      throw new ApiError(res.status, `API error ${res.status} on /api/users`);
    }
    return res.json() as Promise<UserDto[]>;
  },

  /** Delete a user (DELETE /api/users/{id}). Requires an authenticated admin. */
  async delete(id: string): Promise<void> {
    const res = await fetch(`${BASE_URL}/api/users/${id}`, {
      method: 'DELETE',
      headers: authHeaders(),
    });
    if (!res.ok) {
      throw new ApiError(res.status, `API error ${res.status} on /api/users/${id}`);
    }
  },
};
