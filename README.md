# CompetitiveCounterApp

![.NET MAUI](https://img.shields.io/badge/.NET_MAUI-9-512BD4?logo=.net)
![Platform](https://img.shields.io/badge/Platform-Android_|_iOS_|_macOS_|_Windows-blue)

Una aplicación móvil multiplataforma desarrollada con .NET MAUI para llevar el registro de partidas competitivas entre jugadores. Perfecta para hacer seguimiento de victorias y derrotas en juegos competitivos con amigos y familiares.

## ?? Características

### Gestión de Juegos
- **Crear y personalizar juegos** con nombre, icono, descripción y colores personalizados (tema claro/oscuro)
- **Vista en cuadrícula** de todos los juegos registrados
- **Detalles de juego** con historial de sesiones y estadísticas
- **Edición y eliminación** de juegos

### Gestión de Sesiones
- Registro de sesiones de juego con fecha y notas
- Seguimiento de victorias por jugador en cada sesión
- Historial completo de sesiones por juego
- Visualización de datos agregados

### Gestión de Jugadores
- Crear jugadores con nombres y colores personalizados
- Sistema de jugadores reutilizable entre diferentes juegos
- Estadísticas por jugador

### Características Adicionales
- **Tema claro/oscuro** con soporte completo
- **Colores personalizados** por juego adaptables al tema
- **Iconos FluentUI** para una interfaz moderna
- **Diseño responsivo** para diferentes tamaños de pantalla
- **Actualización pull-to-refresh** en las listas

## ??? Arquitectura

### Patrón MVVM
El proyecto utiliza el patrón **Model-View-ViewModel** con:
- **Models**: Entidades de dominio (Game, Session, Player, SessionPlayer)
- **Views (Pages)**: Interfaces de usuario en XAML
- **ViewModels (PageModels)**: Lógica de presentación con CommunityToolkit.MVVM

### Estructura del Proyecto

```
CompetitiveCounterApp/
??? Models/                    # Entidades de dominio
?   ??? Game.cs               # Juego competitivo
?   ??? Session.cs            # Sesión de juego
?   ??? Player.cs             # Jugador
?   ??? SessionPlayer.cs      # Relación sesión-jugador con victorias
??? Pages/                     # Vistas XAML
?   ??? GamesPage.xaml        # Lista principal de juegos
?   ??? GameDetailPage.xaml   # Detalles y sesiones de un juego
?   ??? CreateGamePage.xaml   # Crear nuevo juego
?   ??? EditGamePage.xaml     # Editar juego existente
?   ??? Controls/             # Controles reutilizables
??? PageModels/               # ViewModels
?   ??? GamesPageModel.cs
?   ??? GameDetailPageModel.cs
?   ??? CreateGamePageModel.cs
?   ??? EditGamePageModel.cs
??? Data/                      # Capa de datos
?   ??? GameRepository.cs     # Repositorio de juegos
?   ??? SessionRepository.cs  # Repositorio de sesiones
?   ??? PlayerRepository.cs   # Repositorio de jugadores
?   ??? Constants.cs          # Constantes y paths de BD
??? Services/                  # Servicios de la aplicación
??? Converters/               # Conversores XAML
??? Resources/                # Recursos de la app
    ??? Fonts/
    ??? Images/
    ??? Styles/
```

## ??? Tecnologías

### Frameworks y Librerías
- **.NET 9** - Framework principal
- **.NET MAUI** - Framework multiplataforma
- **CommunityToolkit.MVVM** (v8.3.2) - Implementación MVVM
- **CommunityToolkit.Maui** (v11.1.1) - Controles y helpers adicionales
- **Syncfusion.Maui.Toolkit** (v1.0.6) - Componentes UI avanzados

### Base de Datos
- **SQLite** (Microsoft.Data.Sqlite.Core v8.0.8)
- **SQLitePCLRaw.bundle_green** (v2.1.10)

### Características de .NET MAUI Utilizadas
- Shell Navigation
- Data Binding
- Dependency Injection
- Platform-specific code
- XAML Markup Extensions

## ?? Plataformas Soportadas

| Plataforma | Versión Mínima |
|------------|----------------|
| Android    | API 21 (5.0)   |
| iOS        | 15.0           |
| macOS      | 15.0           |
| Windows    | 10.0.17763.0   |

## ?? Cómo Ejecutar

### Requisitos Previos
- Visual Studio 2022 (v17.12 o superior)
- Workload de .NET MAUI instalado
- SDK de .NET 9

### Pasos de Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/iazuaje/CompetitiveCounterApp.git
   cd CompetitiveCounterApp
   ```

2. **Restaurar paquetes NuGet**
   ```bash
   dotnet restore
   ```

3. **Ejecutar la aplicación**
   ```bash
   dotnet build
   dotnet run
   ```

   O simplemente abrir en Visual Studio y presionar F5

## ?? Modelo de Datos

### Game (Juego)
```csharp
- ID: int
- Name: string
- Icon: string (glyph de FluentUI)
- Description: string
- ColorLight: string (hex)
- ColorDark: string (hex)
- CreatedDate: DateTime
```

### Session (Sesión)
```csharp
- ID: int
- GameID: int
- SessionDate: DateTime
- Notes: string
- SessionPlayers: List<SessionPlayer>
```

### Player (Jugador)
```csharp
- ID: int
- Name: string
- ColorHex: string
```

### SessionPlayer (Relación)
```csharp
- ID: int
- SessionID: int
- PlayerID: int
- Wins: int (victorias en esa sesión)
```

## ?? Diseño

- **Tema**: Soporte completo para modo claro y oscuro
- **Colores**: Sistema de colores personalizados por juego
- **Iconos**: FluentUI System Icons
- **Fuentes**: Open Sans, Segoe UI Semibold
- **Layout**: Diseño responsivo con grids y colecciones

## ?? Gestión de Estado

- **Dependency Injection** para repositorios y servicios
- **Observable Properties** con CommunityToolkit.MVVM
- **Commands** para interacciones del usuario
- **Shell Navigation** para navegación entre páginas

## ?? Próximas Mejoras

- [ ] Estadísticas avanzadas por jugador
- [ ] Gráficos de rendimiento
- [ ] Exportación de datos
- [ ] Sincronización en la nube
- [ ] Notificaciones de recordatorios
- [ ] Soporte para torneos

## ????? Autor

**iazuaje**
- GitHub: [@iazuaje](https://github.com/iazuaje)

## ?? Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo LICENSE para más detalles.

---

? Si te gusta este proyecto, ¡dale una estrella en GitHub!
