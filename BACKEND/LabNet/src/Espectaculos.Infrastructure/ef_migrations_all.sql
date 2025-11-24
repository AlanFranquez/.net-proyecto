CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE beneficio (
        "BeneficioId" uuid NOT NULL,
        "Tipo" text NOT NULL,
        "Nombre" character varying(100) NOT NULL,
        "Descripcion" text,
        "VigenciaInicio" timestamp with time zone,
        "VigenciaFin" timestamp with time zone,
        "CupoTotal" integer,
        "CupoPorUsuario" integer,
        "RequiereBiometria" boolean NOT NULL,
        "CriterioElegibilidad" text,
        "RowVersion" bytea,
        CONSTRAINT "PK_beneficio" PRIMARY KEY ("BeneficioId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE espacio (
        "Id" uuid NOT NULL,
        "Nombre" character varying(100) NOT NULL,
        "Activo" boolean NOT NULL,
        "Tipo" integer NOT NULL,
        "Modo" integer NOT NULL,
        CONSTRAINT "PK_espacio" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE "Novedades" (
        "NovedadId" uuid NOT NULL,
        "Titulo" character varying(200) NOT NULL,
        "Contenido" text,
        "Tipo" integer NOT NULL,
        "CreadoEnUtc" timestamp with time zone NOT NULL,
        "PublicadoDesdeUtc" timestamp with time zone,
        "PublicadoHastaUtc" timestamp with time zone,
        "Publicado" boolean NOT NULL,
        CONSTRAINT "PK_Novedades" PRIMARY KEY ("NovedadId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE regla_de_acceso (
        "ReglaId" uuid NOT NULL,
        "VentanaHoraria" text NOT NULL,
        "VigenciaInicio" timestamp with time zone NOT NULL,
        "VigenciaFin" timestamp with time zone NOT NULL,
        "Prioridad" integer NOT NULL,
        "Politica" integer NOT NULL,
        "Rol" text,
        "RequiereBiometriaConfirmacion" boolean NOT NULL,
        CONSTRAINT "PK_regla_de_acceso" PRIMARY KEY ("ReglaId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE rol (
        "RolId" uuid NOT NULL,
        "Tipo" character varying(50) NOT NULL,
        "Prioridad" integer NOT NULL,
        "FechaAsignado" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_rol" PRIMARY KEY ("RolId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE usuario (
        "UsuarioId" uuid NOT NULL,
        "Documento" character varying(50) NOT NULL,
        "PasswordHash" character varying(150) NOT NULL,
        "Nombre" character varying(100) NOT NULL,
        "Apellido" character varying(100) NOT NULL,
        "Email" character varying(150) NOT NULL,
        "Estado" text NOT NULL,
        "CredencialId" uuid,
        CONSTRAINT "PK_usuario" PRIMARY KEY ("UsuarioId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE beneficio_espacio (
        "BeneficioId" uuid NOT NULL,
        "EspacioId" uuid NOT NULL,
        CONSTRAINT "PK_beneficio_espacio" PRIMARY KEY ("BeneficioId", "EspacioId"),
        CONSTRAINT "FK_beneficio_espacio_beneficio_BeneficioId" FOREIGN KEY ("BeneficioId") REFERENCES beneficio ("BeneficioId") ON DELETE CASCADE,
        CONSTRAINT "FK_beneficio_espacio_espacio_EspacioId" FOREIGN KEY ("EspacioId") REFERENCES espacio ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE espacio_regla_de_acceso (
        "EspacioId" uuid NOT NULL,
        "ReglaId" uuid NOT NULL,
        CONSTRAINT "PK_espacio_regla_de_acceso" PRIMARY KEY ("EspacioId", "ReglaId"),
        CONSTRAINT "FK_espacio_regla_de_acceso_espacio_EspacioId" FOREIGN KEY ("EspacioId") REFERENCES espacio ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_espacio_regla_de_acceso_regla_de_acceso_ReglaId" FOREIGN KEY ("ReglaId") REFERENCES regla_de_acceso ("ReglaId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE beneficio_usuario (
        "BeneficioId" uuid NOT NULL,
        "UsuarioId" uuid NOT NULL,
        CONSTRAINT "PK_beneficio_usuario" PRIMARY KEY ("BeneficioId", "UsuarioId"),
        CONSTRAINT "FK_beneficio_usuario_beneficio_BeneficioId" FOREIGN KEY ("BeneficioId") REFERENCES beneficio ("BeneficioId") ON DELETE CASCADE,
        CONSTRAINT "FK_beneficio_usuario_usuario_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuario ("UsuarioId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE canje (
        "CanjeId" uuid NOT NULL,
        "BeneficioId" uuid NOT NULL,
        "UsuarioId" uuid NOT NULL,
        "Fecha" timestamp with time zone NOT NULL,
        "Estado" text NOT NULL,
        "VerificacionBiometrica" boolean,
        "Firma" character varying(1024),
        CONSTRAINT "PK_canje" PRIMARY KEY ("CanjeId"),
        CONSTRAINT "FK_canje_beneficio_BeneficioId" FOREIGN KEY ("BeneficioId") REFERENCES beneficio ("BeneficioId") ON DELETE RESTRICT,
        CONSTRAINT "FK_canje_usuario_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuario ("UsuarioId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE credencial (
        "CredencialId" uuid NOT NULL,
        "Tipo" text NOT NULL,
        "Estado" text NOT NULL,
        "IdCriptografico" text NOT NULL,
        "FechaEmision" character varying(48) NOT NULL,
        "FechaExpiracion" timestamp with time zone,
        "UsuarioId" uuid NOT NULL,
        CONSTRAINT "PK_credencial" PRIMARY KEY ("CredencialId"),
        CONSTRAINT "FK_credencial_usuario_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuario ("UsuarioId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE dispositivo (
        "DispositivoId" uuid NOT NULL,
        "NumeroTelefono" character varying(20),
        "Plataforma" integer NOT NULL,
        "HuellaDispositivo" text NOT NULL,
        "BiometriaHabilitada" boolean NOT NULL,
        "Estado" integer NOT NULL,
        "UsuarioId" uuid NOT NULL,
        CONSTRAINT "PK_dispositivo" PRIMARY KEY ("DispositivoId"),
        CONSTRAINT "FK_dispositivo_usuario_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuario ("UsuarioId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE usuario_rol (
        "UsuarioId" uuid NOT NULL,
        "RolId" uuid NOT NULL,
        "FechaAsignado" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_usuario_rol" PRIMARY KEY ("UsuarioId", "RolId"),
        CONSTRAINT "FK_usuario_rol_rol_RolId" FOREIGN KEY ("RolId") REFERENCES rol ("RolId") ON DELETE CASCADE,
        CONSTRAINT "FK_usuario_rol_usuario_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuario ("UsuarioId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE evento_acceso (
        "EventoId" uuid NOT NULL,
        "MomentoDeAcceso" timestamp with time zone NOT NULL,
        "CredencialId" uuid NOT NULL,
        "EspacioId" uuid NOT NULL,
        "Resultado" text NOT NULL,
        "Motivo" character varying(1000),
        "Modo" text NOT NULL,
        "Firma" character varying(1024),
        CONSTRAINT "PK_evento_acceso" PRIMARY KEY ("EventoId"),
        CONSTRAINT "FK_evento_acceso_credencial_CredencialId" FOREIGN KEY ("CredencialId") REFERENCES credencial ("CredencialId") ON DELETE RESTRICT,
        CONSTRAINT "FK_evento_acceso_espacio_EspacioId" FOREIGN KEY ("EspacioId") REFERENCES espacio ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE notificacion (
        id uuid NOT NULL,
        tipo integer NOT NULL,
        titulo text NOT NULL,
        cuerpo text,
        programada_para_utc timestamp with time zone,
        estado integer NOT NULL,
        lectura_estado integer NOT NULL,
        canales jsonb NOT NULL,
        metadatos jsonb,
        creado_en_utc timestamp with time zone NOT NULL,
        audiencia integer NOT NULL,
        dispositivo_id uuid,
        usuario_id uuid,
        CONSTRAINT pk_notificacion PRIMARY KEY (id),
        CONSTRAINT "FK_notificacion_dispositivo_dispositivo_id" FOREIGN KEY (dispositivo_id) REFERENCES dispositivo ("DispositivoId") ON DELETE CASCADE,
        CONSTRAINT "FK_notificacion_usuario_usuario_id" FOREIGN KEY (usuario_id) REFERENCES usuario ("UsuarioId") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE TABLE sincronizacion (
        "SincronizacionId" uuid NOT NULL,
        "CreadoEn" timestamp with time zone NOT NULL,
        "CantidadItems" integer NOT NULL,
        "Tipo" text NOT NULL,
        "Estado" text,
        "DetalleError" text,
        "Checksum" text,
        "DispositivoId" uuid NOT NULL,
        CONSTRAINT "PK_sincronizacion" PRIMARY KEY ("SincronizacionId"),
        CONSTRAINT "FK_sincronizacion_dispositivo_DispositivoId" FOREIGN KEY ("DispositivoId") REFERENCES dispositivo ("DispositivoId") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_beneficio_espacio_EspacioId" ON beneficio_espacio ("EspacioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_beneficio_usuario_UsuarioId" ON beneficio_usuario ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_canje_BeneficioId" ON canje ("BeneficioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_canje_UsuarioId" ON canje ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE UNIQUE INDEX "IX_credencial_UsuarioId" ON credencial ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_dispositivo_UsuarioId" ON dispositivo ("UsuarioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_espacio_regla_de_acceso_ReglaId" ON espacio_regla_de_acceso ("ReglaId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_evento_acceso_CredencialId" ON evento_acceso ("CredencialId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_evento_acceso_EspacioId" ON evento_acceso ("EspacioId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX ix_notificacion_dispositivo_lectura ON notificacion (dispositivo_id, lectura_estado);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX ix_notificacion_estado ON notificacion (estado);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_notificacion_usuario_id" ON notificacion (usuario_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_Novedades_CreadoEnUtc" ON "Novedades" ("CreadoEnUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_Novedades_Publicado" ON "Novedades" ("Publicado");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_Novedades_PublicadoDesdeUtc" ON "Novedades" ("PublicadoDesdeUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_Novedades_PublicadoHastaUtc" ON "Novedades" ("PublicadoHastaUtc");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_sincronizacion_DispositivoId" ON sincronizacion ("DispositivoId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    CREATE INDEX "IX_usuario_rol_RolId" ON usuario_rol ("RolId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251124041734_Init') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251124041734_Init', '8.0.8');
    END IF;
END $EF$;
COMMIT;

