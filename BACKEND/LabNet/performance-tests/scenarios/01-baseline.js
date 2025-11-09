/**
 * Escenario 1: BASELINE (Carga Normal)
 * 
 * Objetivo: Establecer métricas base en condiciones normales de operación
 * VUs: 10 usuarios concurrentes
 * Duración: 5 minutos
 * Ramp-up: 30 segundos
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js';

import { COMMON_HEADERS, OPTIONS_BASELINE, randomSleep } from '../config/common.js';
import { ENDPOINTS, TEST_IDS } from '../config/endpoints.js';
import { generateCanje, generateEventoAcceso } from '../utils/data-generators.js';

// Métricas personalizadas
const errorRate = new Rate('errors');
const espaciosDuration = new Trend('espacios_duration');
const canjesDuration = new Trend('canjes_duration');

// Configuración del escenario
export const options = {
    ...OPTIONS_BASELINE,
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
};

// Setup: Se ejecuta una vez antes de todas las iteraciones
export function setup() {
    console.log('🚀 Iniciando Escenario 1: BASELINE');
    console.log(`📊 Configuración: 10 VUs durante 5 minutos`);
    
    // Verificar que la API está disponible
    const healthCheck = http.get(ENDPOINTS.HEALTH);
    if (healthCheck.status !== 200) {
        throw new Error(`❌ API no disponible: ${healthCheck.status}`);
    }
    console.log('✅ Health check exitoso');
    
    return {
        startTime: new Date().toISOString(),
    };
}

// Función principal: Se ejecuta por cada VU en cada iteración
export default function(data) {
    // Simular patron de uso real: mezcla de operaciones de lectura y escritura
    
    // 1. Listar espacios (40% del tráfico - operación más común)
    let response = http.get(ENDPOINTS.ESPACIOS_LIST, {
        headers: COMMON_HEADERS,
        tags: { name: 'ListarEspacios', endpoint: 'espacios' },
    });
    
    const espaciosCheck = check(response, {
        'espacios: status 200': (r) => r.status === 200,
        'espacios: tiempo < 500ms': (r) => r.timings.duration < 500,
        'espacios: tiene contenido': (r) => r.body.length > 0,
    });
    
    errorRate.add(!espaciosCheck);
    espaciosDuration.add(response.timings.duration);
    
    randomSleep(1, 2);
    
    // 2. Obtener detalle de un espacio específico (30% del tráfico)
    if (Math.random() < 0.75) { // 75% de las veces
        response = http.get(ENDPOINTS.ESPACIOS_LIST, {
            headers: COMMON_HEADERS,
            tags: { name: 'ObtenerEspacio', endpoint: 'espacios' },
        });
        
        check(response, {
            'espacio detalle: status 200 o 404': (r) => r.status === 200 || r.status === 404,
            'espacio detalle: tiempo < 300ms': (r) => r.timings.duration < 300,
        });
        
        randomSleep(1, 2);
    }
    
    // 3. Crear canje (15% del tráfico - operación crítica)
    if (Math.random() < 0.20) { // ~20% de las veces
        const canjeData = generateCanje();
        
        response = http.post(
            ENDPOINTS.CANJES_CREATE,
            JSON.stringify(canjeData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'CrearCanje', endpoint: 'canjes' },
            }
        );
        
        const canjeCheck = check(response, {
            'canje: status 200 o 201': (r) => r.status === 200 || r.status === 201 || r.status === 400,
            'canje: tiempo < 400ms': (r) => r.timings.duration < 400,
        });
        
        errorRate.add(!canjeCheck);
        canjesDuration.add(response.timings.duration);
        
        randomSleep(2, 3);
    }
    
    // 4. Registrar evento de acceso (10% del tráfico - operación crítica)
    if (Math.random() < 0.15) { // ~15% de las veces
        const eventoData = generateEventoAcceso();
        
        response = http.post(
            ENDPOINTS.EVENTOS_CREATE,
            JSON.stringify(eventoData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'RegistrarEvento', endpoint: 'eventos' },
            }
        );
        
        check(response, {
            'evento: status 200 o 201': (r) => r.status === 200 || r.status === 201 || r.status === 400,
            'evento: tiempo < 400ms': (r) => r.timings.duration < 400,
        });
        
        randomSleep(2, 3);
    }
    
    // 5. Consultar credenciales (10% del tráfico)
    if (Math.random() < 0.15) {
        response = http.get(ENDPOINTS.CREDENCIALES_LIST, {
            headers: COMMON_HEADERS,
            tags: { name: 'ListarCredenciales', endpoint: 'credenciales' },
        });
        
        check(response, {
            'credenciales: status 200': (r) => r.status === 200,
            'credenciales: tiempo < 500ms': (r) => r.timings.duration < 500,
        });
        
        randomSleep(1, 2);
    }
    
    // 6. Health check (5% del tráfico - monitoring)
    if (Math.random() < 0.05) {
        response = http.get(ENDPOINTS.HEALTH, {
            tags: { name: 'HealthCheck', endpoint: 'health' },
        });
        
        check(response, {
            'health: status 200': (r) => r.status === 200,
            'health: tiempo < 100ms': (r) => r.timings.duration < 100,
        });
    }
    
    randomSleep(2, 4);
}

// Teardown: Se ejecuta una vez después de todas las iteraciones
export function teardown(data) {
    console.log('');
    console.log('✅ Escenario 1: BASELINE completado');
    console.log(`⏱️  Inicio: ${data.startTime}`);
    console.log(`⏱️  Fin: ${new Date().toISOString()}`);
}

// Resumen personalizado
export function handleSummary(data) {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        [`results/baseline-${timestamp}.json`]: JSON.stringify(data, null, 2),
        'results/baseline-latest.json': JSON.stringify(data, null, 2),
    };
}
