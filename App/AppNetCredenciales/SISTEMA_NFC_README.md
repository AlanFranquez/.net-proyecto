# ?? Sistema NFC de Control de Acceso

## ?? Descripción General

Sistema completo de control de acceso mediante NFC para la aplicación de credenciales. Permite a los funcionarios validar el acceso de usuarios mediante la lectura de chips NFC y registra todos los eventos en un historial.

## ??? Arquitectura Implementada

### Modelos
- **EventoAcceso**: Registra cada intento de acceso (exitoso o denegado)
- **Credencial**: Contiene el IdCriptográfico que se transmite vía NFC
- **Espacio**: Lugares donde se puede validar el acceso
- **ReglaDeAcceso**: Define qué credenciales tienen permiso

### Servicios
1. **NFCService** (`App\AppNetCredenciales\Services\NFCService.cs`)
   - Maneja la lectura y escritura de tags NFC
   - Disponible para Android (usando NfcAdapter)
   - Incluye modo de simulación para desarrollo

2. **EventosService** (`App\AppNetCredenciales\Services\EventosService.cs`)
   - Valida credenciales por IdCriptográfico
   - Verifica permisos según reglas de acceso
   - Registra eventos (permitidos, denegados, no encontrados)
   - Sincroniza con la API cuando hay conexión

### Vistas

#### 1. NFCEspacioSelectionView
**Ruta**: `nfc-espacios`

Vista donde el funcionario selecciona el espacio en el que está ubicado.

**Características**:
- Lista de espacios disponibles con iconos según tipo
- Información de cada espacio (nombre, tipo, descripción)
- Botón de cancelar

**ViewModel**: `NFCEspacioSelectionViewModel`

#### 2. NFCReaderActiveView
**Ruta**: `nfc-reader?espacioId={id}`

Vista principal del lector NFC activo.

**Características**:
- **Estados visuales**:
  - ?? Amarillo: Esperando dispositivo
  - ?? Azul: Validando
  - ?? Verde: Acceso concedido
  - ?? Rojo: Acceso denegado
  - ?? Naranja: Error

- **Feedback**:
  - Cambio de color de fondo según resultado
  - Vibración diferenciada (1 corta para éxito, 2 largas para error)
  - Muestra información del último acceso

- **Funciones**:
  - Lectura continua de tags NFC
  - Ver historial de eventos
  - Botón de simulación (solo desarrollo)
  - Detener lector

**ViewModel**: `NFCReaderActiveViewModel`

#### 3. CredencialView (Actualizada)
Vista del usuario común que muestra su credencial.

**Nueva funcionalidad**:
- Botón "Activar NFC para Acceso"
- Emite el IdCriptográfico vía NFC
- El usuario acerca su dispositivo al lector del funcionario

## ?? Flujo de Uso

### Funcionario (Lector)
1. Hacer clic en el botón NFC (??) en NavbarFuncionario
2. Seleccionar el espacio donde está ubicado
3. El lector se activa automáticamente
4. Esperar a que los usuarios acerquen sus dispositivos
5. Ver resultados en tiempo real
6. Consultar historial si es necesario

### Usuario Común
1. Abrir vista de Credencial
2. Presionar "Activar NFC para Acceso"
3. Acercar el dispositivo al lector del funcionario
4. El sistema valida automáticamente

## ?? Registro de Eventos

Cada interacción registra un evento con:
- Fecha y hora
- Espacio donde ocurrió
- Credencial utilizada (o IdCriptográfico si no se encontró)
- Usuario asociado
- Resultado (Permitir/Denegar)
- Motivo (si fue denegado)
- Modo (Online/Offline)

## ?? Validación de Permisos

El sistema valida:
1. ? Credencial existe en la base de datos
2. ? Credencial está en estado "Activada"
3. ? Credencial no está expirada
4. ? Usuario tiene permisos según ReglaDeAcceso
   - Verifica tipo de credencial requerido
   - Verifica rol del usuario

## ??? Configuración

### Servicios Registrados (MauiProgram.cs)
```csharp
builder.Services.AddSingleton<NFCService>();
builder.Services.AddSingleton<IEventosService, EventosService>();
builder.Services.AddTransient<NFCEspacioSelectionView>();
builder.Services.AddTransient<NFCReaderActiveView>();
```

### Rutas Registradas (AppShell.xaml.cs)
```csharp
Routing.RegisterRoute("nfc-espacios", typeof(NFCEspacioSelectionView));
Routing.RegisterRoute("nfc-reader", typeof(NFCReaderActiveView));
```

## ?? Modo Desarrollo

Para facilitar el testing sin hardware NFC:

1. **Botón de Simulación**: En NFCReaderActiveView hay un botón "Simular Lectura" que procesa un IdCriptográfico de prueba.

2. **IdCriptográfico de prueba**: Por defecto usa `"ABC123XYZ"` (el mismo que tiene el usuario de prueba).

3. **Cambiar a producción**: En `NFCReaderActiveViewModel.cs`, cambiar:
```csharp
public bool ModoDesarrollo => false; // Oculta el botón de simulación
```

## ?? Métodos Agregados a LocalDBService

```csharp
// Búsqueda de credenciales
GetCredencialByIdCriptograficoAsync(string idCriptografico)

// Usuarios
GetUsuarioByIdApiAsync(string idApi)

// Espacios
GetEspacioByIdAsync(int espacioId)

// Eventos
SaveEventoAccesoAsync(EventoAcceso evento)
GetEventosAccesoByEspacioIdAsync(int espacioId)
GetEventosAccesoNoSincronizadosAsync()

// Reglas de acceso
GetReglasDeAccesoByEspacioIdAsync(int espacioId)
GetReglaDeAccesoByIdAsync(int reglaId)
```

## ?? Colores y UI

### Estados del Lector
- **Esperando**: `#FFC107` (Amarillo)
- **Validando**: `#2196F3` (Azul)
- **Concedido**: `#4CAF50` (Verde)
- **Denegado**: `#F44336` (Rojo)
- **Error**: `#FF9800` (Naranja)

### Iconos
- Lector NFC: ??
- Acceso concedido: ?
- Acceso denegado: ?
- Validando: ??
- Error: ??

## ?? Futuras Mejoras

1. **Vista de Historial**: Pantalla dedicada para ver todos los eventos de un espacio
2. **Estadísticas**: Gráficos de accesos por día/hora
3. **Exportar Historial**: CSV o PDF con los eventos
4. **Notificaciones Push**: Alertar al administrador de intentos de acceso denegados
5. **Modo HCE**: Implementar Host Card Emulation para emular una tarjeta NFC completa
6. **Soporte iOS**: Integrar CoreNFC para dispositivos Apple

## ?? Debugging

Para ver los logs del sistema NFC en Visual Studio:
```
[NFCService] - Logs del servicio NFC
[EventosService] - Logs de validación
[NFCReaderActiveVM] - Logs del lector activo
[NFCEspacioSelectionVM] - Logs de selección de espacio
```

## ? Testing

### Probar sin hardware NFC:
1. Usar el botón "Simular Lectura (Test)"
2. El sistema procesará un IdCriptográfico de prueba
3. Verificar que muestre acceso concedido/denegado correctamente

### Probar con NFC real:
1. Asegurarse que el dispositivo tiene NFC habilitado
2. El funcionario inicia el lector
3. El usuario activa NFC en su credencial
4. Acercar dispositivos (máximo 4cm de distancia)

## ?? Referencias

- **NFC en Android**: https://developer.android.com/guide/topics/connectivity/nfc
- **HCE (Host Card Emulation)**: https://developer.android.com/guide/topics/connectivity/nfc/hce
- **.NET MAUI Platform Features**: https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/

---

**Autor**: Sistema Automatizado de Credenciales  
**Versión**: 1.0.0  
**Fecha**: Noviembre 2024
