/**
 * Configuración común para todas las pruebas de k6
 */

import { sleep } from 'k6';

// URL base de la API según entorno
// 
// DETECCIÓN AUTOMÁTICA:
// 1. Si existe BASE_URL en variable de entorno → usa ese valor
// 2. Si no, usa 'http://localhost:8080' por defecto
// 
// Los scripts de PowerShell (run-all.ps1, run-aws.ps1) detectan automáticamente
// el backend desde Terraform y setean BASE_URL antes de ejecutar k6.
//
// Uso manual:
//   k6 run -e BASE_URL=http://mi-alb-123.us-east-1.elb.amazonaws.com scenarios/01-baseline.js
//   k6 run scenarios/01-baseline.js  (usa localhost:8080)
export const BASE_URL = __ENV.BASE_URL || 'http://localhost:8080';

// Configuración de headers comunes
export const COMMON_HEADERS = {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
};

// Thresholds comunes (SLOs)
export const DEFAULT_THRESHOLDS = {
    // Latencia: p95 debe ser menor a 300ms, p99 menor a 500ms
    'http_req_duration': ['p(95)<300', 'p(99)<500'],
    
    // Tasa de éxito: al menos 99.5% de requests exitosos
    'http_req_failed': ['rate<0.005'],
    
    // Checks: al menos 99% deben pasar
    'checks': ['rate>0.99'],
    
    // Tiempo bloqueado (DNS, TCP, TLS): p95 < 50ms
    'http_req_blocked': ['p(95)<50'],
    
    // Tiempo de conexión: p95 < 100ms
    'http_req_connecting': ['p(95)<100'],
};

// Thresholds estrictos para endpoints críticos
export const CRITICAL_THRESHOLDS = {
    'http_req_duration{endpoint:canjes}': ['p(95)<200', 'p(99)<400'],
    'http_req_duration{endpoint:eventos}': ['p(95)<200', 'p(99)<400'],
    'http_req_failed{endpoint:canjes}': ['rate<0.001'],
    'http_req_failed{endpoint:eventos}': ['rate<0.001'],
};

// Configuración de opciones para diferentes escenarios
export const OPTIONS_BASELINE = {
    stages: [
        { duration: '30s', target: 10 },  // Ramp-up a 10 usuarios
        { duration: '5m', target: 10 },   // Mantener 10 usuarios
        { duration: '30s', target: 0 },   // Ramp-down
    ],
    thresholds: DEFAULT_THRESHOLDS,
};

export const OPTIONS_PEAK_LOAD = {
    stages: [
        { duration: '2m', target: 100 },  // Ramp-up a 100 usuarios
        { duration: '10m', target: 100 }, // Mantener 100 usuarios
        { duration: '1m', target: 0 },    // Ramp-down
    ],
    thresholds: {
        ...DEFAULT_THRESHOLDS,
        'http_req_duration': ['p(95)<500', 'p(99)<800'], // Más permisivo bajo carga
    },
};

export const OPTIONS_STRESS = {
    stages: [
        { duration: '2m', target: 50 },   // Ramp-up suave
        { duration: '3m', target: 200 },  // Incremento medio
        { duration: '5m', target: 500 },  // Push al límite
        { duration: '3m', target: 500 },  // Mantener presión
        { duration: '2m', target: 0 },    // Ramp-down
    ],
    thresholds: {
        'http_req_failed': ['rate<0.01'], // Permitir hasta 1% de errores bajo estrés
    },
};

export const OPTIONS_SOAK = {
    stages: [
        { duration: '2m', target: 50 },   // Ramp-up
        { duration: '1h', target: 50 },   // Mantener 1 hora
        { duration: '2m', target: 0 },    // Ramp-down
    ],
    thresholds: DEFAULT_THRESHOLDS,
};

export const OPTIONS_SPIKE = {
    stages: [
        { duration: '10s', target: 10 },   // Carga base
        { duration: '10s', target: 200 },  // Spike súbito
        { duration: '30s', target: 200 },  // Mantener spike
        { duration: '10s', target: 10 },   // Volver a base
        { duration: '10s', target: 200 },  // Segundo spike
        { duration: '30s', target: 200 },  
        { duration: '10s', target: 10 },   
        { duration: '10s', target: 200 },  // Tercer spike
        { duration: '30s', target: 200 },  
        { duration: '10s', target: 0 },    // Final
    ],
    thresholds: {
        'http_req_duration': ['p(95)<800', 'p(99)<1200'], // Más permisivo en spikes
        'http_req_failed': ['rate<0.02'],
    },
};

// Configuración de sleep entre requests
export const SLEEP_MIN = 1;
export const SLEEP_MAX = 3;

// Configuración de timeouts
export const REQUEST_TIMEOUT = '30s';

// Tags personalizados para métricas
export function addCustomTags(response, endpoint) {
    response.request.tags = response.request.tags || {};
    response.request.tags.endpoint = endpoint;
    response.request.tags.status = response.status;
    return response;
}

// Función para generar summarios personalizados
export function handleSummary(data) {
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        'summary.json': JSON.stringify(data),
    };
}

// Sleep aleatorio entre min y max segundos
export function randomSleep(min = SLEEP_MIN, max = SLEEP_MAX) {
    const sleepTime = Math.random() * (max - min) + min;
    sleep(sleepTime);
}
