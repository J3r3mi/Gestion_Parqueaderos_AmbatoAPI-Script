# Gestión de Parqueaderos Ambato - API

API REST para la gestión de parqueaderos privados en la ciudad de Ambato.

El backend fue desarrollado con **ASP.NET Core, C# y Entity Framework Core**, utilizando **MySQL** como sistema gestor de base de datos.

La versión actual del proyecto incorpora una **base de datos actualizada y ampliada**, reemplazando la estructura inicial basada en SQL Server e incorporando nuevas entidades y funcionalidades para mejorar la gestión integral de los parqueaderos.

La API permite gestionar usuarios, roles, parqueaderos, tarifas, plazas de estacionamiento, reservas, accesos, transacciones de pago y recuperación de contraseñas, proporcionando los servicios necesarios para su posterior integración con una aplicación móvil desarrollada con **Ionic + Angular**.

## Estructura de la API

La API está organizada de la siguiente manera:

```text
GestionParqueaderosAmbato.API/
│
├── Controllers/
│   ├── UsuariosController.cs
│   ├── ParqueaderosController.cs
│   ├── PlazasController.cs
│   ├── ReservasController.cs
│   ├── AccesosController.cs
│   ├── TarifasController.cs
│   ├── TransaccionesPagoController.cs
│   └── PasswordResetController.cs
│
├── Data/
│   └── GestionParqueaderosDbContext.cs
│
├── DTOs/
│   ├── LoginDto.cs
│   ├── LoginRespuestaDto.cs
│   ├── RegistroUsuarioDto.cs
│   └── UsuarioDto.cs
│
├── Models/
│   ├── Usuario.cs
│   ├── Parqueadero.cs
│   ├── Tarifa.cs
│   ├── Plaza.cs
│   ├── Reserva.cs
│   ├── Acceso.cs
│   ├── TransaccionPago.cs
│   └── PasswordResetToken.cs
│
├── appsettings.json
├── Program.cs
└── GestionParqueaderosAmbato.API.csproj
```

> **Nota:** Los nombres de los Controllers y Models anteriores son una propuesta basada en las nuevas tablas de la base de datos. Deben coincidir con los archivos que realmente existan en el proyecto.

## Controllers

Los Controllers contienen los endpoints que permiten realizar las operaciones sobre los diferentes recursos del sistema.

### UsuariosController

Gestiona las operaciones relacionadas con los usuarios, incluyendo el registro, autenticación y administración de los diferentes roles del sistema.

### ParqueaderosController

Gestiona la información de los parqueaderos privados registrados, incluyendo su ubicación, capacidad y administrador responsable.

### TarifasController

Gestiona las tarifas de los parqueaderos de acuerdo con el tipo de vehículo y permite mantener un historial de tarifas mediante su fecha de vigencia.

### PlazasController

Gestiona las plazas de estacionamiento de cada parqueadero, incluyendo su código, tipo de vehículo y estado actual.

Los estados disponibles son:

* Libre
* Reservada
* Ocupada
* Mantenimiento

### ReservasController

Gestiona las reservas realizadas por los usuarios, incluyendo la hora estimada de llegada, estado de la reserva y generación de tokens QR para su validación.

### AccesosController

Registra y gestiona los accesos físicos al parqueadero, permitiendo controlar las entradas y salidas asociadas a una reserva.

### TransaccionesPagoController

Gestiona las transacciones de pago relacionadas con las reservas, incluyendo monto, método de pago y estado de la transacción.

### PasswordResetController

Gestiona los tokens utilizados para la recuperación de contraseñas de los usuarios, incluyendo su fecha de expiración y estado de utilización.

## Data

Contiene el `GestionParqueaderosDbContext`, encargado de establecer la comunicación entre la API y la base de datos **MySQL** mediante Entity Framework Core.

El contexto permite trabajar con las diferentes entidades del sistema y sus relaciones.

## DTOs

Contienen los objetos utilizados para recibir y devolver información de forma controlada, evitando exponer directamente determinados datos de las entidades.

Entre ellos se encuentran los DTO utilizados para:

* Inicio de sesión
* Registro de usuarios
* Respuestas de autenticación
* Gestión de información de usuarios

## Models

Representan las entidades principales utilizadas por el sistema y relacionadas con la base de datos MySQL.

Las entidades principales son:

* **Usuario**
* **Parqueadero**
* **Tarifa**
* **Plaza**
* **Reserva**
* **Acceso**
* **Transacción de pago**
* **Token de recuperación de contraseña**

## Relaciones principales

La base de datos establece las siguientes relaciones:

```text
Usuario
   │
   ├── Parqueaderos
   │
   ├── Reservas
   │
   ├── Accesos
   │
   └── Password Reset Tokens
          

Parqueadero
   │
   ├── Tarifas
   │
   └── Plazas
          

Plaza
   │
   └── Reservas
          

Reserva
   │
   ├── Accesos
   │
   └── Transacciones de Pago
```

Estas relaciones permiten gestionar el ciclo completo de utilización de un parqueadero, desde la disponibilidad de una plaza y la reserva, hasta el registro del acceso y el pago.

## Funcionalidades principales

La nueva estructura de la base de datos permite implementar funcionalidades como:

* Registro y autenticación de usuarios.
* Gestión de roles de conductor, administrador y operador.
* Administración de parqueaderos.
* Gestión de plazas de estacionamiento.
* Control del estado de las plazas.
* Administración de tarifas según tipo de vehículo.
* Creación y gestión de reservas.
* Generación y validación de tokens QR.
* Registro de entradas y salidas.
* Registro de transacciones de pago.
* Recuperación de contraseñas.
* Consulta de indicadores del parqueadero mediante la vista `vw_kpi_parqueadero`.

## Base de datos

La API utiliza **MySQL** como sistema gestor de base de datos.

La base de datos actual se denomina:

```text
smart_parking
```

La versión actual corresponde a una **estructura ampliada respecto al modelo inicial**, incorporando nuevas tablas y relaciones para cubrir funcionalidades adicionales del sistema.

### Tablas principales

| Tabla                   | Función                                         |
| ----------------------- | ----------------------------------------------- |
| `usuarios`              | Usuarios y roles del sistema                    |
| `parqueaderos`          | Información de los parqueaderos                 |
| `tarifas`               | Tarifas según parqueadero y tipo de vehículo    |
| `plazas`                | Espacios disponibles dentro de cada parqueadero |
| `reservas`              | Reservas realizadas por los usuarios            |
| `accesos`               | Registro de entradas y salidas                  |
| `transacciones_pago`    | Registro de pagos y recaudaciones               |
| `password_reset_tokens` | Recuperación de contraseñas                     |

Además, se incluye la vista:

```text
vw_kpi_parqueadero
```

Esta vista permite obtener indicadores relacionados con las plazas totales, ocupadas, libres y reservadas de cada parqueadero.

El script de creación y configuración de la base de datos MySQL se encuentra incluido en el repositorio.

## Actualización de la base de datos

La versión inicial del proyecto utilizaba **Microsoft SQL Server** con una estructura más reducida.

Como parte de la actualización del proyecto:

* Se reemplazó SQL Server por **MySQL**.
* Se actualizó la estructura de la base de datos.
* Se incorporaron nuevas tablas y relaciones.
* Se añadieron las entidades de tarifas, plazas, accesos y transacciones de pago.
* Se incorporó el sistema de recuperación de contraseñas.
* Se agregó información relacionada con los estados de las reservas y plazas.
* Se incorporó una vista para la obtención de indicadores del parqueadero.
* Se añadieron datos mínimos de prueba para facilitar el desarrollo y las pruebas.

## Tecnologías utilizadas

* **Lenguaje:** C#
* **Framework:** ASP.NET Core
* **ORM:** Entity Framework Core
* **Base de datos:** MySQL
* **Motor de almacenamiento:** InnoDB
* **Frontend previsto:** Ionic + Angular
* **Arquitectura:** API REST
* **Base de datos anterior:** Microsoft SQL Server