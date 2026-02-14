# CompetitiveCounterApp

![.NET MAUI](https://img.shields.io/badge/.NET_MAUI-9-512BD4?logo=.net)
![Platform](https://img.shields.io/badge/Platform-Android_|_iOS_|_macOS_|_Windows-blue)

Una aplicación móvil multiplataforma desarrollada con .NET MAUI para llevar el registro de partidas competitivas entre jugadores.

## Características

- **Gestión de Juegos**: Crear y personalizar juegos con iconos, colores y descripciones
- **Gestión de Sesiones**: Registro de partidas con fecha, notas y victorias por jugador
- **Gestión de Jugadores**: Sistema de jugadores reutilizable entre diferentes juegos
- **Temas**: Soporte completo para modo claro y oscuro con colores personalizados
- **UI Moderna**: Interfaz con FluentUI Icons y diseño responsivo

## Arquitectura

El proyecto utiliza el patrón **MVVM** con CommunityToolkit.MVVM:

```
CompetitiveCounterApp/
├── Models/                    # Entidades de dominio
│   ├── Game.cs
│   ├── Session.cs
│   ├── Player.cs
│   └── SessionPlayer.cs
├── Pages/                     # Vistas XAML
│   ├── GamesPage.xaml
│   ├── GameDetailPage.xaml
│   ├── CreateGamePage.xaml
│   ├── EditGamePage.xaml
│   └── Controls/
├── PageModels/                # ViewModels
│   ├── GamesPageModel.cs
│   ├── GameDetailPageModel.cs
│   ├── CreateGamePageModel.cs
│   └── EditGamePageModel.cs
├── Data/                      # Repositorios SQLite
│   ├── GameRepository.cs
│   ├── SessionRepository.cs
│   └── PlayerRepository.cs
├── Services/                  # Servicios de la aplicación
└── Resources/                 # Recursos (Fonts, Images, Styles)
```

## Tecnologías

- **.NET 9** con .NET MAUI
- **CommunityToolkit.MVVM** (v8.3.2)
- **CommunityToolkit.Maui** (v11.1.1)
- **Syncfusion.Maui.Toolkit** (v1.0.6)
- **SQLite** (Microsoft.Data.Sqlite.Core v8.0.8)

## Plataformas Soportadas

| Plataforma | Versión Mínima |
|------------|----------------|
| Android    | API 21 (5.0)   |
| iOS        | 15.0           |
| macOS      | 15.0           |
| Windows    | 10.0.17763.0   |


## Modelo de Datos (Sujeto a cambios)

```csharp
Game
├── ID, Name, Icon, Description
├── ColorLight, ColorDark
└── CreatedDate

Session
├── ID, GameID, SessionDate, Notes
└── SessionPlayers[]

Player
├── ID, Name
└── ColorHex

SessionPlayer
├── ID, SessionID, PlayerID
└── Wins
```

## Licencia

MIT License - ver el archivo LICENSE para más detalles.
