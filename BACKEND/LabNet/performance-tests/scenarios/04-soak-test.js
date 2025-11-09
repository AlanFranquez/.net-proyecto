/**
 * Escenario 4: SOAK TEST (Prueba de Resistencia/Durabilidad)
 * 
 * Objetivo: Validar estabilidad a largo plazo (memory leaks, degradación de rendimiento)
 * VUs: 50 usuarios concurrentes constantes
 * Duración: 1 hora
 * Ramp-up: 2 minutos
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js';

import { COMMON_HEADERS, OPTIONS_SOAK, randomSleep } from '../config/common.js';
import { ENDPOINTS } from '../config/endpoints.js';
import { generateCanje, generateEventoAcceso } from '../utils/data-generators.js';

// Métricas personalizadas para monitorear degradación
const errorRate = new Rate('errors');
const degradationTrend = new Trend('response_time_trend');
const memoryLeakIndicator = new Rate('slow_requests_rate'); // >1s
const iterationCounter = new Counter('total_iterations');

// Configuración del escenario
export const options = {
    ...OPTIONS_SOAK,
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
};

// Setup
export function setup() {
    console.log('🚀 Iniciando Escenario 4: SOAK TEST (Prueba de Resistencia)');
    console.log(`📊 Configuración: 50 VUs durante 1 hora`);
    console.log(`🎯 Objetivo: Detectar memory leaks y degradación`);
    console.log(`⏱️  Duración estimada: ~62 minutos`);
    
    const healthCheck = http.get(ENDPOINTS.HEALTH);
    if (healthCheck.status !== 200) {
        throw new Error(`❌ API no disponible: ${healthCheck.status}`);
    }
    console.log('✅ Health check exitoso');
    
    return {
        startTime: new Date().toISOString(),
        checkpointInterval: 300, // Checkpoint cada 5 minutos
    };
}

// Función principal: Simula uso normal sostenido durante largo tiempo
export default function(data) {
    iterationCounter.add(1);
    
    // Patrón de uso normal y variado para detectar degradación
    
    // 1. Listar espacios (operación común)
    let response = http.get(ENDPOINTS.ESPACIOS_LIST, {
        headers: COMMON_HEADERS,
        tags: { name: 'ListarEspacios', scenario: 'soak' },
    });
    
    const duration = response.timings.duration;
    degradationTrend.add(duration);
    
    const espaciosOk = check(response, {
        'espacios: status 200': (r) => r.status === 200,
        'espacios: tiempo < 500ms': (r) => r.timings.duration < 500,
        'espacios: sin degradación': (r) => r.timings.duration < 1000,
    });
    
    if (duration > 1000) memoryLeakIndicator.add(1);
    errorRate.add(!espaciosOk);
    
    randomSleep(2, 4);
    
    // 2. Operaciones de escritura (canjes)
    if (Math.random() < 0.3) {
        const canjeData = generateCanje();
        
        response = http.post(
            ENDPOINTS.CANJES_CREATE,
            JSON.stringify(canjeData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'CrearCanje', scenario: 'soak' },
            }
        );
        
        const canjeDuration = response.timings.duration;
        degradationTrend.add(canjeDuration);
        
        check(response, {
            'canje: completado': (r) => r.status === 200 || r.status === 201 || r.status === 400,
            'canje: sin degradación': (r) => r.timings.duration < 1500,
        });
        
        if (canjeDuration > 1000) memoryLeakIndicator.add(1);
        
        randomSleep(2, 3);
    }
    
    // 3. Registrar eventos de acceso
    if (Math.random() < 0.25) {
        const eventoData = generateEventoAcceso();
        
        response = http.post(
            ENDPOINTS.EVENTOS_CREATE,
            JSON.stringify(eventoData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'RegistrarEvento', scenario: 'soak' },
            }
        );
        
        check(response, {
            'evento: completado': (r) => r.status === 200 || r.status === 201 || r.status === 400,
            'evento: sin degradación': (r) => r.timings.duration < 1500,
        });
        
        randomSleep(2, 3);
    }
    
    // 4. Consultas de credenciales
    if (Math.random() < 0.2) {
        response = http.get(ENDPOINTS.CREDENCIALES_LIST, {
            headers: COMMON_HEADERS,
            tags: { name: 'ConsultarCredenciales', scenario: 'soak' },
        });
        
        check(response, {
            'credenciales: status 200': (r) => r.status === 200,
            'credenciales: sin degradación': (r) => r.timings.duration < 800,
        });
        
        randomSleep(1, 2);
    }
    
    // 5. Consultas de usuarios
    if (Math.random() < 0.15) {
        response = http.get(ENDPOINTS.USUARIOS_LIST, {
            headers: COMMON_HEADERS,
            tags: { name: 'ConsultarUsuarios', scenario: 'soak' },
        });
        
        check(response, {
            'usuarios: status 200': (r) => r.status === 200,
        });
        
        randomSleep(1, 2);
    }
    
    // 6. Health checks periódicos para monitoreo
    if (Math.random() < 0.05) {
        response = http.get(ENDPOINTS.HEALTH, {
            tags: { name: 'HealthCheck', scenario: 'soak' },
        });
        
        check(response, {
            'health: sistema saludable': (r) => r.status === 200,
            'health: respuesta rápida': (r) => r.timings.duration < 200,
        });
    }
    
    // Pausa normal entre operaciones
    randomSleep(3, 6);
}

// Teardown
export function teardown(data) {
    console.log('');
    console.log('✅ Escenario 4: SOAK TEST completado');
    console.log(`⏱️  Inicio: ${data.startTime}`);
    console.log(`⏱️  Fin: ${new Date().toISOString()}`);
    
    // Calcular duración total
    const startTime = new Date(data.startTime);
    const endTime = new Date();
    const durationMinutes = Math.round((endTime - startTime) / 1000 / 60);
    console.log(`⏱️  Duración total: ${durationMinutes} minutos`);
}

// Resumen personalizado con análisis de degradación
export function handleSummary(data) {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    
    // Analizar degradación de rendimiento
    const responseTimes = data.metrics.response_time_trend || {};
    const avgResponseTime = responseTimes.values?.avg || 0;
    const maxResponseTime = responseTimes.values?.max || 0;
    const p99ResponseTime = responseTimes.values?.['p(99)'] || 0;
    
    const slowRequestsRate = data.metrics.slow_requests_rate?.values.rate || 0;
    const errorRateValue = data.metrics.errors?.values.rate || 0;
    
    console.log('');
    console.log('📈 Análisis de Degradación:');
    console.log(`   📊 Tiempo de respuesta promedio: ${avgResponseTime.toFixed(2)}ms`);
    console.log(`   📊 Tiempo de respuesta máximo: ${maxResponseTime.toFixed(2)}ms`);
    console.log(`   📊 P99: ${p99ResponseTime.toFixed(2)}ms`);
    console.log(`   🐌 Requests lentos (>1s): ${(slowRequestsRate * 100).toFixed(2)}%`);
    console.log(`   ❌ Tasa de errores: ${(errorRateValue * 100).toFixed(2)}%`);
    
    // Detectar posibles memory leaks o degradación
    let healthVerdict = '✅ SALUDABLE';
    if (slowRequestsRate > 0.05) healthVerdict = '⚠️  DEGRADACIÓN DETECTADA';
    if (slowRequestsRate > 0.10 || errorRateValue > 0.02) healthVerdict = '❌ DEGRADACIÓN CRÍTICA';
    
    console.log(`   ${healthVerdict}`);
    
    if (slowRequestsRate > 0.05) {
        console.log('');
        console.log('⚠️  RECOMENDACIONES:');
        console.log('   - Revisar uso de memoria de la API');
        console.log('   - Verificar conexiones de base de datos no cerradas');
        console.log('   - Analizar logs de Serilog/Seq para errores');
        console.log('   - Revisar métricas de Prometheus/Grafana');
    }
    
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        [`results/soak-test-${timestamp}.json`]: JSON.stringify(data, null, 2),
        'results/soak-test-latest.json': JSON.stringify(data, null, 2),
    };
}
