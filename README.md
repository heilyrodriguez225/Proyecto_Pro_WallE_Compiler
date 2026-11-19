# Pixel WallE Compiler

Intérprete de un lenguaje de dominio específico para dibujar sobre un lienzo de píxeles. El programa controla a **WallE**, un robot que se desplaza por el lienzo y pinta con un pincel de color y grosor configurables.

Proyecto de la asignatura **Programación** — Ciencias de la Computación, Universidad de La Habana.

## Características

- **Analizador léxico** que reconoce funciones, etiquetas, números, cadenas, operadores y símbolos.
- **Analizador sintáctico** de descenso recursivo que construye un árbol de sintaxis abstracta (AST).
- **Intérprete** con ámbito de variables, expresiones aritméticas y booleanas, y saltos condicionales.
- **Interfaz gráfica** desarrollada en Godot (C#): editor de código, lienzo redimensionable, visualización de errores e importación y exportación de programas.

## El lenguaje

Todo programa comienza con `Spawn`, que sitúa a WallE en el lienzo. A partir de ahí admite instrucciones de dibujo, asignaciones, etiquetas y saltos condicionales.

```
Spawn(0, 0)
Color("Red")
Size(3)
i <- 0
bucle
DrawLine(1, 1, 5)
i <- i + 1
GoTo [bucle] (i < 5)
```

### Instrucciones

| Instrucción | Descripción |
|---|---|
| `Spawn(x, y)` | Posición inicial de WallE |
| `Color(color)` | Color del pincel: `Red`, `Blue`, `Green`, `Yellow`, `Orange`, `Purple`, `Black`, `White`, `Transparent` |
| `Size(n)` | Grosor del pincel (se ajusta a un valor impar) |
| `DrawLine(dirX, dirY, distancia)` | Línea en la dirección indicada |
| `DrawCircle(dirX, dirY, radio)` | Circunferencia |
| `DrawRectangle(dirX, dirY, distancia, ancho, alto)` | Rectángulo |
| `Fill()` | Relleno del área contigua del mismo color |

### Funciones

`GetActualX()`, `GetActualY()`, `GetCanvasSize()`, `GetColorCount(...)`, `IsBrushColor(color)`, `IsBrushSize(n)`, `IsCanvasColor(...)`

### Expresiones

- Aritméticas: `+`, `-`, `*`, `/`, `**`, `%`
- Comparación: `==`, `>=`, `<=`, `>`, `<`
- Lógicas: `&&`, `||`
- Asignación: `variable <- expresión`
- Salto condicional: `GoTo [etiqueta] (condición)`

## Estructura

```
Compiler/
├── Lexer/        Analizador léxico y tokens
├── Parser/       Analizador sintáctico, AST, ámbito y funciones integradas
├── Interpreter.cs
└── ICanvas.cs
UI/
├── EditorScene/  Escena principal de la interfaz
├── CodeEditor.cs
└── PixelCanvas.cs
```

## Tecnologías

- C#
- Godot Engine (Mono)
