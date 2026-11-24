# ProjecteKahoot
Projecte II · Aplicación de concurso para escritorio. Menú principal **Kahoot** con el logotipo facilitado.

## Abrir y jugar
1. Extrae el ZIP. En Unity Hub elige **Add → Add project from disk** y selecciona la carpeta **ProjecteKahoot** que contiene Assets, Packages y ProjectSettings.
2. Usa **Unity 6000.3.14f1** (la versión de tu captura). En el primer inicio espera a que Unity resuelva el paquete uGUI.
3. Abre **Assets/Scenes/MenuPrincipal.unity** y pulsa **Play**. Cada escena contiene un Canvas guardado con sus objetos UI. Despliega Canvas → Escenario para editar la cabecera y FormularioScroll → Viewport → Content para ver tarjetas, campos y botones. El menú ProjecteKahoot → Ver interfaz en Hierarchy selecciona un control. Los campos de datos y las listas variables se enlazan en Play. Las posiciones del menú principal se editan con Rect Transform. Durante la ejecución los formularios se distribuyen con grupos de layout. El proyecto es independiente del anterior.
4. Usa ratón, flechas o cruceta/stick y botón A/Intro. Tab y Mayús+Tab cambian el foco. En las preguntas, las teclas 1–9 eligen la opción correspondiente. El reloj cambia de color al entrar en el último cuarto del tiempo. Usa el teclado para introducir texto. Ajustes conserva volumen e idioma. Las preguntas conservan su idioma de autor.

## Funciones y archivos
El selector combina los JSON de Assets/Resources/Quizzes y los JSON de Cuestionarios dentro de Application.persistentDataPath. Se recarga en cada entrada. El creador permite nuevas preguntas y respuestas, marcar la correcta y elegir de 1 a 600 segundos por pregunta. El botón de ejemplo guarda directamente un cuestionario de diez preguntas. Los dos ejemplos incluidos son originales.
Cada acierto suma 100 + redondeo(900 × fracción de tiempo restante). Un fallo o tiempo agotado suma cero. El XML conserva nombre, puntos y segundos. Orden de clasificación: puntos descendentes y tiempo ascendente. El código diferencia las claves de cuestionarios incluidos y creados.
En Windows los datos se guardan normalmente en %USERPROFILE%/AppData/LocalLow/ProjecteII/ProjecteKahoot. Subcarpetas: Cuestionarios, Resultados e Informes. El lector muestra un ranking vacío si falta el XML. Los errores de formato o acceso quedan registrados cuando el sistema permite escribir. Un XML corrupto se conserva para su recuperación. En el JSON, correct es la posición de la opción correcta empezando en 1. El nombre del archivo identifica su clasificación.
Los ajustes afectan a la interfaz (español, catalán, inglés) y al volumen de los efectos originales desde silencio hasta 100 %.

## Entrega y build
Docs/Memoria.pdf contiene los mockups, la matriz de la rúbrica, fragmentos de código y las pruebas propuestas. Docs/Navegacion.drawio es el diagrama editable.
Con el módulo **Windows Build Support** adecuado para tu editor, selecciona **ProjecteKahoot → Build Windows x64**. Se generará Builds/Windows/ProjecteKahoot.exe con sus carpetas auxiliares. Para la entrega, comprime la carpeta Windows completa. Abre el ejecutable y comprueba las pruebas de la memoria.
Publica el proyecto en tu propio repositorio de GitHub conservando .gitignore. Crea una release y adjunta el ZIP de la build. Copia el enlace real del repositorio en la entrega. El archivo entregado aquí contiene el código fuente y la documentación; la compilación y publicación requieren tu editor y tu cuenta.
