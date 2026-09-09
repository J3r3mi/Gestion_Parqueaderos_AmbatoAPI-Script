# Gestión de Parqueaderos Ambato

Aplicación móvil para la localización, reserva y orientación hacia parqueaderos privados en zonas comerciales de la ciudad de Ambato mediante geolocalización y optimización de rutas.

## Componentes del proyecto

El sistema está compuesto por:

* **Aplicación móvil:** interfaz utilizada por los usuarios para consultar parqueaderos, visualizar su ubicación, realizar reservas y orientarse mediante rutas.
* **API REST:** servicio encargado de gestionar la comunicación entre la aplicación móvil y la base de datos.
* **Base de datos MySQL:** almacena la información de usuarios, parqueaderos, espacios, reservas y demás datos necesarios para el funcionamiento del sistema.

## Tecnologías utilizadas

### Aplicación móvil

* Ionic
* Angular
* TypeScript

### API

* C#
* ASP.NET Core Web API
* Entity Framework Core
* MySQL

### Base de datos

* MySQL

## Funcionalidades principales

* Registro y gestión de usuarios.
* Consulta de parqueaderos privados.
* Visualización de parqueaderos mediante geolocalización.
* Consulta de espacios disponibles.
* Reserva de espacios de estacionamiento.
* Gestión de parqueaderos.
* Gestión de espacios.
* Consulta y administración de reservas.
* Orientación hacia los parqueaderos mediante rutas.

## Estructura del proyecto

```text
Gestion-Parqueaderos-Ambato
│
├── API
│   └── GestionParqueaderosAmbato.API
│
├── Aplicacion-Movil
│   └── Proyecto de aplicación móvil
│
├── BaseDeDatos
│   └── Script MySQL
│
├── CAMBIOS
└── README.md
```

## Base de datos

El proyecto utiliza **MySQL** como sistema gestor de base de datos. La base de datos contiene las estructuras necesarias para administrar usuarios, roles, parqueaderos, espacios y reservas.

## Funcionamiento

La aplicación móvil consume los servicios proporcionados por la API REST. La API procesa las solicitudes realizadas desde la aplicación y realiza las operaciones correspondientes sobre la base de datos MySQL.

```text
Aplicación móvil
       ↓
    API REST
       ↓
    MySQL
```
