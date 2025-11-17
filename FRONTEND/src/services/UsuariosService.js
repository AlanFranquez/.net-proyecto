// src/services/UsuariosService.js
import { apiClient } from './apiClient';

export const UsuariosService = {
  getAll: () => apiClient.get('/api/usuarios'),

  getById: (id) => apiClient.get(`/api/usuarios/${id}`),

  create: (usuario) =>
    apiClient.post('/api/usuarios', {
      nombre: usuario.nombre,
      apellido: usuario.apellido,
      email: usuario.email,
      documento: usuario.documento,
      password: usuario.password,
      rolesIDs: usuario.rolesIDs ?? null,
    }),

  update: (id, usuario) =>
    apiClient.put(`/api/usuarios/${id}`, {
      nombre: usuario.nombre,
      apellido: usuario.apellido,
      email: usuario.email,
      documento: usuario.documento,
      rolesIDs: usuario.rolesIDs ?? null,
    }),

  delete: (id) => apiClient.del(`/api/usuarios/${id}`),
};
