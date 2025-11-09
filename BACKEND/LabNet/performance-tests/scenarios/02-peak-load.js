/**
 * Escenario 2: PEAK LOAD (Carga Pico)
 * 
 * Objetivo: Simular horas pico (entrada/salida del comedor, inicio de clases)
 * VUs: 100 usuarios concurrentes
 * Duración: 10 minutos
 * Ramp-up: 2 minutos
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js';

import { COMMON_HEADERS, OPTIONS_PEAK_LOAD, randomSleep } from '../config/common.js';
import { ENDPOINTS } from '../config/endpoints.js';
import { generateCanje, generateEventoAcceso } from '../utils/data-generators.js';

// Métricas personalizadas
const errorRate = new Rate('errors');
const canjesExitosos = new Counter('canjes_exitosos');
const canjesFallidos = new Counter('canjes_fallidos');
const eventosRegistrados = new Counter('eventos_registrados');

// Configuración del escenario
export const options = {
    ...OPTIONS_PEAK_LOAD,
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
};

// Setup
export function setup() {
    console.log('🚀 Iniciando Escenario 2: PEAK LOAD');
    console.log(`📊 Configuración: 100 VUs durante 10 minutos`);
    console.log(`🎯 Simulando: Hora pico de comedor universitario`);
    
    const healthCheck = http.get(ENDPOINTS.HEALTH);
    if (healthCheck.status !== 200) {
        throw new Error(`❌ API no disponible: ${healthCheck.status}`);
    }
    console.log('✅ Health check exitoso');
    
    return {
        startTime: new Date().toISOString(),
    };
}

// Función principal: Simula comportamiento de usuario en hora pico
export default function(data) {
    // En hora pico, la mayoría de las operaciones son canjes y consultas rápidas
    
    // 1. Consulta rápida de espacios disponibles (50% del tráfico)
    let response = http.get(ENDPOINTS.ESPACIOS_LIST, {
        headers: COMMON_HEADERS,
        tags: { name: 'ConsultarEspacios', endpoint: 'espacios', scenario: 'peak' },
    });
    
    check(response, {
        'espacios: status 200': (r) => r.status === 200,
        'espacios: tiempo < 800ms': (r) => r.timings.duration < 800,
    });
    
    sleep(0.5); // Pausa muy corta en hora pico
    
    // 2. Intentar canje (35% del tráfico - ALTA FRECUENCIA en pico)
    if (Math.random() < 0.70) { // 70% de los usuarios intentan canjear
        const canjeData = generateCanje();
        
        response = http.post(
            ENDPOINTS.CANJES_CREATE,
            JSON.stringify(canjeData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'IntentarCanje', endpoint: 'canjes', scenario: 'peak' },
                timeout: '10s',
            }
        );
        
        const canjeExitoso = check(response, {
            'canje: status 200/201': (r) => r.status === 200 || r.status === 201,
            'canje: tiempo < 800ms': (r) => r.timings.duration < 800,
            'canje: sin errores 5xx': (r) => r.status < 500,
        });
        
        if (canjeExitoso) {
            canjesExitosos.add(1);
        } else {
            canjesFallidos.add(1);
            errorRate.add(true);
        }
        
        sleep(1);
    }
    
    // 3. Registrar múltiples eventos de acceso (25% del tráfico)
    if (Math.random() < 0.50) {
        const eventoData = generateEventoAcceso();
        
        response = http.post(
            ENDPOINTS.EVENTOS_CREATE,
            JSON.stringify(eventoData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'RegistrarAcceso', endpoint: 'eventos', scenario: 'peak' },
                timeout: '10s',
            }
        );
        
        const eventoRegistrado = check(response, {
            'evento: status 200/201': (r) => r.status === 200 || r.status === 201,
            'evento: tiempo < 800ms': (r) => r.timings.duration < 800,
        });
        
        if (eventoRegistrado) {
            eventosRegistrados.add(1);
        }
        
        sleep(0.5);
    }
    
    // 4. Verificar credencial (15% del tráfico)
    if (Math.random() < 0.30) {
        response = http.get(ENDPOINTS.CREDENCIALES_LIST, {
            headers: COMMON_HEADERS,
            tags: { name: 'VerificarCredencial', endpoint: 'credenciales', scenario: 'peak' },
        });
        
        check(response, {
            'credencial: status 200': (r) => r.status === 200,
            'credencial: tiempo < 600ms': (r) => r.timings.duration < 600,
        });
        
        sleep(0.5);
    }
    
    // Pausa breve entre iteraciones (usuarios activos en hora pico)
    sleep(Math.random() * 2 + 0.5); // 0.5 - 2.5 segundos
}

// Teardown
export function teardown(data) {
    console.log('');
    console.log('✅ Escenario 2: PEAK LOAD completado');
    console.log(`⏱️  Inicio: ${data.startTime}`);
    console.log(`⏱️  Fin: ${new Date().toISOString()}`);
}

// Resumen personalizado
export function handleSummary(data) {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    
    // Calcular estadísticas adicionales
    const canjesTotal = data.metrics.canjes_exitosos?.values.count || 0;
    const canjesFails = data.metrics.canjes_fallidos?.values.count || 0;
    const canjesSuccessRate = canjesTotal > 0 
        ? ((canjesTotal / (canjesTotal + canjesFails)) * 100).toFixed(2) 
        : 0;
    
    console.log('');
    console.log('📈 Estadísticas de Canjes:');
    console.log(`   ✅ Exitosos: ${canjesTotal}`);
    console.log(`   ❌ Fallidos: ${canjesFails}`);
    console.log(`   📊 Tasa de éxito: ${canjesSuccessRate}%`);
    
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        [`results/peak-load-${timestamp}.json`]: JSON.stringify(data, null, 2),
        'results/peak-load-latest.json': JSON.stringify(data, null, 2),
    };
}
