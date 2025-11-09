#!/bin/bash
# Script de automatización para ejecutar todas las pruebas de rendimiento (Linux/macOS)
# Uso: ./run-all.sh [--skip-baseline] [--skip-peak-load] [--skip-stress] [--skip-soak] [--skip-spike] [--quick]

set -e

# Colores
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;90m'
NC='\033[0m' # No Color

# Parsear argumentos
SKIP_BASELINE=false
SKIP_PEAK_LOAD=false
SKIP_STRESS=false
SKIP_SOAK=false
SKIP_SPIKE=false
QUICK=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --skip-baseline) SKIP_BASELINE=true; shift ;;
        --skip-peak-load) SKIP_PEAK_LOAD=true; shift ;;
        --skip-stress) SKIP_STRESS=true; shift ;;
        --skip-soak) SKIP_SOAK=true; shift ;;
        --skip-spike) SKIP_SPIKE=true; shift ;;
        --quick) QUICK=true; shift ;;
        *) echo "Opción desconocida: $1"; exit 1 ;;
    esac
done

# Banner
echo -e "${CYAN}"
echo "🚀 ==============================================="
echo "   K6 PERFORMANCE TEST SUITE - LabNet API"
echo "==============================================="
echo -e "${NC}"

# Verificar que k6 está instalado
if ! command -v k6 &> /dev/null; then
    echo -e "${RED}❌ ERROR: k6 no está instalado${NC}"
    echo -e "${YELLOW}   Instalar con: brew install k6 (macOS)${NC}"
    echo -e "${YELLOW}   O desde: https://k6.io/docs/get-started/installation/${NC}"
    exit 1
fi

K6_VERSION=$(k6 version | head -n 1)
echo -e "${GREEN}✅ k6 encontrado: $K6_VERSION${NC}"

# Verificar que la API está disponible
API_URL=${BASE_URL:-"http://localhost:8080"}
echo -e "${CYAN}\n🔍 Verificando disponibilidad de la API en $API_URL...${NC}"

if curl -s -o /dev/null -w "%{http_code}" "$API_URL/health" | grep -q "200"; then
    echo -e "${GREEN}✅ API disponible y saludable${NC}"
else
    echo -e "${RED}❌ ERROR: No se puede conectar a la API${NC}"
    echo -e "${YELLOW}   Asegúrate de que la API esté ejecutándose en $API_URL${NC}"
    echo -e "${YELLOW}   Ejecutar: ./scripts/up.ps1 -Seed${NC}"
    exit 1
fi

# Crear directorio de resultados
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="$SCRIPT_DIR/results"
mkdir -p "$RESULTS_DIR"

# Timestamp
TIMESTAMP=$(date +"%Y-%m-%d_%H-%M-%S")
SUMMARY_FILE="$RESULTS_DIR/test-suite-summary-$TIMESTAMP.txt"

# Inicializar resumen
{
    echo "============================================================"
    echo "  K6 PERFORMANCE TEST SUITE - RESUMEN"
    echo "  Fecha: $(date '+%Y-%m-%d %H:%M:%S')"
    echo "  API: $API_URL"
    echo "============================================================"
    echo ""
} > "$SUMMARY_FILE"

# Contadores
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Función para ejecutar un escenario
run_scenario() {
    local NAME=$1
    local SCRIPT_PATH=$2
    local OUTPUT_FILE=$3
    local DESCRIPTION=$4
    
    echo -e "${CYAN}"
    echo "============================================================"
    echo "  EJECUTANDO: $NAME"
    echo "  $DESCRIPTION"
    echo "============================================================"
    echo -e "${NC}"
    
    ((TOTAL_TESTS++))
    START_TIME=$(date +%s)
    
    if k6 run --out "json=$OUTPUT_FILE" "$SCRIPT_PATH"; then
        END_TIME=$(date +%s)
        DURATION=$((END_TIME - START_TIME))
        echo -e "${GREEN}\n✅ $NAME COMPLETADO (${DURATION}s)${NC}"
        ((PASSED_TESTS++))
        echo "✅ $NAME - APROBADO (${DURATION}s)" >> "$SUMMARY_FILE"
    else
        END_TIME=$(date +%s)
        DURATION=$((END_TIME - START_TIME))
        echo -e "${RED}\n❌ $NAME FALLIDO (${DURATION}s)${NC}"
        ((FAILED_TESTS++))
        echo "❌ $NAME - FALLIDO (${DURATION}s)" >> "$SUMMARY_FILE"
    fi
    
    echo ""
}

# Ejecutar escenarios

# Escenario 1: Baseline
if [ "$SKIP_BASELINE" = false ]; then
    run_scenario \
        "Escenario 1: BASELINE" \
        "scenarios/01-baseline.js" \
        "$RESULTS_DIR/baseline-$TIMESTAMP.json" \
        "Carga normal - 10 VUs durante 5 minutos"
else
    echo -e "${YELLOW}\n⏭️  Escenario 1: BASELINE omitido${NC}"
fi

# Escenario 2: Peak Load
if [ "$SKIP_PEAK_LOAD" = false ]; then
    run_scenario \
        "Escenario 2: PEAK LOAD" \
        "scenarios/02-peak-load.js" \
        "$RESULTS_DIR/peak-load-$TIMESTAMP.json" \
        "Carga pico - 100 VUs durante 10 minutos"
else
    echo -e "${YELLOW}\n⏭️  Escenario 2: PEAK LOAD omitido${NC}"
fi

# Modo Quick
if [ "$QUICK" = true ]; then
    echo -e "${YELLOW}\n⚡ Modo Quick activado - omitiendo pruebas largas${NC}"
    SKIP_STRESS=true
    SKIP_SOAK=true
    SKIP_SPIKE=true
fi

# Escenario 3: Stress Test
if [ "$SKIP_STRESS" = false ]; then
    run_scenario \
        "Escenario 3: STRESS TEST" \
        "scenarios/03-stress-test.js" \
        "$RESULTS_DIR/stress-test-$TIMESTAMP.json" \
        "Prueba de estrés - 10→500 VUs durante 15 minutos"
else
    echo -e "${YELLOW}\n⏭️  Escenario 3: STRESS TEST omitido${NC}"
fi

# Escenario 4: Soak Test
if [ "$SKIP_SOAK" = false ]; then
    echo -e "${YELLOW}\n⚠️  ADVERTENCIA: El Soak Test durará aproximadamente 62 minutos${NC}"
    read -p "¿Continuar? (s/N): " CONTINUE
    if [ "$CONTINUE" = "s" ] || [ "$CONTINUE" = "S" ]; then
        run_scenario \
            "Escenario 4: SOAK TEST" \
            "scenarios/04-soak-test.js" \
            "$RESULTS_DIR/soak-test-$TIMESTAMP.json" \
            "Prueba de resistencia - 50 VUs durante 1 hora"
    else
        echo -e "${YELLOW}⏭️  Escenario 4: SOAK TEST omitido por el usuario${NC}"
    fi
else
    echo -e "${YELLOW}\n⏭️  Escenario 4: SOAK TEST omitido${NC}"
fi

# Escenario 5: Spike Test
if [ "$SKIP_SPIKE" = false ]; then
    run_scenario \
        "Escenario 5: SPIKE TEST" \
        "scenarios/05-spike-test.js" \
        "$RESULTS_DIR/spike-test-$TIMESTAMP.json" \
        "Prueba de spikes - 10↔200 VUs con 3 picos súbitos"
else
    echo -e "${YELLOW}\n⏭️  Escenario 5: SPIKE TEST omitido${NC}"
fi

# Resumen final
echo -e "${CYAN}"
echo "============================================================"
echo "  RESUMEN DE EJECUCIÓN"
echo "============================================================"
echo -e "${NC}"

{
    echo ""
    echo "============================================================"
    echo "  ESTADÍSTICAS FINALES"
    echo "============================================================"
    echo "Total de pruebas ejecutadas: $TOTAL_TESTS"
    echo "Pruebas aprobadas: $PASSED_TESTS"
    echo "Pruebas fallidas: $FAILED_TESTS"
} >> "$SUMMARY_FILE"

if [ $FAILED_TESTS -eq 0 ]; then
    echo "✅ TODAS LAS PRUEBAS APROBADAS" >> "$SUMMARY_FILE"
    echo -e "${GREEN}\n✅ TODAS LAS PRUEBAS APROBADAS ($PASSED_TESTS/$TOTAL_TESTS)${NC}"
else
    echo "⚠️  ALGUNAS PRUEBAS FALLARON" >> "$SUMMARY_FILE"
    echo -e "${YELLOW}\n⚠️  $FAILED_TESTS de $TOTAL_TESTS pruebas FALLARON${NC}"
fi

echo -e "${GRAY}\n📄 Resumen guardado en: $SUMMARY_FILE${NC}"

# Mostrar resumen
cat "$SUMMARY_FILE"

echo -e "${CYAN}\n📊 Resultados detallados disponibles en:${NC}"
echo -e "${GRAY}   $RESULTS_DIR${NC}"

echo -e "${CYAN}\n🎯 PRÓXIMOS PASOS:${NC}"
echo -e "${GRAY}   1. Revisar métricas en los archivos JSON de results/${NC}"
echo -e "${GRAY}   2. Comparar con los SLOs definidos en el README${NC}"
echo -e "${GRAY}   3. Analizar logs en Seq: http://localhost:5341${NC}"
echo -e "${GRAY}   4. Revisar métricas en Grafana: http://localhost:3000${NC}"

echo -e "${GREEN}\n✨ Pruebas de rendimiento completadas!\n${NC}"

# Código de salida
if [ $FAILED_TESTS -gt 0 ]; then
    exit 1
else
    exit 0
fi
