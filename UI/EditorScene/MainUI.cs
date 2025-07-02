using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace NuevoProyectoPixelWallE.Scripts.UI.EditorScene
{
    public class MainUI : Control
    {
        [Export] public TextEdit _editor;
        [Export] public Button _runBtn;
        [Export] public TextureRect _canvasDisplay;
        private RichTextLabel _errorDisplay;
        private Button _importBtn, _exportBtn, _resizeBtn;
        private FileDialog _importFLD, _exportFLD;
        private AcceptDialog _resizeDLg;
        private SpinBox _size;
        private Interpreter compiler;
        public override void _Ready()
        {
            _editor = GetNode<TextEdit>("Editor");
            _runBtn = GetNode<Button>("RunButton");
            _canvasDisplay = GetNode<TextureRect>("Canvas");
            _errorDisplay = GetNode<RichTextLabel>("ErrorDisplay");
            _importBtn = GetNode<Button>("Import");
            _exportBtn = GetNode<Button>("Export");
            _importFLD = GetNode<FileDialog>("FDImport");
            _exportFLD = GetNode<FileDialog>("FDExport");
            _resizeDLg = GetNode<AcceptDialog>("ResizeAlarm");
            _resizeBtn = GetNode<Button>("Resize");
            _size = GetNode<SpinBox>("ResizeBox");
            _runBtn.Connect("pressed", this, nameof(OnRunPressed));
            _resizeBtn.Connect("pressed", this, nameof(OnResizePressed));
            _importBtn.Connect("pressed", this, nameof(OnImportPressed));
            _exportBtn.Connect("pressed", this, nameof(OnExportPressed));
            _importFLD.Connect("file_selected", this, nameof(OnFileImportSelected));
            _exportFLD.Connect("file_selected", this, nameof(OnFileExportSelected));
            _resizeDLg.Connect("confirmed", this, nameof(OnResizeConfirmed));

            compiler = new Interpreter((int)_size.Value);
        }
        private void OnRunPressed()
        {
            _errorDisplay.Clear();
            string code = _editor.Text;
            Color[,] resultMatrix = compiler.Execute(code);
            #region Errors Management
            StringBuilder sb = new StringBuilder();
            foreach (var err in Interpreter.Error)
            {
                sb.AppendLine($". [color=red]{err.Message}[/color]");
            }
            _errorDisplay.BbcodeEnabled = true;
            _errorDisplay.BbcodeText = sb.ToString();
            #endregion
            var cellSize = CellSizeDefinition(resultMatrix);
            if (cellSize == -1) return;
            //// Genera la textura y la asigna al TextureRect
            var tex = MatrixToTexture(resultMatrix, cellSize);
            _canvasDisplay.Texture = tex;
        }
        // Convierte matriz de colores en ImageTexture
        private ImageTexture MatrixToTexture(Color[,] matrix, int cellSize = 1)
        {
            int rows = matrix.GetLength(0);
            int cols = rows > 0 ? matrix.GetLength(1) : 0;
            var img = new Image();
            img.Create(cols * cellSize, rows * cellSize, false, Image.Format.Rgba8);
            img.Lock();
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    Color c = matrix[y, x];
                    if (cellSize == 1)
                    {
                        img.SetPixel(x, y, c);
                    }
                    else
                    {
                        for (int dy = 0; dy < cellSize; dy++)
                            for (int dx = 0; dx < cellSize; dx++)
                                img.SetPixel(x * cellSize + dx, y * cellSize + dy, c);
                    }
                }
            }
            img.Unlock();
            var tex = new ImageTexture();
            tex.CreateFromImage(img);
            return tex;
        }
        private void OnResizePressed()
        {
            _resizeDLg.PopupCenteredRatio(0.3f);
        }
        private void OnImportPressed()
        {
            _importFLD.PopupCenteredRatio(0.5f);
        }
        private void OnExportPressed()
        {
            _exportFLD.PopupCenteredRatio(0.5f);
        }
        private void OnFileImportSelected(string path)
        {
            var file = new Godot.File();
            if (file.Open(path, Godot.File.ModeFlags.Read) == Error.Ok)
            {
                string contenido = file.GetAsText();
                file.Close();
                _editor.Text = contenido;
            }
            OnRunPressed();
        }
        private void OnFileExportSelected(string path)
        {
            var file = new Godot.File();
            if (file.Open(path, Godot.File.ModeFlags.Write) == Error.Ok)
            {
                file.StoreString(_editor.Text);
                file.Close();
            }
        }
        private void OnResizeConfirmed()
        {
            var size = Convert.ToInt32(_size.Value);
            compiler = new Interpreter(size);
            var m = new Color[size, size];
            var tex =MatrixToTexture(m,CellSizeDefinition(m));
            _canvasDisplay.Texture = tex;
        }
        private int CellSizeDefinition(Color[,] resultMatrix)
        {
            int cols = resultMatrix.GetLength(0);
            int rows = resultMatrix.GetLength(1);
            Vector2 area = _canvasDisplay.RectSize;
            if (cols == 0 || rows == 0 || area.x <= 0 || area.y <= 0)
                return -1;

            int cellSizeX = (int)(area.x / cols);
            int cellSizeY = (int)(area.y / rows);
            int cellSize = Mathf.Max(1, Mathf.Min(cellSizeX, cellSizeY));
            return cellSize;
        }
    }
}