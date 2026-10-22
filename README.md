# SistemaGestionLigaEsports

Backend para la gestión de una liga competitiva de Valorant. La API permite administrar jugadores, equipos, torneos, inscripciones, grupos y partidas, incluyendo la clasificación y las fases eliminatorias.

## Tecnologías

- C#
- ASP.NET Core Web API
- .NET 10
- Persistencia en memoria
- OpenAPI en entorno de desarrollo

## Arquitectura

El proyecto utiliza una arquitectura sencilla por capas:

```text
Controllers/
Dtos/
Modelos/
Repositorios/
  Interfaces/
Servicios/
  Interfaces/
```

- `Controllers`: expone los endpoints HTTP y traduce las reglas de negocio a respuestas HTTP.
- `Dtos`: define los contratos de entrada y salida de la API.
- `Modelos`: contiene las entidades internas del dominio.
- `Repositorios`: implementa el almacenamiento en memoria mediante interfaces.
- `Servicios`: contiene las reglas de negocio y depende de abstracciones de repositorio o servicio.

Los repositorios se registran como `Singleton` para conservar los datos durante la ejecución. Los servicios se registran como `Scoped` mediante inyección de dependencias.

## Módulos

### Jugadores

Administra el CRUD de jugadores.

Reglas principales:

- Cada jugador tiene un `Guid`.
- El `Nickname` es obligatorio y único sin distinguir mayúsculas y minúsculas.
- Un jugador no puede desactivarse o eliminarse mientras pertenezca a un equipo activo.

### Equipos

Administra equipos y sus plantillas.

Reglas principales:

- El nombre del equipo es obligatorio y único sin distinguir mayúsculas y minúsculas.
- Un equipo puede tener como máximo cinco jugadores activos.
- Solo se pueden agregar jugadores existentes y activos.
- Un jugador no puede pertenecer a dos equipos activos.
- Los equipos inscritos en torneos no pueden desactivarse, eliminarse ni perder jugadores.

### Torneos

Administra el ciclo de vida de los torneos.

Estados disponibles:

- `Creado`
- `InscripcionesAbiertas`
- `EnCurso`
- `Finalizado`

Reglas principales:

- La capacidad debe ser de 8 o 16 equipos.
- El torneo solo puede iniciar cuando alcanza su capacidad de inscripciones.
- Las transiciones de estado deben seguir el orden definido.
- Un torneo no puede finalizar antes de completar y finalizar la partida final.
- Un torneo con inscripciones no puede eliminarse.

### Inscripciones

Relaciona equipos existentes con torneos.

Reglas principales:

- Solo se inscriben equipos activos con exactamente cinco jugadores activos.
- Solo se aceptan inscripciones cuando el torneo está en `InscripcionesAbiertas`.
- Un equipo no puede inscribirse dos veces en el mismo torneo.
- La capacidad máxima del torneo no puede superarse.
- La comprobación de duplicados, capacidad y registro se realiza de forma atómica en memoria.
- Una inscripción no puede eliminarse cuando el torneo está en curso o finalizado.

### Grupos

Genera y consulta los grupos de un torneo.

Reglas principales:

- Solo se generan grupos para torneos en `EnCurso`.
- Los torneos de 8 equipos generan 2 grupos de 4.
- Los torneos de 16 equipos generan 4 grupos de 4.
- Cada equipo inscrito pertenece a un único grupo.
- La generación se basa únicamente en las inscripciones existentes.
- La generación de grupos no puede repetirse para el mismo torneo.

### Partidas

Genera los enfrentamientos de grupos y las fases eliminatorias, registra resultados y calcula clasificaciones.

Reglas principales:

- Cada pareja de equipos juega una sola vez en la fase de grupos.
- No se permiten enfrentamientos de un equipo contra sí mismo.
- Una victoria otorga 3 puntos y una derrota 0 puntos.
- Los desempates utilizan puntos, diferencia de rondas y enfrentamiento directo.
- En empates múltiples se utiliza una mini-clasificación entre los equipos empatados.
- Si los criterios deportivos no separan a los equipos, se utiliza `EquipoId` ascendente como criterio técnico determinista.
- Los resultados no pueden tener rondas negativas ni empate.
- Las partidas finalizadas no pueden modificarse.
- Las eliminatorias solo avanzan cuando la fase anterior está completa.

## Requisitos

- .NET 10 SDK instalado.
- Git, si se desea clonar el repositorio.

## Ejecución local

Desde la carpeta raíz del proyecto:

```bash
dotnet restore
dotnet build
dotnet run
```

La URL HTTP local configurada para desarrollo es:

```text
http://localhost:5167
```

El puerto puede variar según el perfil de ejecución o la configuración local.

La documentación OpenAPI se expone en desarrollo mediante la configuración incorporada de ASP.NET Core.

## Endpoints principales

### Jugadores

| Método | Ruta |
|---|---|
| GET | `/api/jugadores` |
| GET | `/api/jugadores/{id}` |
| POST | `/api/jugadores` |
| PUT | `/api/jugadores/{id}` |
| DELETE | `/api/jugadores/{id}` |

### Equipos

| Método | Ruta |
|---|---|
| GET | `/api/equipos` |
| GET | `/api/equipos/{id}` |
| POST | `/api/equipos` |
| PUT | `/api/equipos/{id}` |
| DELETE | `/api/equipos/{id}` |
| GET | `/api/equipos/{equipoId}/jugadores` |
| POST | `/api/equipos/{equipoId}/jugadores/{jugadorId}` |
| DELETE | `/api/equipos/{equipoId}/jugadores/{jugadorId}` |

### Torneos

| Método | Ruta |
|---|---|
| GET | `/api/torneos` |
| GET | `/api/torneos/{id}` |
| POST | `/api/torneos` |
| PUT | `/api/torneos/{id}` |
| DELETE | `/api/torneos/{id}` |

### Inscripciones

| Método | Ruta |
|---|---|
| GET | `/api/inscripciones` |
| GET | `/api/inscripciones/{id}` |
| POST | `/api/inscripciones` |
| DELETE | `/api/inscripciones/{id}` |

### Grupos

| Método | Ruta |
|---|---|
| POST | `/api/torneos/{torneoId}/grupos/generar` |
| GET | `/api/torneos/{torneoId}/grupos` |
| GET | `/api/grupos/{id}` |

### Partidas

| Método | Ruta |
|---|---|
| GET | `/api/torneos/{torneoId}/partidas` |
| GET | `/api/partidas/{id}` |
| POST | `/api/torneos/{torneoId}/partidas/generar-grupos` |
| PUT | `/api/partidas/{id}/resultado` |
| GET | `/api/grupos/{grupoId}/clasificacion` |
| POST | `/api/torneos/{torneoId}/eliminatorias/generar` |

## Limitaciones actuales

- Los datos se almacenan únicamente en memoria.
- Todos los datos se pierden al reiniciar la API.
- No existe todavía una base de datos.
- No se incluye autenticación.
- Las pruebas integrales completas de Postman están pendientes.

## Estado actual

La solución compila correctamente con `dotnet build`. Se han realizado comprobaciones básicas de algunos endpoints, pero todavía está pendiente la batería integral de pruebas de Postman para validar el flujo completo de los seis módulos.
