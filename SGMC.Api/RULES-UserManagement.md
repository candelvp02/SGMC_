# Reglas de Gestion de Cuentas de Usuario

## PBI 14 -- GGestión de Usuarios por Administrador

## 1) Restriccion de Eliminacion de Usuarios

Los usuarios nunca pueden ser eliminados del sistema de forma permanente.

Justificacion --> El sistema debe conservar el historial completo de actividad de cada usuario para fines de auditoria, trazabilidad y cumplimiento normativo.

Implementacion --> No existe ningun endpoint DELETE para usuarios en la API; la base de datos no permite operaciones de borrado en la tabla de usuarios. Y en su lugar, el campo 'IsActive' (o 'StatusId') cambia a inactivo.

## 2) Bloqueo Temporal de Cuentas

Los administradores pueden desactivar una cuenta de usuario en cualquier momento.

Reglas --> Al desactivar una cuenta, el usuario no podrá iniciar sesión ni acceder a ninguna funcionalidad del sistema; la cuenta y todos sus datos permaneceran intactos en el sistema; el admin puede reactivar la cuenta en cualquier momento; y toda desactivacion o reactivacion queda registrada con fecha y hora.

## 3) Gestion de Roles

Los administradores pueden cambiar el rol asignado a cualquier usuario.

Roles disponibles --> Paciente, Doctor y Administrador.

Reglas --> Solo un usuario con rol Administrador puede cambiar roles; el cambio de rol queda registrado con fecha y hora automaticamente. Y un usuario no puede cambiar su propio rol.

## 4) Control de Acceso a esta Seccion

Solo los usuarios autenticados con rol Administrador pueden: ver el listado de usuarios, buscar usuarios por nombre o correo, activar o desactivar cuentas, y cambiar roles.

Cualquier intento de acceso por parte de un paciente o doctor debe ser bloqueado y retornar un error 403 Forbidden.

## 5) Auditoria Automatica

El sistema registra automaticamente el 'CreatedAt' (fecha y hora en que se registro el usuario) y el 'UpdatedAt' (fecha y hora del ultimo cambio realizado sobre el usuario).

Estos campos son de solo lectura y no pueden ser modificados manualmente.

## 6) Campos Visibles en el Listado

- Nombre Completo - texto, solo lectura.
- Correo Electronico - texto, solo lectura.
- Rol Asignado - seleccion (Paciente/Doctor/Administrador), editable solo por administradores.
- Estado de la cuenta - activo o inactivo, editable solo por administradores.
- Fecha de registro - fecha y hora, solo lectura.
- Fecha de ultima actualizacion - fecha y hora, solo lectura.