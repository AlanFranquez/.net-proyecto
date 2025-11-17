export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || 'http://localhost:8080';

export const COGNITO_REGION =
  import.meta.env.VITE_COGNITO_REGION || 'us-east-1';

export const COGNITO_USER_POOL_ID =
  import.meta.env.VITE_COGNITO_USER_POOL_ID || '';

export const COGNITO_CLIENT_ID =
  import.meta.env.VITE_COGNITO_CLIENT_ID || '';
