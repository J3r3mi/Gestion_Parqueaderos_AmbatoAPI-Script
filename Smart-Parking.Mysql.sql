-- ============================================================
-- SISTEMA DE PARQUEO INTELIGENTE - AMBATO
-- Esquema de base de datos MySQL (InnoDB requerido para
-- transacciones y locking pesimista SELECT ... FOR UPDATE)
-- ============================================================

CREATE DATABASE IF NOT EXISTS smart_parking
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE smart_parking;

-- ------------------------------------------------------------
-- 1. USUARIOS Y ROLES
-- ------------------------------------------------------------
CREATE TABLE usuarios (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nombre          VARCHAR(120)    NOT NULL,
    cedula          VARCHAR(20)     NOT NULL UNIQUE,
    correo          VARCHAR(150)    NOT NULL UNIQUE,
    password_hash   VARCHAR(255)    NOT NULL,       -- BCrypt/Argon2 hash, nunca texto plano
    rol             ENUM('conductor', 'administrador', 'operador') NOT NULL DEFAULT 'conductor',
    activo          TINYINT(1)      NOT NULL DEFAULT 1,
    created_at      DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at      DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP
                                     ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- 2. PARQUEADEROS
-- ------------------------------------------------------------
CREATE TABLE parqueaderos (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    nombre          VARCHAR(150)    NOT NULL,
    direccion       VARCHAR(255)    NOT NULL,
    latitud         DECIMAL(10, 7)  NOT NULL,
    longitud        DECIMAL(10, 7)  NOT NULL,
    capacidad_total INT             NOT NULL,
    administrador_id INT            NULL,           -- admin responsable del local
    activo          TINYINT(1)      NOT NULL DEFAULT 1,
    created_at      DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_parqueadero_admin
        FOREIGN KEY (administrador_id) REFERENCES usuarios(id)
        ON DELETE SET NULL
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- 3. TARIFAS (histórico: nunca se borra, se versiona por fecha)
-- ------------------------------------------------------------
CREATE TABLE tarifas (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    parqueadero_id  INT             NOT NULL,
    tipo_vehiculo   ENUM('auto', 'moto', 'camioneta') NOT NULL DEFAULT 'auto',
    valor_hora      DECIMAL(6, 2)   NOT NULL,
    vigente_desde   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_tarifa_parqueadero
        FOREIGN KEY (parqueadero_id) REFERENCES parqueaderos(id)
        ON DELETE CASCADE
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- 4. PLAZAS (unidad de bloqueo transaccional para reservas)
-- ------------------------------------------------------------
CREATE TABLE plazas (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    parqueadero_id  INT             NOT NULL,
    codigo          VARCHAR(10)     NOT NULL,       -- ej. "A-12"
    estado          ENUM('libre', 'reservada', 'ocupada', 'mantenimiento')
                                    NOT NULL DEFAULT 'libre',
    tipo_vehiculo   ENUM('auto', 'moto', 'camioneta') NOT NULL DEFAULT 'auto',
    version         INT             NOT NULL DEFAULT 0,  -- reservado para futuro locking optimista
    created_at      DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_plaza_parqueadero
        FOREIGN KEY (parqueadero_id) REFERENCES parqueaderos(id)
        ON DELETE CASCADE,
    UNIQUE KEY uq_plaza_codigo (parqueadero_id, codigo),
    INDEX idx_plaza_estado (parqueadero_id, estado)   -- clave para consultas de "cupos libres"
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- 5. RESERVAS
-- ------------------------------------------------------------
CREATE TABLE reservas (
    id                  INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id          INT             NOT NULL,
    plaza_id            INT             NOT NULL,
    hora_reserva        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    hora_estimada_arribo DATETIME       NOT NULL,
    hora_llegada_real   DATETIME        NULL,
    hora_salida         DATETIME        NULL,
    estado              ENUM('pendiente', 'confirmada', 'cancelada', 'expirada', 'completada')
                                        NOT NULL DEFAULT 'pendiente',
    qr_token            VARCHAR(255)    NULL UNIQUE,   -- token firmado, validado en puerta
    qr_expira_en        DATETIME        NULL,
    created_at          DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_reserva_usuario
        FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_reserva_plaza
        FOREIGN KEY (plaza_id) REFERENCES plazas(id)
        ON DELETE CASCADE,
    INDEX idx_reserva_estado (estado),
    INDEX idx_reserva_arribo (hora_estimada_arribo)   -- para el job que expira reservas vencidas
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- 6. ACCESOS (validación física en puerta, hecha por el operador)
-- ------------------------------------------------------------
CREATE TABLE accesos (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    reserva_id      INT             NOT NULL,
    operador_id     INT             NULL,
    tipo            ENUM('entrada', 'salida') NOT NULL,
    fecha_hora      DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_acceso_reserva
        FOREIGN KEY (reserva_id) REFERENCES reservas(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_acceso_operador
        FOREIGN KEY (operador_id) REFERENCES usuarios(id)
        ON DELETE SET NULL
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- 7. TRANSACCIONES DE PAGO / RECAUDACIÓN
-- ------------------------------------------------------------
CREATE TABLE transacciones_pago (
    id              INT AUTO_INCREMENT PRIMARY KEY,
    reserva_id      INT             NOT NULL,
    monto           DECIMAL(8, 2)   NOT NULL,
    metodo_pago     ENUM('efectivo', 'tarjeta', 'transferencia') NOT NULL DEFAULT 'efectivo',
    estado          ENUM('pendiente', 'pagado', 'anulado') NOT NULL DEFAULT 'pendiente',
    fecha_hora      DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_pago_reserva
        FOREIGN KEY (reserva_id) REFERENCES reservas(id)
        ON DELETE CASCADE
) ENGINE=InnoDB;

-- ------------------------------------------------------------
-- 7B. TOKENS DE RECUPERACIÓN DE CONTRASEÑA
-- ------------------------------------------------------------
CREATE TABLE password_reset_tokens (
    id          INT AUTO_INCREMENT PRIMARY KEY,
    usuario_id  INT             NOT NULL,
    token       VARCHAR(255)    NOT NULL UNIQUE,
    expira_en   DATETIME        NOT NULL,
    usado       TINYINT(1)      NOT NULL DEFAULT 0,
    created_at  DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_reset_usuario
        FOREIGN KEY (usuario_id) REFERENCES usuarios(id)
        ON DELETE CASCADE,
    INDEX idx_reset_expira (expira_en)
) ENGINE=InnoDB;

-- ============================================================
-- VISTA DE APOYO PARA EL DASHBOARD (KPIs en tiempo real)
-- ============================================================
CREATE OR REPLACE VIEW vw_kpi_parqueadero AS
SELECT
    p.id                                                AS parqueadero_id,
    p.nombre                                            AS parqueadero_nombre,
    COUNT(pl.id)                                        AS plazas_totales,
    SUM(CASE WHEN pl.estado = 'ocupada'   THEN 1 ELSE 0 END) AS plazas_ocupadas,
    SUM(CASE WHEN pl.estado = 'libre'     THEN 1 ELSE 0 END) AS cupos_libres,
    SUM(CASE WHEN pl.estado = 'reservada' THEN 1 ELSE 0 END) AS reservas_activas
FROM parqueaderos p
LEFT JOIN plazas pl ON pl.parqueadero_id = p.id
GROUP BY p.id, p.nombre;

-- ============================================================
-- DATOS DE PRUEBA MÍNIMOS PARA DESARROLLO
-- ============================================================
INSERT INTO usuarios (nombre, cedula, correo, password_hash, rol) VALUES
('Admin Demo',      '1800000001', 'admin@parqueo.ec',    '$2a$11$PLACEHOLDER_HASH', 'administrador'),
('Conductor Demo',  '1800000002', 'conductor@parqueo.ec','$2a$11$PLACEHOLDER_HASH', 'conductor'),
('Operador Demo',   '1800000003', 'operador@parqueo.ec', '$2a$11$PLACEHOLDER_HASH', 'operador');

INSERT INTO parqueaderos (nombre, direccion, latitud, longitud, capacidad_total, administrador_id) VALUES
('Parqueadero Centro Ambato', 'Cevallos y Bolívar', -1.2417700, -78.6227900, 20, 1);

INSERT INTO tarifas (parqueadero_id, tipo_vehiculo, valor_hora) VALUES
(1, 'auto', 1.00),
(1, 'moto', 0.50);

INSERT INTO plazas (parqueadero_id, codigo, tipo_vehiculo) VALUES
(1, 'A-01', 'auto'), (1, 'A-02', 'auto'), (1, 'A-03', 'auto'),
(1, 'M-01', 'moto'), (1, 'M-02', 'moto');
