import { API_BASE_URL } from '../config/env';

function getAuthToken() {
  return localStorage.getItem('authToken');
}

async function request(path, options = {}) {
  const token = getAuthToken();

  const headers = {
    'Content-Type': 'application/json',
    ...(options.headers || {}),
  };

  if (token) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    // Optional: throw a more detailed error
    const text = await res.text().catch(() => '');
    throw new Error(`API error ${res.status}: ${text || res.statusText}`);
  }

  // If no content, just return null
  if (res.status === 204) return null;

  return res.json();
}

// Convenience helpers
export const apiClient = {
  get: (path) => request(path, { method: 'GET' }),
  post: (path, body) =>
    request(path, {
      method: 'POST',
      body: JSON.stringify(body),
    }),
  put: (path, body) =>
    request(path, {
      method: 'PUT',
      body: JSON.stringify(body),
    }),
  del: (path) => request(path, { method: 'DELETE' }),
};
