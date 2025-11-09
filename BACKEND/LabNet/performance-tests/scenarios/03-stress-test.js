/**
 * Escenario 3: STRESS TEST (Prueba de Estrés)
 * 
 * Objetivo: Encontrar los límites del sistema incrementando carga gradualmente
 * VUs: 10 → 500 (incremental)
 * Duración: 15 minutos
 * Patrón: Ramp-up progresivo hasta el límite
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js';

import { COMMON_HEADERS, OPTIONS_STRESS } from '../config/common.js';
import { ENDPOINTS } from '../config/endpoints.js';
import { generateCanje, generateEventoAcceso, generateUsuario } from '../utils/data-generators.js';

// Métricas personalizadas para stress
const errorRate = new Rate('errors');
const timeouts = new Rate('timeouts');
const serverErrors = new Rate('server_errors_5xx');
const clientErrors = new Rate('client_errors_4xx');

// Configuración del escenario
export const options = {
    ...OPTIONS_STRESS,
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
};

// Setup
export function setup() {
    console.log('🚀 Iniciando Escenario 3: STRESS TEST');
    console.log(`📊 Configuración: 10 → 500 VUs durante 15 minutos`);
    console.log(`⚠️  Objetivo: Encontrar límites del sistema`);
    
    const healthCheck = http.get(ENDPOINTS.HEALTH);
    if (healthCheck.status !== 200) {
        throw new Error(`❌ API no disponible: ${healthCheck.status}`);
    }
    console.log('✅ Health check exitoso');
    
    return {
        startTime: new Date().toISOString(),
        maxVUs: 500,
    };
}

// Función principal: Mix de operaciones bajo estrés extremo
export default function(data) {
    // Bajo estrés, mezclamos todas las operaciones
    
    // 1. Operación ligera: Listar espacios
    let response = http.get(ENDPOINTS.ESPACIOS_LIST, {
        headers: COMMON_HEADERS,
        tags: { name: 'ListarEspacios', scenario: 'stress' },
        timeout: '15s', // Timeout más largo para stress
    });
    
    const espaciosOk = check(response, {
        'espacios: no timeout': (r) => r.status !== 0,
        'espacios: sin error 5xx': (r) => r.status < 500,
    });
    
    if (response.status === 0) timeouts.add(1);
    if (response.status >= 500) serverErrors.add(1);
    if (response.status >= 400 && response.status < 500) clientErrors.add(1);
    errorRate.add(!espaciosOk);
    
    sleep(0.5);
    
    // 2. Operación media: Crear canje
    if (Math.random() < 0.6) {
        const canjeData = generateCanje();
        
        response = http.post(
            ENDPOINTS.CANJES_CREATE,
            JSON.stringify(canjeData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'CrearCanje', scenario: 'stress' },
                timeout: '15s',
            }
        );
        
        check(response, {
            'canje: completado': (r) => r.status !== 0,
            'canje: no error crítico': (r) => r.status !== 503 && r.status !== 504,
        });
        
        if (response.status === 0) timeouts.add(1);
        if (response.status >= 500) serverErrors.add(1);
        
        sleep(0.5);
    }
    
    // 3. Operación pesada: Registrar evento de acceso
    if (Math.random() < 0.4) {
        const eventoData = generateEventoAcceso();
        
        response = http.post(
            ENDPOINTS.EVENTOS_CREATE,
            JSON.stringify(eventoData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'RegistrarEvento', scenario: 'stress' },
                timeout: '15s',
            }
        );
        
        check(response, {
            'evento: completado': (r) => r.status !== 0,
        });
        
        if (response.status === 0) timeouts.add(1);
        if (response.status >= 500) serverErrors.add(1);
        
        sleep(1);
    }
    
    // 4. Operación compleja: Crear usuario completo
    if (Math.random() < 0.2) {
        const usuarioData = generateUsuario();
        
        response = http.post(
            ENDPOINTS.USUARIOS_CREATE,
            JSON.stringify(usuarioData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'CrearUsuario', scenario: 'stress' },
                timeout: '20s',
            }
        );
        
        check(response, {
            'usuario: completado': (r) => r.status !== 0,
        });
        
        if (response.status === 0) timeouts.add(1);
        if (response.status >= 500) serverErrors.add(1);
        
        sleep(1);
    }
    
    // 5. Health check (monitoreo durante estrés)
    if (Math.random() < 0.1) {
        response = http.get(ENDPOINTS.HEALTH, {
            tags: { name: 'HealthCheck', scenario: 'stress' },
            timeout: '5s',
        });
        
        check(response, {
            'health: disponible': (r) => r.status === 200,
        });
    }
    
    // Pausa mínima bajo estrés
    sleep(Math.random() * 1.5);
}

// Teardown
export function teardown(data) {
    console.log('');
    console.log('✅ Escenario 3: STRESS TEST completado');
    console.log(`⏱️  Inicio: ${data.startTime}`);
    console.log(`⏱️  Fin: ${new Date().toISOString()}`);
    console.log(`📊 Máximo VUs alcanzado: ${data.maxVUs}`);
    
    // Verificar recuperación del sistema
    console.log('');
    console.log('🔍 Verificando recuperación del sistema...');
    
    sleep(5); // Esperar 5 segundos
    
    const recoveryCheck = http.get(ENDPOINTS.HEALTH);
    if (recoveryCheck.status === 200) {
        console.log('✅ Sistema recuperado correctamente');
    } else {
        console.log(`⚠️  Sistema aún recuperándose: ${recoveryCheck.status}`);
    }
}

// Resumen personalizado
export function handleSummary(data) {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    
    // Analizar métricas de estrés
    const errorRateValue = data.metrics.errors?.values.rate || 0;
    const timeoutRate = data.metrics.timeouts?.values.rate || 0;
    const serverErrorRate = data.metrics.server_errors_5xx?.values.rate || 0;
    
    console.log('');
    console.log('📈 Análisis de Estrés:');
    console.log(`   ❌ Tasa de errores: ${(errorRateValue * 100).toFixed(2)}%`);
    console.log(`   ⏱️  Tasa de timeouts: ${(timeoutRate * 100).toFixed(2)}%`);
    console.log(`   🔥 Errores de servidor (5xx): ${(serverErrorRate * 100).toFixed(2)}%`);
    
    // Determinar si el sistema sobrevivió al estrés
    let verdict = '🎉 APROBADO';
    if (errorRateValue > 0.05) verdict = '⚠️  CON ALERTAS';
    if (errorRateValue > 0.10) verdict = '❌ FALLIDO';
    
    console.log(`   ${verdict}`);
    
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        [`results/stress-test-${timestamp}.json`]: JSON.stringify(data, null, 2),
        'results/stress-test-latest.json': JSON.stringify(data, null, 2),
    };
}
