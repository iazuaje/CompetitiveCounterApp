# CompetitiveCounterApp

App .NET MAUI 10 (MVVM con CommunityToolkit.MVVM, EF Core + SQLite) para registrar partidas competitivas. Dos proyectos: la app MAUI y `CompetitiveCounterApp.Data` (entidades, `AppDbContext`, migraciones). No hay tests.

## Mobile-first

El producto es exclusivamente móvil. El único target es `net10.0-android`; iOS está previsto a futuro (`Platforms/iOS` se conserva, falta agregar `net10.0-ios` al `.csproj`). No reintroducir Windows, Mac Catalyst ni Tizen.

- Toda decisión de diseño y UX se razona sobre una pantalla de teléfono: alto útil reducido, interacción táctil, contenido desplazable.
- No compilar tras pasos intermedios ni cambios parciales. Compilar solo al **final de un plan completo** (o si el usuario lo pide), con Android Debug:

```powershell
dotnet build "CompetitiveCounterApp\CompetitiveCounterApp.csproj" -f net10.0-android
```

## Layout de las pantallas de juegos

`CreateGamePage`, `EditGamePage` y `GameDetailPage` usan encabezado con imagen + panel inferior redondeado con `<Grid RowDefinitions="0.4*, *">`. Ambas filas son proporcionales a propósito: con la inferior en `auto`, en teléfonos cortos el panel aplasta el encabezado y la imagen cambia de tamaño entre pantallas. El panel inferior siempre lleva `ScrollView`; la imagen usa `Aspect="AspectFill"` sin `HeightRequest`.

## Convenciones

- Page models: `[ObservableProperty]` sobre campos `_camelCase`, `[RelayCommand]`, propiedades derivadas notificadas desde `partial void OnXChanged`.
- Formularios de juego heredan de `GameFormPageModelBase` y de jugador de `PlayerFormPageModelBase`; la lógica compartida vive ahí y no se duplica.
- Imágenes de juego en `AppDataDirectory/GameImages`, movidas desde caché con `MoveTemporaryImageToPermanent()`.
- Páginas nuevas: ruta con `AddTransientWithShellRoute` en `MauiProgram.cs` y entrada `<MauiXaml Update=...>` en el `.csproj`. Parámetros por query string (`?id=`) vía `IQueryAttributable`; recarga con `EventToCommandBehavior` → `AppearingCommand`.
- Errores: `_errorHandler.HandleError(e)`; tareas sin `await` con `.FireAndForgetSafeAsync(_errorHandler)`. Avisos con `AppShell.DisplayToastAsync`.
- `GamesPage` está en el archivo `GamePage.xaml`.
- Textos de la interfaz en español, con tildes. Archivos fuente en UTF-8.

## Tema y colores

- Colores y superficies con `AppThemeBinding`; nada de colores literales como `White` en fondos.
- Colores por entidad como par hex `ColorLight` / `ColorDark`, resueltos con `ThemeColorPair`. `Player` sigue el mismo patrón que `Game`.
- Nunca `White` fijo sobre un color dinámico: en oscuro los colores son pasteles claros. Texto e iconos encima usan `OnColor` / `OnGameColor` (sobre el color), `OnToolbarColor` (sobre `ToolbarColor`) o `ThemeColorPair.ContrastingTextColor(...)`. Sobre `Primary`/`PrimaryDark` fijos: `AppThemeBinding Light=White, Dark=PrimaryDarkText`.
- Al cambiar el tema, `App` envía `AppThemeChangedMessage`; todo page model con colores calculados se registra y llama `NotifyThemeChanged()` en sus modelos.

## Feedback táctil

- **Acción clara**: `Button` nativo, `Pressed` suave (`Scale` 0.96, `Opacity` 0.92), `BorderWidth="0"`.
- **Card que no puede ser Button**: `behaviors:AnimatedTap.Command` sobre el `Border`, con el contenido `InputTransparent="True"`. No `TouchBehavior` ni `TapGestureRecognizer.Command` (navega antes de animar).
- **Listas con pulso**: `ScrollView` + `BindableLayout`. `CollectionView` solo si el ítem no se anima (en Android recicla celdas y pierde el comando o el pulso).
- **Grilla de 2 columnas**: `FlexLayout` con `JustifyContent="SpaceBetween"` y `FlexLayout.Basis="48%"`, solo margen inferior.
- **Háptica**: `HapticFeedbackHelper.Click()` al inicio de cada comando de usuario.

## Datos y sesiones

- Repositorios con `IDbContextFactory`: un contexto por operación, lecturas `AsNoTracking()`. Las reglas de negocio de sesión viven en `SessionRepository` y lanzan `InvalidOperationException` con mensaje en español.
- `Session.ClosedAt`: `null` = activa. Máximo una activa por `GameID` (`IX_Sessions_GameID_Active`). `SessionPlayer` único por `(SessionID, PlayerID)`; métrica solo `Wins`, nunca menor a 0.
- Fechas en hora local (`DateTime.Now`, `LocalDateTimeConverter`), sin UTC.
- Toda propiedad calculada nueva en una entidad va en `entity.Ignore(...)` de `AppDbContext`.
- No generar ni editar migraciones EF: tras cambiar entidades o Fluent API, el desarrollador crea la migración a mano.

## Commits

Nunca crear commits ni ejecutar `git add`/`git commit`; si piden un mensaje, solo redactarlo. En inglés, conventional commits (`feat:`, `fix:`, `refactor:`, `docs:`, `chore:`), título ≤ 72 caracteres en minúscula y sin punto final, y descripción de una o dos frases (≤ 150 caracteres) sobre el porqué, sin listar archivos.
