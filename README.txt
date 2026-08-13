Estructura de la API

La API está organizada de la siguiente manera:

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
Controllers

Contienen los endpoints que permiten realizar las operaciones sobre cada recurso.

Data

Contiene el DbContext, encargado de establecer la comunicación entre la API y SQL Server.

DTOs

Contienen los objetos utilizados para recibir y devolver información de forma controlada, evitando exponer directamente determinados datos de las entidades.

Models

Representan las entidades principales de la base de datos.



Base de datos

La API utiliza SQL Server con la siguiente base de datos:

GestionParqueaderosAmbato

El script de la base de datos se encuentra en:

BD/GestionParqueaderosAmbato.sql
Tablas principales
Roles

Almacena los roles disponibles para los usuarios.

Roles
├── IdRol
├── Nombre
├── Descripcion
└── Estado
Usuarios

Almacena la información de los usuarios registrados.

Usuarios
├── IdUsuario
├── IdRol
├── Nombres
├── Apellidos
├── Cedula
├── Correo
├── Telefono
├── PasswordHash
├── Estado
└── FechaRegistro

La contraseña no se almacena directamente. Se genera un hash utilizando PasswordHasher.

Parqueaderos

Almacena la información de los parqueaderos.

Parqueaderos
├── IdParqueadero
├── IdAdministrador
├── Nombre
├── Direccion
├── Latitud
├── Longitud
├── Telefono
├── HorarioAtencion
└── Estado

Las propiedades Latitud y Longitud permiten posteriormente utilizar la ubicación de los parqueaderos en el mapa de la aplicación móvil.

Espacios

Representa los espacios individuales pertenecientes a cada parqueadero.

Espacios
├── IdEspacio
├── IdParqueadero
├── NumeroEspacio
├── Estado
└── Observacion

Los espacios pueden manejar estados como:

Disponible
Ocupado
Reservado
Reservas

Almacena las reservas realizadas por los usuarios.

Reservas
├── IdReserva
├── IdUsuario
├── IdEspacio
├── FechaReserva
├── HoraInicio
├── HoraFin
└── Estado

Relaciones de la base de datos

Las principales relaciones son:

Roles
  │
  └── Usuarios
        │
        ├── Reservas
        │
        └── Parqueaderos
                │
                └── Espacios
                        │
                        └── Reservas
Relaciones
Un rol puede estar asociado a varios usuarios.
Un usuario pertenece a un rol.
Un administrador puede administrar uno o varios parqueaderos.
Un parqueadero contiene varios espacios.
Un espacio pertenece a un parqueadero.
Un usuario puede realizar varias reservas.
Una reserva pertenece a un usuario y a un espacio.


Entity Framework Core

La conexión entre la API y SQL Server se realiza mediante:

GestionParqueaderosDbContext

El contexto contiene los DbSet correspondientes a las entidades:

public DbSet<Usuario> Usuarios { get; set; }
public DbSet<Rol> Roles { get; set; }
public DbSet<Parqueadero> Parqueaderos { get; set; }
public DbSet<Espacio> Espacios { get; set; }
public DbSet<Reserva> Reservas { get; set; }

Entity Framework Core permite consultar y modificar la información de SQL Server desde C#.


Configuración de SQL Server

La cadena de conexión se encuentra en:

appsettings.json

Ejemplo:

{
  "ConnectionStrings": {
    "ConexionSQLServer": "Server=SERVIDOR\\SQLEXPRESS;Database=GestionParqueaderosAmbato;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}

Se debe cambiar SERVIDOR\\SQLEXPRESS por el nombre de la instancia de SQL Server del equipo donde se ejecute la API.

La base de datos debe llamarse:

GestionParqueaderosAmbato


Endpoints principales
Usuarios
Registrar usuario
POST /api/Usuarios/registro

Ejemplo:

{
  "idRol": 2,
  "nombres": "Juan",
  "apellidos": "Pérez",
  "cedula": "1801234567",
  "correo": "jose.perez@gmail.com",
  "telefono": "0992222222",
  "password": "123456"
}
Iniciar sesión
POST /api/Usuarios/login

Ejemplo:

{
  "correo": "jose.perez@gmail.com",
  "password": "123456"
}

El Login devuelve un JWT, que debe utilizarse para acceder a los endpoints protegidos.

Parqueaderos
GET    /api/Parqueaderos
GET    /api/Parqueaderos/{id}
POST   /api/Parqueaderos
PUT    /api/Parqueaderos/{id}
DELETE /api/Parqueaderos/{id}

El endpoint:

GET /api/Parqueaderos

devuelve información como:

{
  "idParqueadero": 1,
  "nombre": "Parqueadero Mera",
  "direccion": "Calle Mera, zona centro, Ambato",
  "latitud": -1.24908,
  "longitud": -78.61675,
  "telefono": "032999001",
  "horarioAtencion": "Lunes a Domingo 07:00 - 21:00",
  "estado": true
}
Espacios
GET    /api/Espacios
GET    /api/Espacios/{id}
POST   /api/Espacios
PUT    /api/Espacios/{id}
DELETE /api/Espacios/{id}

También existe el endpoint para consultar los espacios de un parqueadero específico:

GET /api/Espacios/parqueadero/{idParqueadero}

Ejemplo:

GET /api/Espacios/parqueadero/1
Reservas
GET    /api/Reservas
GET    /api/Reservas/{id}
POST   /api/Reservas
PUT    /api/Reservas/{id}
DELETE /api/Reservas/{id}

Las reservas utilizan el IdUsuario y el IdEspacio para relacionar al usuario con el espacio reservado.

9. Autenticación JWT

La API utiliza JWT para controlar el acceso a los endpoints protegidos.

El proceso es:

POST /api/Usuarios/login
           │
           ▼
     Credenciales
           │
           ▼
   Verificación de usuario
           │
           ▼
      Generación JWT
           │
           ▼
         Token
           │
           ▼
Authorization: Bearer TOKEN

Para enviar el token desde el frontend se debe utilizar el encabezado:

Authorization: Bearer <TOKEN>

En Swagger se puede utilizar el botón:

Authorize

para ingresar el token.


Seguridad de contraseñas

Las contraseñas no se guardan en texto plano.

Durante el registro se utiliza un PasswordHasher para generar el valor de PasswordHash.

Ejemplo de un hash generado:

AQAAAAIAAYagAAAAEIn+/vz6PhpAc/bMDiB+hdzjO1Y6uckk3C4wvMkl9vohpD3gk4desLR5pUvs3FtFhQ==

Durante el Login se verifica la contraseña proporcionada contra el hash almacenado.

Swagger

La API utiliza Swagger para documentar y probar los endpoints.

Una vez ejecutada la API, acceder a:

https://localhost:7121/swagger

La dirección y el puerto pueden cambiar dependiendo de la configuración del equipo.

Desde Swagger se pueden probar:

Usuarios
Login
Parqueaderos
Espacios
Reservas
Autenticación JWT

Prueba de autenticación

Se verificó que los endpoints protegidos no pueden ser utilizados sin autenticación.

Por ejemplo:

GET /api/Parqueaderos

sin JWT devuelve:

401 Unauthorized

Al enviar un JWT válido:

Authorization: Bearer <TOKEN>

la solicitud puede ejecutarse correctamente.


La API queda lista para ser consumida por el frontend Ionic + Angular.