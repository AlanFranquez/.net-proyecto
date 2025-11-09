/**
 * Definición de endpoints de la API para pruebas de carga
 */

import { BASE_URL } from './common.js';

// Endpoints de la API
export const ENDPOINTS = {
    // Health
    HEALTH: `${BASE_URL}/health`,
    
    // Espacios
    ESPACIOS_LIST: `${BASE_URL}/api/espacios`,
    ESPACIOS_GET: (id) => `${BASE_URL}/api/espacios/${id}`,
    ESPACIOS_CREATE: `${BASE_URL}/api/espacios`,
    ESPACIOS_UPDATE: (id) => `${BASE_URL}/api/espacios/${id}`,
    ESPACIOS_DELETE: (id) => `${BASE_URL}/api/espacios/${id}`,
    
    // Reglas de Acceso
    REGLAS_LIST: `${BASE_URL}/api/reglas`,
    REGLAS_GET: (id) => `${BASE_URL}/api/reglas/${id}`,
    REGLAS_CREATE: `${BASE_URL}/api/reglas`,
    
    // Canjes (CRÍTICO)
    CANJES_LIST: `${BASE_URL}/api/canjes`,
    CANJES_CREATE: `${BASE_URL}/api/canjes`,
    CANJES_GET: (id) => `${BASE_URL}/api/canjes/${id}`,
    
    // Eventos de Acceso (CRÍTICO)
    EVENTOS_LIST: `${BASE_URL}/api/eventos`,
    EVENTOS_CREATE: `${BASE_URL}/api/eventos`,
    EVENTOS_GET: (id) => `${BASE_URL}/api/eventos/${id}`,
    
    // Credenciales
    CREDENCIALES_LIST: `${BASE_URL}/api/credenciales`,
    CREDENCIALES_GET: (id) => `${BASE_URL}/api/credenciales/${id}`,
    CREDENCIALES_CREATE: `${BASE_URL}/api/credenciales`,
    
    // Usuarios
    USUARIOS_LIST: `${BASE_URL}/api/usuarios`,
    USUARIOS_GET: (id) => `${BASE_URL}/api/usuarios/${id}`,
    USUARIOS_CREATE: `${BASE_URL}/api/usuarios`,
    
    // Beneficios
    BENEFICIOS_LIST: `${BASE_URL}/api/beneficios`,
    BENEFICIOS_GET: (id) => `${BASE_URL}/api/beneficios/${id}`,
    
    // Dispositivos
    DISPOSITIVOS_LIST: `${BASE_URL}/api/dispositivos`,
    DISPOSITIVOS_GET: (id) => `${BASE_URL}/api/dispositivos/${id}`,
    
    // Roles
    ROLES_LIST: `${BASE_URL}/api/roles`,
    ROLES_GET: (id) => `${BASE_URL}/api/roles/${id}`,
    
    // Notificaciones
    NOTIFICACIONES_LIST: `${BASE_URL}/api/notificaciones`,
    NOTIFICACIONES_GET: (id) => `${BASE_URL}/api/notificaciones/${id}`,
    
    // Sincronizaciones
    SINCRONIZACIONES_LIST: `${BASE_URL}/api/sincronizaciones`,
    SINCRONIZACIONES_GET: (id) => `${BASE_URL}/api/sincronizaciones/${id}`,
};

// IDs de ejemplo para pruebas (ajustar según datos seedeados)
export const TEST_IDS = {
    ESPACIO_ID: '00000000-0000-0000-0000-000000000001', // Ajustar con ID real
    USUARIO_ID: '00000000-0000-0000-0000-000000000001',
    CREDENCIAL_ID: '00000000-0000-0000-0000-000000000001',
    REGLA_ID: '00000000-0000-0000-0000-000000000001',
    ROL_ID: '00000000-0000-0000-0000-000000000001',
};

// Distribución de requests por tipo (porcentajes)
export const REQUEST_DISTRIBUTION = {
    ESPACIOS_LIST: 0.30,      // 30% - consulta frecuente
    ESPACIOS_GET: 0.20,       // 20% - detalle
    CANJES_CREATE: 0.15,      // 15% - crítico
    EVENTOS_CREATE: 0.10,     // 10% - crítico
    CREDENCIALES_GET: 0.10,   // 10% - validación
    USUARIOS_LIST: 0.05,      // 5%
    BENEFICIOS_LIST: 0.05,    // 5%
    HEALTH: 0.05,             // 5% - monitoring
};

// Función para seleccionar endpoint aleatorio según distribución
export function selectRandomEndpoint() {
    const rand = Math.random();
    let cumulative = 0;
    
    for (const [endpoint, probability] of Object.entries(REQUEST_DISTRIBUTION)) {
        cumulative += probability;
        if (rand <= cumulative) {
            return endpoint;
        }
    }
    
    return 'ESPACIOS_LIST'; // Fallback
}
