# ?? INICIO RÁPIDO - Autenticación Biométrica

## ? TODO LISTO Y FUNCIONANDO

Tu aplicación ahora tiene **autenticación biométrica** funcionando correctamente.

---

## ?? Cómo Probar AHORA (Modo Desarrollo)

### 1. Ejecutar la Aplicación

```bash
# Desde Visual Studio: Presiona F5
# O desde terminal:
dotnet build -t:Run -f net8.0-android
```

### 2. Flujo de Prueba

1. **Login** como funcionario
2. Ir a **ScanView** (botón de escanear)
3. Aparecerá:
   ```
   "¿Deseas continuar?"
   [Autenticar] [Cancelar]
   ```
4. Presionar **"Autenticar"**
5. Aparecerá:
   ```
   "¿Simular autenticación exitosa?"
   [Sí (Éxito)] [No (Fallo)]
   ```
6. Presionar **"Sí (Éxito)"**
7. ? Se activa la cámara
8. Escanear QR con formato: `cryptoId|espacioId`
9. Ver resultado y registro en historial

---

## ?? Lo Que Cambió

### ANTES
```
Funcionario ? Cámara ? Escanear QR ? Validar
```

### AHORA
```
Funcionario ? HUELLA DIGITAL ? ? Cámara ? Escanear QR ? Validar
```

---

## ?? Archivos Importantes

| Archivo | Propósito |
|---------|-----------|
| `Views/ScanView.xaml.cs` | ? Tiene autenticación biométrica |
| `Services/BiometricService.cs` | ?? Servicio de huella (simulado) |
| `GUIA_COMPLETA_BIOMETRIA.md` | ?? Documentación completa |

---

## ?? Diferencia entre Modo Desarrollo y Producción

### Modo DESARROLLO (Actual) ?
- Simula huella con `DisplayAlert`
- Funciona en emuladores
- Perfecto para testing de lógica

### Modo PRODUCCIÓN (Futuro) ?
```bash
# Instalar plugin real
dotnet add package Plugin.Fingerprint --version 3.0.0-beta.1

# Actualizar BiometricService (ver GUIA_COMPLETA_BIOMETRIA.md)
# Configurar permisos nativos
# Probar en dispositivo físico con huella real
```

---

## ? Comandos Útiles

### Limpiar y Reconstruir
```bash
dotnet clean
dotnet build
```

### Ejecutar en Android
```bash
dotnet build -t:Run -f net8.0-android
```

### Ejecutar en iOS
```bash
dotnet build -t:Run -f net8.0-ios
```

---

## ?? Solución de Problemas

### Error: "BiometricService not registered"
```bash
# Verificar que está en MauiProgram.cs:
builder.Services.AddSingleton<BiometricService>();
```

### Error: "Camera permission denied"
```bash
# En el dispositivo/emulador:
# Settings ? Apps ? Tu App ? Permissions ? Camera ? Allow
```

### La cámara no inicia
1. Cerrar y volver a abrir `ScanView`
2. Verificar permisos de cámara
3. Probar en dispositivo físico

---

## ?? Documentación Completa

Ver archivo **`GUIA_COMPLETA_BIOMETRIA.md`** para:
- Flujo detallado paso a paso
- Todas las validaciones de seguridad
- Migración a producción
- Testing exhaustivo
- Troubleshooting avanzado

---

## ? Todo Funcionando

? Compilación exitosa  
? Autenticación biométrica integrada  
? Validaciones de seguridad activas  
? Listo para testing  

---

**¿Preguntas?** Consulta `GUIA_COMPLETA_BIOMETRIA.md`

