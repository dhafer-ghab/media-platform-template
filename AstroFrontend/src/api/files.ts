import { getToken } from '@/lib/auth';
import { ApiError } from './client';

const BASE_URL = import.meta.env.PUBLIC_API_BASE_URL ?? 'http://localhost:5000';

export interface UploadResponse {
  id: string;
}

export interface FileDto {
  id: string;
  fileName: string;
  originalFileName: string;
  contentType: string;
  fileSize: number;
  storageProvider: string;
  bucketName: string;
  storageKey: string;
  url: string;
  createdAt: string;
}

function authHeaders(): Record<string, string> {
  const token = getToken();
  return token ? { Authorization: `Bearer ${token}` } : {};
}

export const filesApi = {

  upload(file: File, onProgress?: (percent: number) => void): Promise<UploadResponse> {
    return new Promise((resolve, reject) => {
      const formData = new FormData();
      formData.append('file', file);

      const xhr = new XMLHttpRequest();
      xhr.open('POST', `${BASE_URL}/api/files`);
      xhr.setRequestHeader('Authorization', authHeaders().Authorization ?? '');

      xhr.upload.addEventListener('progress', (e) => {
        if (e.lengthComputable && onProgress) {
          onProgress(Math.round((e.loaded / e.total) * 100));
        }
      });

      xhr.addEventListener('load', () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          resolve(JSON.parse(xhr.responseText) as UploadResponse);
        } else {
          reject(new ApiError(xhr.status, `Upload failed with status ${xhr.status}`));
        }
      });
      xhr.addEventListener('error', () => reject(new ApiError(0, 'Network error during upload')));
      xhr.addEventListener('abort', () => reject(new ApiError(0, 'Upload aborted')));

      xhr.send(formData);
    });
  },

  /** List all uploaded files. Requires an authenticated admin. */
  async list(): Promise<FileDto[]> {
    const res = await fetch(`${BASE_URL}/api/files`, {
      headers: authHeaders(),
    });
    if (!res.ok) {
      throw new ApiError(res.status, `API error ${res.status} on /api/files`);
    }
    return res.json() as Promise<FileDto[]>;
  },

 /**
 * Sends an array of strings to your backend.
 * Replace the URL and adjust the request format as needed.
 */
  async sendStringList(urls: string[]): Promise<Response> {
    const response = await fetch(`${BASE_URL}/api/files/import`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...authHeaders()
        // 'Authorization': authHeaders().Authorization ?? ''
      },
      body: JSON.stringify({ urls }),
    });
    
    if (!response.ok) {
      throw new Error(`Request failed with status ${response.status}`);
    }

    return response;
  },

  /** Fetch metadata (including the public url) for an uploaded file. */
  async getById(id: string): Promise<FileDto> {
    const res = await fetch(`${BASE_URL}/api/files/${id}`, {
      headers: authHeaders(),
    });
    if (!res.ok) {
      throw new ApiError(res.status, `API error ${res.status} on /api/files/${id}`);
    }
    return res.json() as Promise<FileDto>;
  },
};
