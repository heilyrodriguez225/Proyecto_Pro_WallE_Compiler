using Godot;
using System;
using System.Collections.Generic; 

public static class Functions
{
	public static List<string> ValidColors { get; } = new List<string>
	{
		"Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Black", "White", "Transparent"
	};
	public static Dictionary<string, Func<List<object>, Scope, object>> FunctionMap =
		new Dictionary<string, Func<List<object>,Scope, object>>
	{
		// Color(string color) 
		{
			"Color", (args, scope) =>
			{
				if (args.Count != 1) Interpreter.Error.Add(new Exception("Color requiere 1 parámetro string"));
				string color = Convert.ToString(args[0]);
				if (!ValidColors.Contains(color)) Interpreter.Error.Add(new Exception($"Color no válido: {color}"));
				var state = GetWallEState(scope);
				state.CurrentColor = color;
				return null;
			}
		},
		// Size(int size) 
		{
			"Size", (args, scope) =>
			{
				if (args.Count != 1) Interpreter.Error.Add(new Exception("Size requiere 1 parámetro entero"));
				int size = Convert.ToInt32(args[0]); 
				var state = GetWallEState(scope);
				
				// Ajustar tamaño impar (si es par, restar 1)
				if (size % 2 == 0) size--;
				if (size < 1) size = 1;  // Mínimo 1
				
				state.BrushSize = size;
				return null;
			}
		},
		// DrawLine(int dirX, int dirY, int distance) 
		{
			"DrawLine", (args, scope) =>
			{
				if (args.Count != 3) Interpreter.Error.Add(new Exception("DrawLine requiere 3 parámetros enteros"));
				int dirX = Convert.ToInt32(args[0]);
				int dirY = Convert.ToInt32(args[1]);
				int distance = Convert.ToInt32(args[2]);

				var state = GetWallEState(scope);
				int startX = state.X;
				int startY = state.Y;
				int endX = startX + dirX * distance;
				int endY = startY + dirY * distance;

				state.PixelCanvas.DrawLine(startX, startY, endX, endY, state.CurrentColor, state.BrushSize);
				state.X = endX;
				state.Y = endY;
				return null;
			}
		},
		// DrawCircle(int dirX, int dirY, int radius)
		{
			"DrawCircle", (args, scope) =>
			{
				if (args.Count != 3 || !(args[0] is int) || !(args[1] is int) || !(args[2] is int))
					Interpreter.Error.Add(new Exception("DrawCircle requiere 3 parámetros enteros"));
				int dirX = Convert.ToInt32(args[0]);
				int dirY = Convert.ToInt32(args[1]);
				int radius = Convert.ToInt32(args[2]);

				var state = GetWallEState(scope);
				int centerX = state.X + dirX * radius;
				int centerY = state.Y + dirY * radius;

				state.PixelCanvas.DrawCircle(centerX, centerY, radius, state.CurrentColor, state.BrushSize);
				state.X = centerX;
				state.Y = centerY;
				return null;
			}
		},
		//DrawRectangle(int dirX, int dirY, int distance, int width, int height)
		{
			"DrawRectangle", (args, scope) =>
			{
				if (args.Count != 5 || !(args[0] is int) || !(args[1] is int) || !(args[2] is int) || !(args[3] is int) || !(args[4] is int))
					Interpreter.Error.Add(new Exception("DrawRectangle requiere 5 parámetros enteros"));

				int dirX = Convert.ToInt32(args[0]);
				int dirY = Convert.ToInt32(args[1]);
				int distance = Convert.ToInt32(args[2]);
				int width = Convert.ToInt32(args[3]);
				int height = Convert.ToInt32(args[4]);
				var state = GetWallEState(scope);

				int centerX = state.X + dirX * distance;
				int centerY = state.Y + dirY * distance;
				state.PixelCanvas.DrawRectangle(centerX, centerY, width, height, state.CurrentColor, state.BrushSize);
				state.X = centerX;
				state.Y = centerY;
				return null;
			}
		},
		// Fill()
				{
			"Fill", (args, scope) =>
			{
				if (args.Count != 0) Interpreter.Error.Add(new Exception("Fill no requiere parametros"));
				var state = GetWallEState(scope);
				state.PixelCanvas.FloodFill(state.X, state.Y, state.CurrentColor);
				return null;
			}
		},
		// GetActualX()
			{
			"GetActualX", (args, scope) =>
			{
				if (args.Count != 0) Interpreter.Error.Add(new Exception("GetActualX no requiere parametros"));
				return GetWallEState(scope).X;
			}
		},
		// GetActualY()
		{
			"GetActualY", (args, scope) =>
			{
				if (args.Count != 0) Interpreter.Error.Add(new Exception("GetActualY no requiere parametros"));
				return GetWallEState(scope).Y;
			}
		},
		//GetCanvasSize()
		{
			"GetCanvasSize", (args, scope) =>
			{
				if (args.Count != 0) Interpreter.Error.Add(new Exception("GetCanvasSize no requiere parámetros"));
				return GetWallEState(scope).PixelCanvas.Size;
			}
		},
		//GetColorCount (string color, int x1, int y1, int x2, int y2)
		{
			"GetColorCount", (args, scope) =>
			{
				if (args.Count != 5 || !(args[0] is string) || !(args[1] is int) || !(args[2] is int) || !(args[3] is int) || !(args[4] is int))
					Interpreter.Error.Add(new Exception("GetColorCount requiere 1 string y 4 enter"));
				string color = (string)args[0];
				int x1 = Convert.ToInt32(args[1]);
				int y1 = Convert.ToInt32(args[2]);
				int x2 = Convert.ToInt32(args[3]);
				int y2 = Convert.ToInt32(args[4]);
				var state = GetWallEState(scope);
				return state.PixelCanvas.GetColorCount(color, x1, y1, x2, y2);
			}
		},
		// IsBrushColor(string color) 
			{
			"IsBrushColor", (args, scope) =>
			{
				if (args.Count != 1) Interpreter.Error.Add(new Exception("IsBrushColor requiere 1 parámetro string"));
				string color = Convert.ToString(args[0]);
				var state = GetWallEState(scope);
				if (state.CurrentColor == color) return 1;
				else return 0;
			}
		},
		// IsBrushSize(int size) 
		{
			"IsBrushSize", (args, scope) =>
			{
				if (args.Count != 1) Interpreter.Error.Add(new Exception("IsBrushSize requiere 1 parámetro entero"));
				int size = Convert.ToInt32(args[0]); 
				var state = GetWallEState(scope);
				if (state.BrushSize == size) return 1;
				else return 0;
			}
		},
		// IsCanvasColor
		{
			"IsCanvasColor", (args, scope) =>
			{
				if (args.Count != 3) Interpreter.Error.Add(new Exception("IsCanvasColor requiere 3 parámetros"));
				string color = Convert.ToString(args[0]);
				int vertical = Convert.ToInt32(args[1]);
				int horizontal = Convert.ToInt32(args[2]);

				var state = GetWallEState(scope);
				int targetX = state.X + horizontal;
				int targetY = state.Y + vertical;

				return state.PixelCanvas.IsColor(targetX, targetY, color) ? 1 : 0;
			}
		}
	};
	private static WallEState GetWallEState(Scope scope)
	{
		var res = scope.GetVariable("WallEState");
		if(!(res is WallEState))
			Interpreter.Error.Add(new Exception("Estado de Wall-E no inicializado"));
		return (WallEState)res;
	}
}
