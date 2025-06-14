using Godot;
using System;

public interface ICanvas
{
	int Size { get; }
	void SetPixel(int x, int y, string color);
	string GetPixel(int x, int y);
	void DrawLine(int startX, int startY, int endX, int endY, string color, int brushSize);
	void DrawCircle(int centerX, int centerY, int radius, string color, int brushSize);
	void DrawRectangle(int centerX, int centerY, int width, int height, string color, int brushSize);
	int GetColorCount(string color, int x1, int y1, int x2, int y2);
	bool IsColor(int targetX, int targetY, string color);
	void FloodFill(int x, int y, string color);
}
