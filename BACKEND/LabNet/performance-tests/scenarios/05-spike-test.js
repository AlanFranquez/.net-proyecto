/**
 * Escenario 5: SPIKE TEST (Prueba de Picos Súbitos)
 * 
 * Objetivo: Validar recuperación del sistema ante aumentos súbitos e inesperados de carga
 * VUs: 10 ↔ 200 (picos alternados)
 * Duración: 5 minutos con 3 spikes
 * Patrón: Carga base → Spike súbito → Carga base (repetir)
 */

import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend, Counter } from 'k6/metrics';
import { textSummary } from 'https://jslib.k6.io/k6-summary/0.0.2/index.js';

import { COMMON_HEADERS, OPTIONS_SPIKE } from '../config/common.js';
import { ENDPOINTS } from '../config/endpoints.js';
import { generateCanje, generateEventoAcceso } from '../utils/data-generators.js';

// Métricas personalizadas para spikes
const errorRate = new Rate('errors');
const spikeRecoveryTime = new Trend('spike_recovery_time');
const requestsDuringSpike = new Counter('requests_during_spike');
const requestsDuringBase = new Counter('requests_during_base');
const timeoutsDuringSpike = new Rate('timeouts_during_spike');

// Configuración del escenario
export const options = {
    ...OPTIONS_SPIKE,
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
};

// Setup
export function setup() {
    console.log('🚀 Iniciando Escenario 5: SPIKE TEST');
    console.log(`📊 Configuración: 3 spikes de 10 → 200 VUs`);
    console.log(`⚡ Objetivo: Validar recuperación ante picos súbitos`);
    console.log(`🎯 Simula: Eventos virales, campañas, notificaciones push masivas`);
    
    const healthCheck = http.get(ENDPOINTS.HEALTH);
    if (healthCheck.status !== 200) {
        throw new Error(`❌ API no disponible: ${healthCheck.status}`);
    }
    console.log('✅ Health check exitoso');
    
    return {
        startTime: new Date().toISOString(),
        spikes: [],
    };
}

// Función auxiliar para detectar si estamos en un spike
function isInSpike() {
    const now = Date.now();
    const elapsed = (now - __ENV.START_TIME) / 1000; // segundos desde inicio
    
    // Spikes en: 20-60s, 80-120s, 140-180s (aproximado)
    const inSpike1 = elapsed >= 20 && elapsed <= 60;
    const inSpike2 = elapsed >= 80 && elapsed <= 120;
    const inSpike3 = elapsed >= 140 && elapsed <= 180;
    
    return inSpike1 || inSpike2 || inSpike3;
}

// Función principal: Comportamiento diferenciado según fase
export default function(data) {
    const inSpike = __VU > 50; // Aproximación: más de 50 VUs = estamos en spike
    
    // Durante spike: operaciones rápidas y críticas principalmente
    if (inSpike) {
        requestsDuringSpike.add(1);
        
        // 1. Canje rápido (operación crítica durante spike)
        const canjeData = generateCanje();
        let response = http.post(
            ENDPOINTS.CANJES_CREATE,
            JSON.stringify(canjeData),
            {
                headers: COMMON_HEADERS,
                tags: { name: 'CanjeSpike', scenario: 'spike', phase: 'spike' },
                timeout: '20s', // Timeout más permisivo
            }
        );
        
        const canjeOk = check(response, {
            'spike-canje: completado': (r) => r.status !== 0,
            'spike-canje: no 503': (r) => r.status !== 503, // Service Unavailable
            'spike-canje: tiempo < 2s': (r) => r.timings.duration < 2000,
        });
        
        if (response.status === 0 || response.status === 504) {
            timeoutsDuringSpike.add(1);
        }
        
        errorRate.add(!canjeOk);
        
        sleep(0.3); // Pausa muy corta durante spike
        
        // 2. Evento de acceso (también crítico)
        if (Math.random() < 0.5) {
            const eventoData = generateEventoAcceso();
            
            response = http.post(
                ENDPOINTS.EVENTOS_CREATE,
                JSON.stringify(eventoData),
                {
                    headers: COMMON_HEADERS,
                    tags: { name: 'EventoSpike', scenario: 'spike', phase: 'spike' },
                    timeout: '20s',
                }
            );
            
            check(response, {
                'spike-evento: completado': (r) => r.status !== 0,
            });
            
            sleep(0.3);
        }
        
        // 3. Consulta rápida de espacios
        response = http.get(ENDPOINTS.ESPACIOS_LIST, {
            headers: COMMON_HEADERS,
            tags: { name: 'EspaciosSpike', scenario: 'spike', phase: 'spike' },
            timeout: '10s',
        });
        
        check(response, {
            'spike-espacios: disponible': (r) => r.status === 200 || r.status === 503,
        });
        
        sleep(Math.random() * 0.5); // 0-0.5s durante spike
        
    } else {
        // Durante carga base: operaciones normales y variadas
        requestsDuringBase.add(1);
        
        // 1. Listar espacios (normal)
        let response = http.get(ENDPOINTS.ESPACIOS_LIST, {
            headers: COMMON_HEADERS,
            tags: { name: 'EspaciosBase', scenario: 'spike', phase: 'base' },
        });
        
        check(response, {
            'base-espacios: status 200': (r) => r.status === 200,
            'base-espacios: tiempo < 500ms': (r) => r.timings.duration < 500,
        });
        
        sleep(1);
        
        // 2. Operaciones variadas
        if (Math.random() < 0.3) {
            const canjeData = generateCanje();
            
            response = http.post(
                ENDPOINTS.CANJES_CREATE,
                JSON.stringify(canjeData),
                {
                    headers: COMMON_HEADERS,
                    tags: { name: 'CanjeBase', scenario: 'spike', phase: 'base' },
                }
            );
            
            check(response, {
                'base-canje: completado': (r) => r.status === 200 || r.status === 201 || r.status === 400,
            });
            
            sleep(2);
        }
        
        // 3. Credenciales
        if (Math.random() < 0.2) {
            response = http.get(ENDPOINTS.CREDENCIALES_LIST, {
                headers: COMMON_HEADERS,
                tags: { name: 'CredencialesBase', scenario: 'spike', phase: 'base' },
            });
            
            check(response, {
                'base-credenciales: status 200': (r) => r.status === 200,
            });
        }
        
        // Pausa normal durante carga base
        sleep(2 + Math.random() * 2); // 2-4s
    }
}

// Teardown
export function teardown(data) {
    console.log('');
    console.log('✅ Escenario 5: SPIKE TEST completado');
    console.log(`⏱️  Inicio: ${data.startTime}`);
    console.log(`⏱️  Fin: ${new Date().toISOString()}`);
    
    // Verificar recuperación del sistema después del último spike
    console.log('');
    console.log('🔍 Verificando recuperación post-spikes...');
    
    sleep(5); // Esperar 5 segundos
    
    const recoveryCheck = http.get(ENDPOINTS.HEALTH);
    const recoveryTime = recoveryCheck.timings.duration;
    
    if (recoveryCheck.status === 200 && recoveryTime < 200) {
        console.log(`✅ Sistema recuperado en ${recoveryTime.toFixed(2)}ms`);
    } else if (recoveryCheck.status === 200) {
        console.log(`⚠️  Sistema recuperado pero lento: ${recoveryTime.toFixed(2)}ms`);
    } else {
        console.log(`❌ Sistema aún inestable: ${recoveryCheck.status}`);
    }
}

// Resumen personalizado
export function handleSummary(data) {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    
    // Analizar comportamiento durante spikes vs base
    const requestsSpike = data.metrics.requests_during_spike?.values.count || 0;
    const requestsBase = data.metrics.requests_during_base?.values.count || 0;
    const timeoutsSpike = data.metrics.timeouts_during_spike?.values.rate || 0;
    const errorRateValue = data.metrics.errors?.values.rate || 0;
    
    const totalRequests = requestsSpike + requestsBase;
    const spikePercentage = totalRequests > 0 ? (requestsSpike / totalRequests * 100) : 0;
    
    console.log('');
    console.log('📈 Análisis de Spikes:');
    console.log(`   📊 Requests durante spikes: ${requestsSpike} (${spikePercentage.toFixed(1)}%)`);
    console.log(`   📊 Requests durante base: ${requestsBase}`);
    console.log(`   ⏱️  Timeouts durante spikes: ${(timeoutsSpike * 100).toFixed(2)}%`);
    console.log(`   ❌ Tasa de errores general: ${(errorRateValue * 100).toFixed(2)}%`);
    
    // Evaluar resiliencia ante spikes
    let resilienceVerdict = '✅ RESILIENTE';
    if (timeoutsSpike > 0.05 || errorRateValue > 0.05) {
        resilienceVerdict = '⚠️  RECUPERACIÓN LENTA';
    }
    if (timeoutsSpike > 0.20 || errorRateValue > 0.15) {
        resilienceVerdict = '❌ POCO RESILIENTE';
    }
    
    console.log(`   ${resilienceVerdict}`);
    
    if (timeoutsSpike > 0.05) {
        console.log('');
        console.log('💡 RECOMENDACIONES:');
        console.log('   - Implementar rate limiting');
        console.log('   - Configurar auto-scaling horizontal');
        console.log('   - Implementar circuit breakers');
        console.log('   - Aumentar pool de conexiones de DB');
        console.log('   - Considerar caching para endpoints frecuentes');
    }
    
    return {
        'stdout': textSummary(data, { indent: ' ', enableColors: true }),
        [`results/spike-test-${timestamp}.json`]: JSON.stringify(data, null, 2),
        'results/spike-test-latest.json': JSON.stringify(data, null, 2),
    };
}
