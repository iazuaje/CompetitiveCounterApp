# CompetitiveCounterApp

![.NET MAUI](https://img.shields.io/badge/.NET_MAUI-10-512BD4?logo=.net)
![Platform](https://img.shields.io/badge/Platform-Android-3DDC84)

Aplicación móvil desarrollada con .NET MAUI para llevar el registro de partidas competitivas entre jugadores.

## Características

- **Juegos**: crear y personalizar juegos con imagen o icono, color y descripción.
- **Sesiones**: una sesión activa por juego, con notas y victorias por jugador; al crear una nueva se cierra la anterior.
- **Jugadores**: reutilizables entre juegos, con icono y color propios.
- **Ranking**: podio de los jugadores con más victorias por juego y reordenamiento animado dentro de la sesión.
- **Temas**: modo claro y oscuro con colores por juego y por jugador adaptados a cada tema.
- **Feedback táctil**: animaciones de presión y vibración en las acciones.

## Arquitectura

Patrón **MVVM** con CommunityToolkit.MVVM y persistencia con **EF Core + SQLite**.

```
CompetitiveCounterApp.sln
├── CompetitiveCounterApp/            # App MAUI
│   ├── Pages/                        # Vistas XAML (+ Controls/ reutilizables)
│   ├── PageModels/                   # ViewModels
│   ├── Data/                         # Repositorios y ruta de la base de datos
│   ├── Services/                     # Catálogos, operaciones y manejo de errores
│   ├── Behaviors/                    # AnimatedTap, AnimatedReorder
│   ├── Helpers/ Messages/ Utilities/ Converters/ Models/
│   └── Resources/                    # Fonts, Images, Styles
└── CompetitiveCounterApp.Data/       # Entidades, AppDbContext y migraciones EF
    ├── Models/                       # Game, Session, Player, SessionPlayer
    └── Migrations/
```

## Tecnologías

- **.NET 10** con .NET MAUI (`Microsoft.Maui.Controls` 10.0.90), SDK fijado en `global.json`
- **CommunityToolkit.Mvvm** 8.3.2
- **CommunityToolkit.Maui** 15.0.1
- **Syncfusion.Maui.Toolkit** 1.0.11
- **Microsoft.EntityFrameworkCore.Sqlite** 10.0.12

## Plataformas

El producto es exclusivamente móvil y hoy compila solo para Android. iOS está previsto a futuro: `Platforms/iOS` ya existe y basta con agregar `net10.0-ios` a `TargetFrameworks`.

| Plataforma | Versión mínima | Estado    |
|------------|----------------|-----------|
| Android    | API 21 (5.0)   | Activa    |
| iOS        | 15.0           | Prevista  |

## Compilación

```powershell
dotnet build "CompetitiveCounterApp\CompetitiveCounterApp.csproj" -f net10.0-android
```

## Modelo de datos

```csharp
Game
├── ID, Name, Icon, ImagePath, Description
├── ColorLight, ColorDark
└── CreatedDate

Session
├── ID, GameID, SessionDate, Notes
├── ClosedAt (null = activa; con valor = cerrada)
└── SessionPlayers[]

Player
├── ID, Name, Icon
└── ColorLight, ColorDark

SessionPlayer
├── ID, SessionID, PlayerID
└── Wins
```

### Índices y restricciones

- `IX_Sessions_GameID_Active`: único filtrado sobre `GameID` donde `ClosedAt IS NULL` (una sola sesión activa por juego).
- `IX_SessionPlayers_SessionID_PlayerID`: único sobre `(SessionID, PlayerID)` (un jugador no se agrega dos veces a la misma sesión).
- Borrar un juego elimina en cascada sus sesiones y sus `SessionPlayers`.

`SessionDate` y `ClosedAt` se guardan en hora local del dispositivo (`LocalDateTimeConverter`).

### Migraciones

Las migraciones se aplican solas al iniciar la app (`Database.Migrate()`). Tras cambiar entidades o Fluent API en `AppDbContext`, se crean a mano:

```powershell
dotnet ef migrations add <Nombre> -p CompetitiveCounterApp.Data -s CompetitiveCounterApp
```

## Licencia

MIT License - ver el archivo LICENSE para más detalles.
