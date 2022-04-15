# Math Plotter
A mathematical plotting environment in Unity. Surface plotting via procmeshes. Later on some 2D plotting features.

The main scripts can be found in the Assets folder of the main MathPlotter folder.

Current state of the project: lexer and parser are more or less finished. This will allow to input any mathematical expression as a string and evaluate it on a given domain. Next steps are to implement the marching cubes algorithm ([https://en.wikipedia.org/wiki/Marching_cubes]) for contour surfaces. This will require a slight modification to the lexer and parser to accept equalities as well as expressions. Later down the line it will be desirable to directly compile AST's to a more low-level format to accelerate evaluation from large meshes; this could be implemented directly or make use of C#'s built-in expression tree functionality.
