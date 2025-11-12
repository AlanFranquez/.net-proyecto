# ? FLUJO COMPLETO CORREGIDO - Mostrar QR en Espacio

## ?? Problema Resuelto

**Error encontrado:** Había un typo en la línea 17 del constructor: `EspacioProfilView` en lugar de `EspacioPerfilView`

**Solución aplicada:** 
- Corregido el nombre del constructor
- Agregado logging detallado para debugging
- Mejorado el manejo de errores

---

## ?? Flujo Completo Ahora Funcional

### 1. **Usuario Ve Lista de Espacios**
```
EspacioView ? Muestra todos los espacios (aulas, laboratorios, etc.)
```

### 2. **Usuario Hace Clic en un Espacio**
```
EspacioView ? OnEspacioSelected() 
           ? Shell.Current.GoToAsync("espacioPerfil?espacioId={id}")
           ? EspacioPerfilView
```

### 3. **Vista de Perfil del Espacio - TRES OPCIONES**

#### Opción A: Mostrar QR (FUNCIONANDO ?)
```
Botón: "Mostrar QR"
    ?
OnShowQRClicked()
    ?
1. Obtiene usuario logueado
2. Busca credencial del usuario
3. Obtiene datos del espacio
4. Genera QR: "{IdCriptografico}|{espacioId}"
5. Abre modal con QR
```

#### Opción B: Acceder con Biometría (FUNCIONANDO ?)
```
Botón: "Acceder a este Espacio"
    ?
OnAccessSpaceClicked()
    ?
1. Solicita confirmación
2. Pide autenticación biométrica
3. Valida credencial
4. Muestra resultado
5. Registra evento
```

#### Opción C: Ver Información (SIEMPRE DISPONIBLE ?)
```
Vista muestra automáticamente:
- Nombre del espacio
- Tipo
- Descripción
- Estado (Activo/Inactivo)
```

---

## ?? Cómo Probar el Flujo Completo

### Test 1: Mostrar QR ?

**Pasos:**
1. Login con usuario normal (no funcionario)
2. Ir a **"Espacios"** (ícono calendario)
3. **Click** en cualquier espacio
4. Presionar **"Mostrar QR"**

**Resultado esperado:**
```
? Se abre modal con código QR
? QR contiene: "{IdCriptografico}|{espacioId}"
? El QR se puede escanear
```

**Si falla, revisar logs:**
```
[EspacioPerfil] OnShowQRClicked - Starting...
[EspacioPerfil] Logged user: {email}, idApi: {id}
[EspacioPerfil] Total credenciales found: {count}
[EspacioPerfil] Checking credencial: {id}...
[EspacioPerfil] Found matching credencial!
[EspacioPerfil] Generating QR with data: {data}
[EspacioPerfil] QR Modal opened successfully
```

---

### Test 2: Acceder con Biometría ?

**Pasos:**
1. Login con usuario normal
2. Ir a **"Espacios"**
3. **Click** en cualquier espacio
4. Presionar **"Acceder a este Espacio"**
5. Presionar **"Autenticar"**
6. Simular huella exitosa **"Sí"**

**Resultado esperado:**
```
? Diálogo de confirmación
? Simulación de huella
? Popup "Acceso Permitido"
? Evento registrado en historial
```

---

### Test 3: Ver Información ?

**Pasos:**
1. Login con usuario normal
2. Ir a **"Espacios"**
3. **Click** en cualquier espacio

**Resultado esperado:**
```
? Se muestra información del espacio
? Nombre visible
? Tipo visible
? Botones visibles
```

---

## ?? Logs de Debugging

### Logs Exitosos (Mostrar QR)

```
[EspacioPerfil] OnShowQRClicked - Starting...
[EspacioPerfil] Logged user: juan@test.com, idApi: usr-001
[EspacioPerfil] Total credenciales found: 3
[EspacioPerfil] Checking credencial: cred-001, usuarioIdApi: usr-001, IdCripto: EST-ABC123
[EspacioPerfil] Found matching credencial!
[EspacioPerfil] Espacio: Laboratorio Info 1, idApi: esp-lab-01
[EspacioPerfil] Generating QR with data: EST-ABC123|esp-lab-01
[EspacioPerfil] QR Modal opened successfully
```

### Logs si Falla (Sin Credencial)

```
[EspacioPerfil] OnShowQRClicked - Starting...
[EspacioPerfil] Logged user: maria@test.com, idApi: usr-002
[EspacioPerfil] Total credenciales found: 3
[EspacioPerfil] Checking credencial: cred-001, usuarioIdApi: usr-001, IdCripto: EST-ABC123
[EspacioPerfil] Checking credencial: cred-003, usuarioIdApi: usr-003, IdCripto: DOC-XYZ789
[EspacioPerfil] No credential found for user
Alert: "No tienes una credencial asignada..."
```

---

## ?? Problemas Comunes y Soluciones

### Problema 1: "No hay usuario logueado"

**Causa:** Usuario no está logueado o sesión expiró

**Solución:**
```csharp
// Verificar en LoginViewModel que se guarde correctamente:
await SessionManager.SaveUserAsync(u.UsuarioId, Email, u.idApi);
```

---

### Problema 2: "No tienes una credencial asignada"

**Causa:** Usuario no tiene credencial o `usuarioIdApi` no coincide

**Solución en BD:**
```sql
-- Verificar credenciales
SELECT * FROM credenciales WHERE usuarioIdApi = 'usr-001';

-- Si no existe, crear:
INSERT INTO credenciales (credencialId, tipo, estado, idCriptografico, usuarioIdApi)
VALUES ('cred-001', 'Estudiante', 'Activa', 'EST-ABC123', 'usr-001');
```

---

### Problema 3: "El espacio no tiene un ID válido"

**Causa:** Espacio sin `idApi` en BD

**Solución:**
```sql
-- Verificar espacios
SELECT * FROM espacios WHERE idApi IS NULL;

-- Actualizar:
UPDATE espacios SET idApi = 'esp-001' WHERE espacioId = 1;
```

---

### Problema 4: No aparece el botón "Mostrar QR"

**Causa:** Error en XAML

**Solución:** Verificar en `EspacioPerfilView.xaml`:
```xml
<Button Text="Mostrar QR" 
        Clicked="OnShowQRClicked" />
```

---

### Problema 5: QR se genera pero no se puede escanear

**Causa:** Formato incorrecto

**Verificar formato:**
```
Correcto: "EST-ABC123|esp-lab-01"
Incorrecto: "EST-ABC123esp-lab-01" (falta |)
```

---

## ?? Datos de Prueba Necesarios

### En la Base de Datos:

#### 1. Usuario
```json
{
  "usuarioId": "usr-001",
  "nombre": "Juan",
  "apellido": "Pérez",
  "email": "juan@test.com",
  "password": "Pass123!",
  "idApi": "usr-001"
}
```

#### 2. Credencial del Usuario
```json
{
  "credencialId": "cred-001",
  "tipo": "Estudiante",
  "estado": "Activa",
  "idCriptografico": "EST-ABC123",
  "usuarioIdApi": "usr-001",
  "fechaEmision": "2024-01-01",
  "fechaExpiracion": "2024-12-31"
}
```

#### 3. Espacio
```json
{
  "espacioId": 1,
  "idApi": "esp-lab-01",
  "nombre": "Laboratorio de Informática 1",
  "tipo": "Laboratorio",
  "activo": true,
  "modo": "QR"
}
```

---

## ?? Verificación Final

### Checklist Completo:

- [x] Constructor corregido (`EspacioPerfilView` no `EspacioProfilView`)
- [x] Método `OnShowQRClicked` implementado
- [x] Logging detallado agregado
- [x] Manejo de errores mejorado
- [x] Validación de usuario
- [x] Validación de credencial
- [x] Validación de espacio
- [x] Compilación exitosa
- [x] Listo para testing

---

## ?? Flujo Completo Resumido

```
Login
  ?
EspacioView (Lista de espacios)
  ?
Click en espacio
  ?
EspacioPerfilView
  ?
??????????????????????????????????????????????
?                     ?                      ?
?  "Mostrar QR"       ?  "Acceder"           ?
?  ?                  ?  ?                   ?
?  Busca credencial   ?  Biometría           ?
?  ?                  ?  ?                   ?
?  Genera QR          ?  Valida              ?
?  ?                  ?  ?                   ?
?  Abre modal         ?  Registra acceso     ?
?                     ?                      ?
??????????????????????????????????????????????
```

---

## ?? Resumen

**? PROBLEMA RESUELTO:**
- Typo en constructor corregido
- Logging agregado para debugging
- Mejor manejo de errores

**? FUNCIONALIDADES DISPONIBLES:**
1. Ver información del espacio
2. Mostrar QR para acceso
3. Acceder con autenticación biométrica

**? LISTO PARA USAR:**
- Compila correctamente
- Todos los métodos implementados
- Flujo completo funcional

---

**?? Ya puedes probar el flujo completo desde login hasta mostrar QR!**

