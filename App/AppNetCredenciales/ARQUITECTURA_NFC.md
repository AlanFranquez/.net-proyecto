# ??? Diagrama de Arquitectura del Sistema NFC

## Diagrama de Clases

```mermaid
classDiagram
    %% Modelos
    class EventoAcceso {
        +int EventoId
        +DateTime MomentoDeAcceso
        +int CredencialId
        +string CredencialIdApi
        +int EspacioId
        +string EspacioIdApi
        +AccesoTipo Resultado
        +string Motivo
        +Modo Modo
        +string Firma
    }

    class Credencial {
        +int CredencialId
        +string idApi
        +CredencialTipo Tipo
        +CredencialEstado Estado
        +string IdCriptografico
        +DateTime FechaEmision
        +DateTime? FechaExpiracion
        +string usuarioIdApi
    }

    class Usuario {
        +int UsuarioId
        +string idApi
        +string Nombre
        +string Apellido
        +string Documento
        +string Email
        +string[] RolesIDs
    }

    class Espacio {
        +int EspacioId
        +string idApi
        +string Nombre
        +EspacioTipo Tipo
        +string Descripcion
        +bool Activo
    }

    class ReglaDeAcceso {
        +int ReglaId
        +string TipoCredencialRequerida
        +string RolRequerido
        +DateTime? HoraInicio
        +DateTime? HoraFin
    }

    class EspacioReglaDeAcceso {
        +int Id
        +int EspacioId
        +int ReglaId
    }

    %% Servicios
    class NFCService {
        -bool _isReading
        +bool IsNFCAvailableAsync()
        +Task~NFCReadResult~ StartReadingAsync()
        +Task~NFCWriteResult~ WriteTagAsync(string data)
        +void StopReading()
        +void ProcessNfcIntent(Intent intent)
    }

    class IEventosService {
        <<interface>>
        +Task~EventoAccesoResult~ ValidarYRegistrarAcceso(idCripto, espacioId)
        +Task~List~EventoAcceso~~ ObtenerHistorial(espacioId, fecha)
        +Task~EventoAcceso~ RegistrarEvento(evento)
        +Task~bool~ SincronizarEventos()
    }

    class EventosService {
        -LocalDBService _db
        -ApiService _apiService
        -ConnectivityService _connectivity
        +Task~EventoAccesoResult~ ValidarYRegistrarAcceso(idCripto, espacioId)
        -Task~bool~ ValidarPermisosAcceso(credencial, espacio)
        -Task~EventoAcceso~ RegistrarEventoExitoso(credencial, espacioId)
        -Task~EventoAcceso~ RegistrarEventoDenegado(credencial, espacioId, motivo)
        -Task~EventoAcceso~ RegistrarEventoNoEncontrada(idCripto, espacioId)
        +Task~List~EventoAcceso~~ ObtenerHistorial(espacioId, fechas)
        +Task~bool~ SincronizarEventos()
    }

    class LocalDBService {
        -SQLiteAsyncConnection _connection
        +Task~Credencial?~ GetCredencialByIdCriptograficoAsync(idCripto)
        +Task~Usuario?~ GetUsuarioByIdApiAsync(idApi)
        +Task~Espacio?~ GetEspacioByIdAsync(espacioId)
        +Task~int~ SaveEventoAccesoAsync(evento)
        +Task~List~EventoAcceso~~ GetEventosAccesoByEspacioIdAsync(espacioId)
        +Task~List~EventoAcceso~~ GetEventosAccesoNoSincronizadosAsync()
        +Task~List~EspacioReglaDeAcceso~~ GetReglasDeAccesoByEspacioIdAsync(espacioId)
        +Task~ReglaDeAcceso?~ GetReglaDeAccesoByIdAsync(reglaId)
    }

    %% ViewModels
    class NFCEspacioSelectionViewModel {
        -LocalDBService _db
        +ObservableCollection~EspacioViewModel~ Espacios
        +bool NoEspaciosDisponibles
        +ICommand SelectEspacioCommand
        +Task LoadEspaciosAsync()
        -void OnSelectEspacio(espacioVm)
    }

    class EspacioViewModel {
        +int EspacioId
        +string Nombre
        +string Descripcion
        +string TipoTexto
        +string TipoIcon
        +EspacioViewModel(Espacio espacio)
    }

    class NFCReaderActiveViewModel {
        -NFCService _nfcService
        -IEventosService _eventosService
        -int _espacioId
        +string EspacioNombre
        +bool IsReading
        +string StatusMessage
        +string StatusIcon
        +Color BackgroundColor
        +bool HasLastResult
        +string LastNombreCompleto
        +ICommand VerHistorialCommand
        +ICommand SimularLecturaCommand
        +Task IniciarLectorAsync()
        -Task LoopLecturaContinua()
        -Task ProcesarTagNFC(idCripto)
        -Task MostrarAccesoConcedido(resultado)
        -Task MostrarAccesoDenegado(resultado)
        +void DetenerLector()
    }

    class CredencialViewModel {
        -LocalDBService _db
        -AuthService _auth
        +Credencial Credencial
        +bool IsNFCActive
        +Task LoadCredencialAsync()
    }

    %% Vistas
    class NFCEspacioSelectionView {
        -NFCEspacioSelectionViewModel _viewModel
        +NFCEspacioSelectionView(LocalDBService db)
        -void OnPageLoaded()
        -void OnCancelClicked()
    }

    class NFCReaderActiveView {
        -NFCReaderActiveViewModel _viewModel
        +NFCReaderActiveView(NFCService, IEventosService)
        -void OnPageLoaded()
        -void OnPageUnloaded()
        -void OnDetenerClicked()
    }

    class CredencialView {
        -CredencialViewModel _vm
        -NFCService _nfcService
        +CredencialView(AuthService, LocalDBService, NFCService)
        -void OnActivarNFCClicked()
    }

    %% Relaciones entre modelos
    EventoAcceso --> Credencial : usa
    EventoAcceso --> Espacio : ocurre_en
    EventoAcceso --> Usuario : pertenece_a
    Credencial --> Usuario : pertenece_a
    EspacioReglaDeAcceso --> Espacio : define_para
    EspacioReglaDeAcceso --> ReglaDeAcceso : aplica

    %% Relaciones de servicios
    EventosService ..|> IEventosService : implements
    EventosService --> LocalDBService : usa
    EventosService --> Credencial : valida
    EventosService --> Usuario : obtiene
    EventosService --> Espacio : verifica
    EventosService --> EventoAcceso : crea

    LocalDBService --> Credencial : gestiona
    LocalDBService --> Usuario : gestiona
    LocalDBService --> Espacio : gestiona
    LocalDBService --> EventoAcceso : gestiona
    LocalDBService --> ReglaDeAcceso : gestiona

    %% Relaciones ViewModels
    NFCEspacioSelectionViewModel --> LocalDBService : usa
    NFCEspacioSelectionViewModel --> EspacioViewModel : crea
    EspacioViewModel --> Espacio : representa

    NFCReaderActiveViewModel --> NFCService : usa
    NFCReaderActiveViewModel --> IEventosService : usa

    CredencialViewModel --> LocalDBService : usa
    CredencialViewModel --> Credencial : gestiona

    %% Relaciones Vistas-ViewModels
    NFCEspacioSelectionView --> NFCEspacioSelectionViewModel : bindea
    NFCReaderActiveView --> NFCReaderActiveViewModel : bindea
    CredencialView --> CredencialViewModel : bindea
    CredencialView --> NFCService : usa
```

## Diagrama de Secuencia - Flujo Completo

```mermaid
sequenceDiagram
    autonumber
    
    actor F as Funcionario
    participant NFCES as NFCEspacioSelectionView
    participant NFCESVM as NFCEspacioSelectionViewModel
    participant NFCRA as NFCReaderActiveView
    participant NFCRAVM as NFCReaderActiveViewModel
    participant NFC as NFCService
    
    actor U as Usuario
    participant CV as CredencialView
    participant CVM as CredencialViewModel
    
    participant ES as EventosService
    participant DB as LocalDBService
    
    %% Fase 1: Funcionario inicia lector
    F->>NFCES: Click en botón NFC
    NFCES->>NFCESVM: LoadEspaciosAsync()
    NFCESVM->>DB: GetEspaciosAsync()
    DB-->>NFCESVM: List<Espacio>
    NFCESVM->>NFCES: Muestra lista espacios
    
    F->>NFCES: Selecciona Espacio
    NFCES->>NFCRA: Navegar(espacioId)
    
    NFCRA->>NFCRAVM: IniciarLectorAsync()
    NFCRAVM->>NFC: IsNFCAvailableAsync()
    NFC-->>NFCRAVM: true
    NFCRAVM->>NFCRAVM: LoopLecturaContinua()
    NFCRAVM->>NFC: StartReadingAsync()
    NFCRAVM->>NFCRA: Muestra "Esperando..."
    
    %% Fase 2: Usuario acerca credencial
    U->>CV: Abre credencial
    CV->>CVM: LoadCredencialAsync()
    CVM->>DB: GetCredencialesAsync()
    DB-->>CVM: Credencial
    CVM->>CV: Muestra credencial
    
    U->>CV: Click "Activar NFC"
    CV->>NFC: WriteTagAsync(IdCriptografico)
    NFC-->>CV: Success
    CV->>U: Mensaje "Acerca dispositivo"
    
    %% Fase 3: Lectura y validación
    NFC->>NFCRAVM: Tag detectado (IdCriptografico)
    NFCRAVM->>NFCRA: Muestra "Validando..."
    NFCRAVM->>ES: ValidarYRegistrarAcceso(idCripto, espacioId)
    
    ES->>DB: GetCredencialByIdCriptograficoAsync()
    DB-->>ES: Credencial
    
    ES->>DB: GetUsuarioByIdApiAsync()
    DB-->>ES: Usuario
    
    ES->>ES: Validar estado credencial
    ES->>DB: GetEspacioByIdAsync()
    DB-->>ES: Espacio
    
    ES->>ES: ValidarPermisosAcceso()
    ES->>DB: GetReglasDeAccesoByEspacioIdAsync()
    DB-->>ES: List<Reglas>
    
    alt Acceso Concedido
        ES->>ES: RegistrarEventoExitoso()
        ES->>DB: SaveEventoAccesoAsync(evento)
        DB-->>ES: Evento guardado
        ES-->>NFCRAVM: EventoAccesoResult (concedido)
        NFCRAVM->>NFCRA: MostrarAccesoConcedido()
        NFCRA->>F: Pantalla VERDE + Info usuario
        NFCRAVM->>NFCRAVM: Vibración corta
    else Acceso Denegado
        ES->>ES: RegistrarEventoDenegado()
        ES->>DB: SaveEventoAccesoAsync(evento)
        DB-->>ES: Evento guardado
        ES-->>NFCRAVM: EventoAccesoResult (denegado)
        NFCRAVM->>NFCRA: MostrarAccesoDenegado()
        NFCRA->>F: Pantalla ROJA + Motivo
        NFCRAVM->>NFCRAVM: Vibración doble
    end
    
    NFCRAVM->>NFC: StartReadingAsync()
    Note over NFCRAVM,NFC: Vuelve a estado "Esperando"
```

## Diagrama de Componentes

```mermaid
graph TB
    subgraph "?? Presentación - XAML Views"
        V1[NFCEspacioSelectionView]
        V2[NFCReaderActiveView]
        V3[CredencialView]
    end
    
    subgraph "?? ViewModels"
        VM1[NFCEspacioSelectionViewModel]
        VM2[NFCReaderActiveViewModel]
        VM3[CredencialViewModel]
    end
    
    subgraph "?? Services Layer"
        S1[NFCService]
        S2[EventosService]
        S3[LocalDBService]
        S4[ApiService]
        S5[ConnectivityService]
    end
    
    subgraph "?? Data Layer"
        DB[(SQLite Local DB)]
        API[Backend API]
    end
    
    subgraph "?? Models"
        M1[EventoAcceso]
        M2[Credencial]
        M3[Usuario]
        M4[Espacio]
        M5[ReglaDeAcceso]
    end
    
    V1 --> VM1
    V2 --> VM2
    V3 --> VM3
    
    VM1 --> S3
    VM2 --> S1
    VM2 --> S2
    VM3 --> S3
    VM3 --> S1
    
    S2 --> S3
    S2 --> S4
    S2 --> S5
    S3 --> DB
    S4 --> API
    
    S3 -.-> M1
    S3 -.-> M2
    S3 -.-> M3
    S3 -.-> M4
    S3 -.-> M5
    
    style V1 fill:#E3F2FD
    style V2 fill:#E3F2FD
    style V3 fill:#E3F2FD
    style VM1 fill:#FFF9C4
    style VM2 fill:#FFF9C4
    style VM3 fill:#FFF9C4
    style S1 fill:#C8E6C9
    style S2 fill:#C8E6C9
    style S3 fill:#C8E6C9
    style DB fill:#FFE0B2
    style API fill:#FFE0B2
```

## Estados del Sistema

```mermaid
stateDiagram-v2
    [*] --> Seleccionando: Funcionario abre NFC
    Seleccionando --> Esperando: Selecciona espacio
    Esperando --> Validando: Tag detectado
    Validando --> AccesoConcedido: Credencial válida + permisos
    Validando --> AccesoDenegado: Sin permisos
    Validando --> CredencialNoEncontrada: Credencial inválida
    Validando --> Error: Excepción
    
    AccesoConcedido --> Esperando: Después de 3s
    AccesoDenegado --> Esperando: Después de 3s
    CredencialNoEncontrada --> Esperando: Después de 3s
    Error --> Esperando: Después de 3s
    
    Esperando --> [*]: Detener lector
    
    state Esperando {
        [*] --> LoopLectura
        LoopLectura --> LoopLectura: No hay tag
    }
    
    state Validando {
        [*] --> BuscarCredencial
        BuscarCredencial --> ValidarEstado
        ValidarEstado --> ValidarPermisos
        ValidarPermisos --> RegistrarEvento
    }
```

---

?? **Nota**: Estos diagramas muestran la arquitectura completa del sistema NFC, incluyendo todos los componentes, relaciones y flujos de datos.
