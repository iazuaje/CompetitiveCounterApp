# CompetitiveCounterApp

App .NET MAUI 9 (MVVM con CommunityToolkit.MVVM, SQLite) para registrar partidas competitivas.

## Mobile-first

El producto es exclusivamente móvil. Windows y macOS existen en `TargetFrameworks` solo como conveniencia de desarrollo, no son destinos de entrega.

- Toda decisión de diseño y UX se razona sobre una pantalla de teléfono: alto útil reducido, interacción táctil, contenido que debe poder desplazarse.
- La compilación que valida un cambio es la de Android:

```powershell
dotnet build "CompetitiveCounterApp\CompetitiveCounterApp.csproj" -f net9.0-android
```

Compilar solo para Windows no es evidencia suficiente de que un cambio esté correcto; sirve como verificación rápida de C#/XAML, pero el resultado debe confirmarse en Android.

## Layout de las pantallas de juegos

`CreateGamePage`, `EditGamePage` y `GameDetailPage` comparten un patrón de encabezado con imagen más panel inferior redondeado:

```xml
<Grid RowDefinitions="0.4*, *">
```

Ambas filas son proporcionales a propósito. Con la fila inferior en `auto` el panel reclama el alto de su contenido y en teléfonos de pantalla corta comprime el encabezado hasta hacerlo desaparecer, además de dar un tamaño de imagen distinto en cada pantalla según cuánto contenido tenga. El panel inferior siempre lleva un `ScrollView`.

La imagen del encabezado usa `Aspect="AspectFill"` sin `HeightRequest`: rellena la fila y se recorta.

## Convenciones

- Los formularios de crear y editar heredan de `GameFormPageModelBase`; la lógica compartida (imagen temporal, selección de icono y color, validación) vive ahí y no se duplica en las clases derivadas.
- Las imágenes de juego se guardan en `AppDataDirectory/GameImages` y se mueven desde caché al guardar con `MoveTemporaryImageToPermanent()`.
- Colores y superficies usan `AppThemeBinding` para respetar modo claro y oscuro; no se fijan colores literales como `White` en fondos.
- Los textos de la interfaz están en español, con tildes.
