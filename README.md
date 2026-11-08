# ExamForge

Aplicación multiplataforma desarrollada en **C# con .NET MAUI**, orientada a estudiantes que quieren organizar sus asignaturas, crear preguntas tipo test y preparar exámenes.

Permite gestionar cursos, asignaturas, temas y cuestionarios, consultar resultados y guardar los datos de forma persistente mediante JSON.

## Objetivo del proyecto

Desarrollar una aplicación de estudio que permita organizar contenidos y practicar mediante cuestionarios, aplicando programación orientada a objetos, arquitectura MVVM, interfaces gráficas, persistencia de datos y trabajo colaborativo con Git y GitHub.

## Integrantes

| Integrante | Responsabilidad |
|---|---|
| Santiago Barrera | Modelos, clases de datos y generador de pruebas |
| Samuel Correia | ViewModels, lógica y validaciones |
| Guzmán Fernández | Persistencia y gestión de datos |
| Ander Gago | Servicios e integración con Gemini |
| Imanol Hermosilla | Interfaces XAML y navegación |

## Funcionalidades

### Gestión de usuarios
- Registro e inicio de sesión.
- Gestión y modificación del perfil de usuario.
- Almacenamiento individual de los contenidos.

### Cursos, asignaturas y temas
- Crear, modificar y eliminar cursos.
- Gestionar asignaturas.
- Añadir temas a las asignaturas.
- Consultar y organizar el contenido académico.

### Preguntas
- Crear, modificar y eliminar preguntas tipo test.
- Establecer entre dos y cuatro opciones y una respuesta correcta.
- Añadir explicaciones a las respuestas.
- Organizar preguntas por temas.
- Consultar el banco de preguntas.

### Cuestionarios
- Generar cuestionarios a partir de uno o varios temas.
- Seleccionar la cantidad de preguntas.
- Repartir preguntas entre los temas seleccionados.
- Mezclar aleatoriamente preguntas y opciones.
- Corregir respuestas y consultar resultados.
- Visualizar aciertos, errores y porcentajes.
- Guardar el historial de cuestionarios y revisar exámenes anteriores.
- Consultar estadísticas de rendimiento.

### Generación de datos de prueba

Se ha añadido `GeneradorDemo.cs` para crear automáticamente un usuario de demostración con cursos, asignaturas, temas, preguntas y cuestionarios.

El generador comprueba si el usuario ya existe antes de crear los datos, evitando duplicados y almacenando la información mediante el sistema de persistencia.

## Arquitectura MVVM

ExamForge utiliza el patrón **MVVM (Model-View-ViewModel)** para separar los datos, la interfaz gráfica y la lógica de presentación.

| Capa | Responsabilidad |
|---|---|
| **Models** | Representa las entidades y sus relaciones. |
| **Views** | Contiene las interfaces XAML y sus archivos code-behind. |
| **ViewModels** | Gestiona el estado de las pantallas, los comandos y las validaciones. |
| **Services** | Contiene la persistencia, los cálculos y las operaciones compartidas. |

Se utiliza **CommunityToolkit.Mvvm**, con `[ObservableProperty]` y `[RelayCommand]`, para implementar propiedades observables y comandos.

La comunicación entre vistas y ViewModels se realiza mediante *Binding* y *BindingContext*. También se utiliza code-behind para eventos visuales y navegación.

## Inyección de dependencias

La aplicación utiliza **inyección de dependencias (DI)** configurada en `MauiProgram.cs`.

- **Singleton:** `DataService` mantiene una única instancia compartida durante la ejecución.
- **Transient:** las páginas y ViewModels se crean cuando son necesarios.

Esto permite compartir el estado de la aplicación y mantener separados sus componentes.

## Navegación

La navegación se gestiona mediante **Shell**, utilizando `AppShell.xaml` y `AppShell.xaml.cs`.

Las principales pantallas son:

- `PortalPage`: registro e inicio de sesión.
- `InicioPage`: gestión de cursos.
- `AsignaturasPage`: asignaturas y temas.
- `BancoPreguntasPage`: preguntas e historial.
- `NuevaPreguntaPage`: creación y edición de preguntas.
- `ConfigurarCuestionarioPage`: configuración de cuestionarios.
- `CuestionarioPage`: realización y revisión de exámenes.

Se utiliza `Shell.GoToAsync()` para navegar entre pantallas y `QueryProperty` para transmitir identificadores y otros datos necesarios.

## Persistencia

Los datos se almacenan mediante **JSON**, utilizando `DataService.cs` y `System.Text.Json`.

El archivo principal es `data.json`, guardado en el directorio privado de la aplicación mediante `FileSystem.AppDataDirectory`.

Al iniciarse la aplicación se recuperan los datos almacenados y, cuando se realizan modificaciones, se guardan nuevamente.

Esto permite conservar usuarios, cursos, asignaturas, preguntas y resultados entre ejecuciones.

## Servicios

Los principales servicios y componentes de apoyo son:

| Clase | Función |
|---|---|
| `DataService.cs` | Gestión de usuarios, estado y persistencia JSON |
| `CuestionarioEngine.cs` | Generación, selección y mezcla de preguntas |
| `Estadisticas.cs` | Cálculo de resultados y estadísticas |
| `Validar.cs` | Validación de textos y correos electrónicos |
| `PasswordHasher.cs` | Hash de contraseñas mediante SHA-256 |
| `GeneradorDemo.cs` | Creación de datos de demostración |

El motor de cuestionarios utiliza selección aleatoria y el algoritmo Fisher-Yates para mezclar las opciones y actualizar la posición de la respuesta correcta.

## Estructura del proyecto

```text
ExamForge/
├── Models/
│   ├── Usuario.cs
│   ├── Curso.cs
│   ├── Asignatura.cs
│   ├── Tema.cs
│   ├── Pregunta.cs
│   └── Cuestionario.cs
├── Views/
│   ├── Componentes/
│   └── Vistas/
├── ViewModels/
├── Services/
├── Platforms/
├── Resources/
│   ├── Images/
│   ├── Fonts/
│   ├── Styles/
│   └── Raw/
├── App.xaml
├── AppShell.xaml
├── MauiProgram.cs
├── GeneradorDemo.cs
├── ExamForge.csproj
└── ExamForge.slnx
```

- **Models:** clases de datos y relaciones entre entidades.
- **Views:** interfaces XAML y componentes visuales.
- **ViewModels:** lógica de presentación, comandos y validaciones.
- **Services:** persistencia, estadísticas y lógica compartida.
- **Platforms:** configuración específica de cada plataforma.
- **Resources:** imágenes, fuentes, estilos y datos iniciales.
- **MauiProgram.cs:** configuración de MAUI e inyección de dependencias.
- **AppShell.xaml:** navegación entre pantallas.

## Tecnologías utilizadas

- C# y .NET 10
- .NET MAUI
- XAML
- Arquitectura MVVM
- CommunityToolkit.Mvvm
- Inyección de dependencias
- Shell Navigation
- JSON y System.Text.Json
- Visual Studio 2022
- Android SDK
- Git y GitHub

Los datos se almacenan localmente en cada dispositivo.

## Organización del trabajo con Git

Cada integrante desarrolló su parte en una rama independiente.

| Rama | Contenido |
|---|---|
| `main` | Versión estable |
| `develop` | Integración del proyecto |
| `feature/modelo` | Modelos y clases de datos |
| `feature/persistencia` | Persistencia |
| `feature/viewmodels` | ViewModels y lógica |
| `feature/interfaces` | Interfaces y navegación |
| `feature/services` | Servicios e integración con Gemini |

Se utilizaron commits, push, pull, Pull Requests, merges y stashes para integrar el trabajo de los integrantes en `develop`.

## Problemas encontrados

Durante el desarrollo surgieron algunas dificultades:

- Conflictos al integrar cambios de distintas ramas.
- Adaptación de los modelos a las vistas, ViewModels y servicios.
- Configuración de la persistencia JSON.
- Conservación de los datos entre ejecuciones.
- Configuración y ejecución del emulador Android.
- Sincronización de los cambios realizados por los integrantes.

Estos problemas se fueron abordando mediante pruebas, depuración y revisión conjunta del código.

### Integración con Google Gemini

Durante el desarrollo se intentó implementar una integración con la API de Google Gemini para generar automáticamente preguntas tipo test a partir de documentos proporcionados por el usuario.

Sin embargo, no se consiguió completar su funcionamiento debido a varios problemas:

- Dificultades para cargar documentos y acceder a su contenido.
- Errores relacionados con la autenticación y configuración de la clave API.
- Problemas de conexión con el servicio de Google Gemini.
- Errores de código y configuración durante la integración.

Aunque se consiguió avanzar en la implementación de la interfaz y la selección de documentos, **la generación automática de preguntas mediante IA quedó pendiente de completar**.

El resto de funcionalidades de ExamForge funciona de manera independiente de este servicio.
