namespace SmartParking.Api.Models;

public enum RolUsuario
{
    conductor,
    administrador,
    operador
}

public enum EstadoPlaza
{
    libre,
    reservada,
    ocupada,
    mantenimiento
}

public enum TipoVehiculo
{
    auto,
    moto,
    camioneta
}

public enum EstadoReserva
{
    pendiente,
    confirmada,
    cancelada,
    expirada,
    completada
}

public enum TipoAcceso
{
    entrada,
    salida
}

public enum MetodoPago
{
    efectivo,
    tarjeta,
    transferencia
}

public enum EstadoPago
{
    pendiente,
    pagado,
    anulado
}
