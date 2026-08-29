# API REST - Gestión de Parqueaderos Ambato

API REST para la gestión de parqueaderos privados en la ciudad de Ambato.

El backend fue desarrollado con **ASP.NET Core, C# y Entity Framework Core**, conectado a **SQL Server**. La API permite gestionar usuarios, autenticación, parqueaderos, espacios de estacionamiento y reservas, proporcionando los servicios necesarios para su posterior integración con una aplicación móvil desarrollada con **Ionic + Angular**.

## Estructura de la API

La API está organizada de la siguiente manera:

```text
GestionParqueaderosAmbato.API/
│
├── Controllers/
│   ├── UsuariosController.cs
│   ├── ParqueaderosController.cs
│   ├── EspaciosController.cs
│   └── ReservasController.cs
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
│   ├── Rol.cs
│   ├── Parqueadero.cs
│   ├── Espacio.cs
│   └── Reserva.cs
│
├── appsettings.json
├── Program.cs
└── GestionParqueaderosAmbato.API.csproj
```

## Controllers

Contienen los endpoints que permiten realizar las operaciones sobre cada recurso de la aplicación.

### UsuariosController

Gestiona las operaciones relacionadas con los usuarios del sistema, incluyendo el registro y la autenticación.

### ParqueaderosController

Gestiona la información de los parqueaderos privados registrados en el sistema.

### EspaciosController

Gestiona los espacios de estacionamiento asociados a cada parqueadero.

### ReservasController

Gestiona las reservas de los espacios de estacionamiento.

## Data

Contiene el `GestionParqueaderosDbContext`, encargado de establecer la comunicación entre la API y la base de datos SQL Server mediante Entity Framework Core.

## DTOs

Contienen los objetos utilizados para recibir y devolver información de forma controlada, evitando exponer directamente determinados datos de las entidades.

Entre ellos se encuentran los DTO utilizados para el inicio de sesión, registro y respuesta de autenticación.

## Models

Representan las principales entidades utilizadas por el sistema y relacionadas con la base de datos.

Las entidades principales son:

* Usuario
* Rol
* Parqueadero
* Espacio
* Reserva

## Endpoints principales

La API REST dispone de los siguientes recursos principales:

| Controlador              | Recurso             | Función                                |
| ------------------------ | ------------------- | -------------------------------------- |
| `UsuariosController`     | `/api/Usuarios`     | Gestión de usuarios y autenticación    |
| `ParqueaderosController` | `/api/Parqueaderos` | Gestión de parqueaderos privados       |
| `EspaciosController`     | `/api/Espacios`     | Gestión de espacios de estacionamiento |
| `ReservasController`     | `/api/Reservas`     | Gestión de reservas                    |

Estos recursos constituyen la base del backend para su posterior integración con la aplicación móvil.

## Base de datos

La API utiliza **SQL Server** con la siguiente base de datos:

```text
GestionParqueaderosAmbato
```

La base de datos contiene las estructuras necesarias para almacenar y gestionar la información de usuarios, roles, parqueaderos, espacios y reservas.

## Tecnologías utilizadas

* **Lenguaje:** C#
* **Framework:** ASP.NET Core
* **ORM:** Entity Framework Core
* **Base de datos:** Microsoft SQL Server
* **Frontend previsto:** Ionic + Angular
* **Arquitectura:** API REST.
