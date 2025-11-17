# Funcionalidad NFC - Modo Tótem para Funcionarios

## Descripción General

Se ha implementado una funcionalidad completa de lectura NFC que permite a los **Funcionarios** actuar como un "tótem" o punto de control para leer las credenciales NFC de los **Usuarios**.

## Características Implementadas

### 1. **NFCService Mejorado** (`services/NFCService.cs`)
- ? Implementación real de lectura NFC para Android
- ? Soporte para lectura de tags NDEF
- ? Captura del UID del tag NFC
- ? Gestión de foreground dispatch para priorizar la lectura
- ? Timeout configurable (60 segundos)
- ? Manejo robusto de errores

### 2. **Vista NFCReaderView** (`Views/NFCReaderView.xaml`)
- ? Interfaz visual atractiva con animaciones
- ? Indicadores visuales del estado de lectura
- ? Visualización de información del tag leído
- ? Botones de control (Iniciar/Detener/Procesar)
- ? Instrucciones claras para el usuario

### 3. **Lógica de Procesamiento** (`Views/NFCReaderView.xaml.cs`)
- ? Verificación de rol de Funcionario
- ? Autenticación biométrica requerida
- ? Procesamiento del formato `CryptoId|EspacioId` (compatible con QR)
- ? Búsqueda de credenciales por ID criptográfico
- ? Verificación de estado de credencial
- ? Registro de eventos de acceso
- ? Feedback visual y táctil (vibraciones)

### 4. **Integración con MainActivity** (`Platforms/Android/MainActivity.cs`)
- ? Inicialización del NFCService
- ? Manejo de intents NFC
- ? Soporte para múltiples tipos de tags NFC

### 5. **Permisos Android** (`Platforms/Android/AndroidManifest.xml`)
- ? Permiso `android.permission.NFC`
- ? Feature `android.hardware.nfc` (opcional)

### 6. **Navegación**
- ? Ruta registrada en AppShell: `nfcReader`
- ? Botón NFC agregado al NavbarFuncionario
- ? Acceso exclusivo para usuarios con rol Funcionario

## Cómo Usar

### Para Funcionarios:

1. **Acceder al Lector NFC**
   - Desde cualquier pantalla, presiona el botón NFC (??) en el navbar
   - El sistema verificará tu rol de Funcionario

2. **Autenticación Biométrica**
   - Se solicitará verificar tu identidad con huella digital
   - Esto es obligatorio por seguridad

3. **Verificar NFC**
   - El sistema verificará si tu dispositivo tiene NFC habilitado
   - Si no está habilitado, recibirás una notificación

4. **Iniciar Lectura**
   - Presiona el botón "Iniciar Lectura NFC"
   - Verás una animación indicando que el sistema está listo

5. **Leer Credencial del Usuario**
   - Pide al usuario que acerque su dispositivo o tarjeta NFC
   - El sistema detectará automáticamente el tag
   - Se mostrará la información del tag leído

6. **Procesar Acceso**
   - Presiona "Procesar Credencial"
   - El sistema verificará:
     - Estado de la credencial (debe estar Activada)
     - Asociación con espacios
     - Reglas de acceso (próximamente)
   - Se registrará el evento de acceso

7. **Resultado**
   - ? **Acceso Permitido**: Vibración larga + mensaje de éxito
   - ? **Acceso Denegado**: Vibración corta doble + mensaje de error

## Formato de Datos NFC

### Formato Recomendado
Los tags NFC deben contener datos en formato:
```
{IdCriptografico}|{EspacioIdApi}
```

**Ejemplo:**
```
ABC123XYZ|550e8400-e29b-41d4-a716-446655440000
```

### Compatibilidad
- ? Tags NDEF con datos de texto
- ? Tags sin NDEF (usa UID como identificador)
- ? Compatible con mismo formato que QR codes

## Seguridad

### Medidas Implementadas:
1. **Autenticación Biométrica**: Requerida para acceder al lector
2. **Verificación de Rol**: Solo usuarios con rol "Funcionario"
3. **Registro de Eventos**: Todos los accesos quedan registrados
4. **Timeout**: Lectura se cancela automáticamente después de 60 segundos

## Flujo de Trabajo

```
???????????????????????
?   Funcionario       ?
?  presiona botón NFC ?
???????????????????????
           ?
           ?
???????????????????????
? Verificar Rol       ?
?   Funcionario       ?
???????????????????????
           ?
           ?
???????????????????????
?  Autenticación      ?
?    Biométrica       ?
???????????????????????
           ?
           ?
???????????????????????
? Verificar NFC       ?
?   Disponible        ?
???????????????????????
           ?
           ?
???????????????????????
?   Iniciar Lectura   ?
?   Esperar Tag       ?
???????????????????????
           ?
           ?
???????????????????????
?  Usuario Acerca     ?
?   Dispositivo NFC   ?
???????????????????????
           ?
           ?
???????????????????????
?   Leer Tag NFC      ?
? (UID + datos NDEF)  ?
???????????????????????
           ?
           ?
???????????????????????
?  Buscar Credencial  ?
?   y Espacio         ?
???????????????????????
           ?
           ?
???????????????????????
? Verificar Estado    ?
?   de Credencial     ?
???????????????????????
           ?
           ?
???????????????????????
?  Registrar Evento   ?
?    de Acceso        ?
???????????????????????
           ?
           ?
???????????????????????
?   Mostrar Resultado ?
?  (Permitido/Denegado)?
???????????????????????
```

## Requisitos Técnicos

### Dispositivo Android:
- ? Android 5.0 (API 21) o superior
- ? Hardware NFC
- ? NFC habilitado en configuración
- ? Sensor biométrico (huella digital)

### Permisos:
- `android.permission.NFC`
- `android.permission.USE_BIOMETRIC` (ya existente)

## Próximas Mejoras

### Funcionalidades Pendientes:
- [ ] Implementar verificación completa de reglas de acceso
- [ ] Escritura en tags NFC (para asignar credenciales)
- [ ] Soporte para múltiples formatos de tags
- [ ] Historial de lecturas NFC
- [ ] Modo offline con sincronización
- [ ] Estadísticas de uso NFC

### Mejoras de UX:
- [ ] Sonido al detectar tag
- [ ] Personalización de animaciones
- [ ] Modo oscuro
- [ ] Tutorial interactivo

## Troubleshooting

### "NFC No Disponible"
**Solución**: 
1. Ir a Configuración ? Conexiones ? NFC
2. Activar NFC
3. Activar Android Beam (opcional)

### "No se detecta el tag"
**Soluciones**:
1. Acercar más el dispositivo/tarjeta
2. Mantener el contacto por 2-3 segundos
3. Verificar que el tag esté funcionando
4. Probar en diferente posición

### "Credencial no encontrada"
**Soluciones**:
1. Verificar que la credencial esté sincronizada
2. Verificar formato de datos en el tag
3. Revisar logs para ID del tag detectado

### "Autenticación Biométrica Fallida"
**Soluciones**:
1. Limpiar sensor de huellas
2. Registrar huella nuevamente en configuración
3. Probar con otra huella registrada

## Testing

### Pruebas Recomendadas:

1. **Test de Roles**
   - ? Usuario normal NO debe poder acceder
   - ? Funcionario SÍ debe poder acceder

2. **Test de NFC**
   - ? Detectar dispositivo sin NFC
   - ? Detectar NFC deshabilitado
   - ? Leer tag NDEF
   - ? Leer tag sin NDEF (solo UID)

3. **Test de Seguridad**
   - ? Autenticación biométrica requerida
   - ? Timeout funciona correctamente
   - ? Cancelación funciona correctamente

4. **Test de Procesamiento**
   - ? Credencial válida + Espacio válido = Acceso
   - ? Credencial inactiva = Denegado
   - ? Credencial no encontrada = Error
   - ? Formato incorrecto = Error manejado

## Soporte

Para más información o reportar bugs, contacta al equipo de desarrollo.

---

**Versión**: 1.0.0  
**Fecha**: 2024  
**Plataforma**: .NET MAUI 8.0 - Android
