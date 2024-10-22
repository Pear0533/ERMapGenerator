using System.Globalization;
using System.Xml;
using DdsFileTypePlus;
using ImageMagick;
using PaintDotNet;
using SoulsFormats;

namespace ERMapGenerator;

public partial class ERMapGenerator : Form
{
    private const string version = "1.2";
    private const float mapDisplayMaxZoomLevel = 2.0f;
    private const float mapDisplayZoomIncrement = 0.1f;
    private static string gameModFolderPath = "";
    private static string mapTileMaskBndPath = "";
    private static string mapTileTpfBhdPath = "";
    private static string mapTileTpfBtdPath = "";
    private static string outputFolderPath = "";
    private static BND4 mapTileMaskBnd = new();
    private static BXF4 mapTileTpfBhd = new();
    private static Matrix Flags = new();
    private static XmlNode? MapTileMaskRoot;
    private static bool isDraggingMapDisplay;
    private static int mapDisplayXPos;
    private static int mapDisplayYPos;
    private bool isBusy;
    private float mapDisplayMinZoomLevel = -1;
    private float mapDisplayZoomLevel;
    private string mapImageFilePath = "";
    private Bitmap savedMapImage = null!;

    public ERMapGenerator()
    {
        InitializeComponent();
        SetVersionString();
        CenterToScreen();
        RegisterFormEvents();
    }

    private void RegisterFormEvents()
    {
        mapDisplayPictureBox.MouseWheel += MapDisplayPictureBox_MouseWheel;
    }

    private void SetVersionString()
    {
        versionStr.Text += $@" {version}";
    }

    private static string[] GetAllFolderFiles(string folderPath, string fileType = "*.*")
    {
        try
        {
            return Directory.GetFiles(folderPath, fileType, SearchOption.AllDirectories);
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }

    private static void ShowInformationDialog(string str)
    {
        MessageBox.Show(str, @"Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private static bool ResourceExists(string path, string dispName)
    {
        bool doesExist = Path.HasExtension(path) && File.Exists(path) || Directory.Exists(path);
        if (!doesExist) ShowInformationDialog($"The {dispName} could not be found.");
        return doesExist;
    }

    private void BrowseGameModFolderButton_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog dialog = new()
        {
            Description = @"Open Game/Mod Folder",
            UseDescriptionForTitle = true
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        gameModFolderPath = dialog.SelectedPath;
        string[] gameModFolderFiles = GetAllFolderFiles(gameModFolderPath);
        mapTileMaskBndPath = gameModFolderFiles.FirstOrDefault(i => i.Contains(".mtmskbnd.dcx")) ?? "";
        mapTileTpfBhdPath = gameModFolderFiles.FirstOrDefault(i => i.Contains("71_maptile.tpfbhd")) ?? "";
        mapTileTpfBtdPath = mapTileTpfBhdPath.Replace(".tpfbhd", ".tpfbdt");
        if (!ResourceExists(mapTileMaskBndPath, "map tile mask BND")) return;
        if (!ResourceExists(mapTileTpfBhdPath, "map tile TPF BHD")) return;
        if (!ResourceExists(mapTileTpfBtdPath, "map tile TPF BTD")) return;
        // gameModFolderPathLabel.Text = Path.GetDirectoryName(mapTileMaskBndPath);
        mapTileMaskBnd = BND4.Read(mapTileMaskBndPath);
        mapTileTpfBhd = BXF4.Read(mapTileTpfBhdPath, mapTileTpfBtdPath);
        // outputFolderGroupBox.Enabled = true;
    }

    private void OutputFolderButton_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog dialog = new();
        if (dialog.ShowDialog() != DialogResult.OK) return;
        outputFolderPath = dialog.SelectedPath;
        /*
        outputFolderPathLabel.Text = outputFolderPath;
        statusLabel.Enabled = true;
        progressLabel.Enabled = true;
        automateButton.Enabled = true;
        */
    }

    private static MagickImage CreateMapGrid(int gridSizeX, int gridSizeY, int tileSize)
    {
        return new MagickImage(MagickColors.White, tileSize * gridSizeX, tileSize * gridSizeY);
    }

    private static void SetFlags(int zoomLevel)
    {
        Flags = new Matrix();
        for (int i = 0; i < MapTileMaskRoot?.ChildNodes.Count; ++i)
        {
            XmlNode? node = MapTileMaskRoot.ChildNodes[i];
            if (node == null)
            {
                ShowInformationDialog($@"Coordinate node {i} does not exist.");
                continue;
            }
            if (node.Attributes == null || node.Attributes.Count < 2)
            {
                ShowInformationDialog($@"Coordinate node {i} does not contain any attribute information.");
                continue;
            }
            string coord = $"{int.Parse(node.Attributes[1].Value):00000}";
            if (coord == "02831")
            {
                Console.WriteLine("");
            }
            bool isValid = zoomLevel switch
            {
                0 => coord.StartsWith("0"),
                1 => coord.StartsWith("1"),
                2 => coord.StartsWith("2"),
                _ => false
            };
            if (!isValid) continue;
            int x = int.Parse(coord.Substring(1, 2));
            int y = int.Parse(coord.Substring(3, 2));
            Flags[x, y] = int.Parse(node.Attributes[2].Value);
        }
    }

    private async Task GenerateMaps()
    {
        foreach (BinderFile file in mapTileMaskBnd.Files.SkipLast(1))
        {
            string fileName = Path.GetFileName(file.Name);
            // progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing map tile mask {fileName}..."));
            await Task.Delay(1000);
            XmlDocument doc = new();
            doc.Load(new MemoryStream(file.Bytes));
            MapTileMaskRoot = doc.LastChild;
            if (MapTileMaskRoot == null)
            {
                ShowInformationDialog($@"{fileName} contains no root XML node.");
                continue;
            }
            SetFlags(0);
            int previousZoomLevel = 0;
            int gridSizeX = 41;
            int gridSizeY = 41;
            const int tileSize = 256;
            MagickImage grid = CreateMapGrid(gridSizeX, gridSizeY, tileSize);
            string rawOutputFileName = "";
            foreach (BinderFile tpfFile in mapTileTpfBhd.Files)
            {
                TPF.Texture texFile = TPF.Read(tpfFile.Bytes).Textures[0];
                if (string.IsNullOrEmpty(rawOutputFileName)) rawOutputFileName = texFile.Name;
                string[] tokens = texFile.Name.Split('_');
                if (!(tokens[0].ToLower() == "menu" && tokens[1].ToLower() == "maptile")) continue;
                // progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing texture file {texFile.Name}..."));
                int zoomLevel = int.Parse(tokens[3][1..]);
                if (zoomLevel != previousZoomLevel)
                {
                    SetFlags(zoomLevel);
                    string outputFileName = $"{rawOutputFileName}.tga";
                    string outputFilePath = $"{outputFolderPath}\\{outputFileName}";
                    // progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Writing {outputFileName} to file..."));
                    await Task.Delay(1000);
                    await grid.WriteAsync(outputFilePath);
                    rawOutputFileName = texFile.Name;
                    if (rawOutputFileName.Contains("L0")) return;
                    previousZoomLevel = zoomLevel;
                    gridSizeX = zoomLevel switch
                    {
                        0 => 41,
                        1 => 31,
                        2 => 11,
                        3 => 6,
                        4 => 2,
                        _ => gridSizeX
                    };
                    gridSizeY = zoomLevel switch
                    {
                        0 => 41,
                        1 => 31,
                        2 => 11,
                        3 => 6,
                        4 => 2,
                        _ => gridSizeX
                    };
                    grid = CreateMapGrid(gridSizeX, gridSizeY, tileSize);
                }
                int x = int.Parse(tokens[4]);
                int y = int.Parse(tokens[5]);
                int flag = int.Parse(tokens[6], NumberStyles.HexNumber);
                long[] filteredFlags =
                {
                    Flags[x, y] & ~(1 << 17),
                    Flags[x, y] & ~(1 << 18)
                };
                if (filteredFlags.All(i => i != flag) && zoomLevel < 3) continue;
                MagickImage tile = new(texFile.Bytes);
                tile.Resize(tileSize, tileSize);
                tile.BorderColor = MagickColors.Black;
                tile.Border(5);
                int adjustedX = x * tileSize;
                int adjustedY = grid.Height - y * tileSize - tileSize;
                grid.Draw(new Drawables().Composite(adjustedX, adjustedY, tile));
                MagickReadSettings textSettings = new()
                {
                    Font = "Calibri",
                    FontPointsize = 20,
                    StrokeColor = MagickColors.Red,
                    StrokeWidth = 1,
                    FillColor = MagickColors.Red,
                    TextGravity = Gravity.Northwest,
                    BackgroundColor = MagickColors.Transparent,
                    Height = 50,
                    Width = 100
                };
                MagickImage coordinateText = new($"caption:[{x},{y}]", textSettings);
                MagickImage flagText = new($"caption:0x{flag:X8}", textSettings);
                grid.Composite(coordinateText, adjustedX + 10, adjustedY + 15, CompositeOperator.Over);
                grid.Composite(flagText, adjustedX + 10, adjustedY + 40, CompositeOperator.Over);
            }
        }
    }

    private void RepackTileMap()
    {
        string groundLevel = groundLevelComboBox.Invoke(() => GetGroundLevels()[groundLevelComboBox.SelectedIndex]);
        string zoomLevel = zoomLevelComboBox.Invoke(() => GetZoomLevels().Keys.ToList()[zoomLevelComboBox.SelectedIndex]);
        int gridSize = GetZoomLevels().GetValueOrDefault(zoomLevel);
        const int tileSize = 256;
        string outputDirectory = Path.Combine(Path.GetDirectoryName(mapImageFilePathLabel.Text)!, "output");
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
        using Bitmap mapImage = new(savedMapImage);
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Rectangle tileRect = new(
                    x * tileSize,
                    y * tileSize,
                    Math.Min(tileSize, mapImage.Width - x * tileSize),
                    Math.Min(tileSize, mapImage.Height - y * tileSize)
                );
                using Bitmap tileImage = new(tileSize, tileSize);
                using (Graphics g = Graphics.FromImage(tileImage))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImage(mapImage, new Rectangle(0, 0, tileSize, tileSize), tileRect, GraphicsUnit.Pixel);
                }
                string tileXPos = x.ToString("D2");
                string tileYPos = (gridSize - y - 1).ToString("D2");
                string tileName = $"MENU_MapTile_{groundLevel}_{zoomLevel}_{tileXPos}_{tileYPos}_00000000";
                progressLabel.Invoke(() => progressLabel.Text = $@"Exporting {tileName}.tpf.dcx...");
                string tileFileName = Path.Combine(outputDirectory, $"{tileName}.tpf.dcx");
                TPF.Texture texture = new();
                byte[] bytes = (byte[])new ImageConverter().ConvertTo(tileImage, typeof(byte[]))!;
                IMagickImage<ushort> image = MagickImage.FromBase64(Convert.ToBase64String(bytes));
                texture.Bytes = ConvertMagickImageToDDS(image);
                texture.Name = tileName;
                TPF tpf = new();
                tpf.Textures.Add(texture);
                tpf.Write(tileFileName);
            }
        }
    }

    // TODO: Make this work for unpacking and stitching a map...
    // TODO: Make it so only one automation operation can run at a time...

    private async void AutomateButton_Click(object sender, EventArgs e)
    {
        if (isBusy) return;
        isBusy = true;
        // await Task.Run(GenerateMaps);
        await Task.Run(RepackTileMap);
        progressLabel.Invoke(new Action(() => progressLabel.Text = @"Automation complete!"));
        await Task.Delay(1000);
        progressLabel.Text = @"Waiting...";
        isBusy = false;
    }

    private void UpdateMapDisplayMinZoomLevel()
    {
        mapDisplayPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        Size clientSize = mapDisplayPictureBox.ClientSize;
        Size imageSize = mapDisplayPictureBox.Image.Size;
        mapDisplayMinZoomLevel = (float)clientSize.Width / imageSize.Width;
        mapDisplayZoomLevel = mapDisplayMinZoomLevel;
        mapDisplayPictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
    }

    // TODO: The L3 and L4 zoom levels are not used for M01...

    private static List<string> GetGroundLevels()
    {
        return new List<string>
        {
            "M00",
            "M01",
            "M10",
            "M11"
        };
    }

    private void PopulateGroundLevels()
    {
        mapDisplayOpenMapImageLabel.Visible = false;
        mapDisplayGroupBox.Enabled = true;
        mapConfigurationGroupBox.Enabled = true;
        unpackStitchMapRadioButton.Checked = true;
        List<string> groundLevels = GetGroundLevels();
        groundLevelComboBox.Items.Clear();
        groundLevels[0] += " (Overworld)";
        groundLevels[1] += " (Underworld)";
        groundLevels[2] += " (DLC)";
        groundLevels[3] += " (Unused)";
        groundLevels.ForEach(i => groundLevelComboBox.Items.Add(i));
        groundLevelComboBox.SelectedIndex = 0;
    }

    // TODO: Implement autosizing if the map image doesn't match the zoom level...

    private static Dictionary<string, int> GetZoomLevels()
    {
        return new Dictionary<string, int>
        {
            { "L0", 41 },
            { "L1", 31 },
            { "L2", 11 },
            { "L3", 6 },
            { "L4", 2 }
        };
    }

    private void PopulateZoomLevels()
    {
        Dictionary<string, int> zoomLevels = GetZoomLevels();
        zoomLevelComboBox.Items.Clear();
        List<string> zoomLevelKeys = zoomLevels.Keys.ToList();
        zoomLevelKeys[0] += " (41x41)";
        zoomLevelKeys[1] += " (31x31)";
        zoomLevelKeys[2] += " (11x11)";
        zoomLevelKeys[3] += " (6x6)";
        zoomLevelKeys[4] += " (2x2)";
        zoomLevelKeys.ToList().ForEach(i => zoomLevelComboBox.Items.Add(i));
        zoomLevelComboBox.SelectedIndex = 0;
    }

    private bool UpdateMapImage(string path)
    {
        Bitmap mapImage;
        try
        {
            mapImage = DdsFile.Load(path).CreateAliasedBitmap();
        }
        catch
        {
            ShowInformationDialog("The map image could not be read.");
            return false;
        }
        savedMapImage = new Bitmap(mapImage);
        mapDisplayPictureBox.Image?.Dispose();
        mapDisplayPictureBox.Image = mapImage;
        mapDisplayPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        UpdateMapImagePosition(0, 0);
        mapDisplayMinZoomLevel = -1;
        return true;
    }

    private void BrowseMapImageButton_Click(object sender, EventArgs e)
    {
        if (isBusy) return;
        OpenFileDialog dialog = new()
        {
            Filter = @"DDS File (*.dds)|*.dds",
            Title = @"Select Map DDS Image"
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        if (!UpdateMapImage(dialog.FileName)) return;
        mapImageFilePath = dialog.FileName;
        mapImageFilePathLabel.Text = mapImageFilePath;
        PopulateGroundLevels();
        PopulateZoomLevels();
        // TODO: Toggle the automation controls...
    }

    private void MapDisplayPictureBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        isDraggingMapDisplay = true;
        mapDisplayXPos = e.X;
        mapDisplayYPos = e.Y;
    }

    private void UpdateMapImagePosition(int x, int y)
    {
        int newTop = y + mapDisplayPictureBox.Top - mapDisplayYPos;
        int newLeft = x + mapDisplayPictureBox.Left - mapDisplayXPos;
        int imageTop = newTop + mapDisplayPictureBox.Height;
        int imageLeft = newLeft + mapDisplayPictureBox.Width;
        if (imageTop > mapDisplayPictureBox.Height)
            newTop = 0;
        if (imageLeft > mapDisplayPictureBox.Width)
            newLeft = 0;
        if (newTop < mapDisplayPictureBox.Parent!.ClientSize.Height - mapDisplayPictureBox.Height)
            newTop = mapDisplayPictureBox.Parent.ClientSize.Height - mapDisplayPictureBox.Height;
        if (newLeft < mapDisplayPictureBox.Parent.ClientSize.Width - mapDisplayPictureBox.Width)
            newLeft = mapDisplayPictureBox.Parent.ClientSize.Width - mapDisplayPictureBox.Width;
        mapDisplayPictureBox.Top = newTop;
        mapDisplayPictureBox.Left = newLeft;
    }

    private void MapDisplayPictureBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isDraggingMapDisplay) return;
        UpdateMapImagePosition(e.X, e.Y);
    }

    private void MapDisplayPictureBox_MouseUp(object sender, MouseEventArgs e)
    {
        isDraggingMapDisplay = false;
    }

    private void ERMapGenerator_Shown(object sender, EventArgs e)
    {
        mapDisplayGroupBox.Enabled = false;
        mapConfigurationGroupBox.Enabled = false;
    }

    private void MapDisplayPictureBox_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (mapDisplayMinZoomLevel < 0) UpdateMapDisplayMinZoomLevel();
        int oldWidth = mapDisplayPictureBox.Image.Width;
        int oldHeight = mapDisplayPictureBox.Image.Height;
        int oldX = mapDisplayPictureBox.Left;
        int oldY = mapDisplayPictureBox.Top;
        mapDisplayZoomLevel = e.Delta > 0
            ? Math.Min(mapDisplayZoomLevel * (1 + mapDisplayZoomIncrement), mapDisplayMaxZoomLevel)
            : Math.Max(mapDisplayZoomLevel * (1 - mapDisplayZoomIncrement), mapDisplayMinZoomLevel);
        Bitmap mapImage = new(savedMapImage,
            new Size((int)(savedMapImage.Width * mapDisplayZoomLevel),
                (int)(savedMapImage.Height * mapDisplayZoomLevel)));
        mapDisplayPictureBox.Image?.Dispose();
        mapDisplayPictureBox.Image = mapImage;
        float mouseX = e.X;
        float mouseY = e.Y;
        float scaleFactorX = (float)mapImage.Width / oldWidth;
        float scaleFactorY = (float)mapImage.Height / oldHeight;
        int newLeft = (int)(oldX - mouseX * (scaleFactorX - 1));
        int newTop = (int)(oldY - mouseY * (scaleFactorY - 1));
        mapDisplayPictureBox.Left = newLeft;
        mapDisplayPictureBox.Top = newTop;
        int imageTop = newTop + mapDisplayPictureBox.Height;
        int imageLeft = newLeft + mapDisplayPictureBox.Width;
        if (imageTop > mapDisplayPictureBox.Height)
            newTop = 0;
        if (imageLeft > mapDisplayPictureBox.Width)
            newLeft = 0;
        if (newTop < mapDisplayPictureBox.Parent!.ClientSize.Height - mapDisplayPictureBox.Height)
            newTop = mapDisplayPictureBox.Parent.ClientSize.Height - mapDisplayPictureBox.Height;
        if (newLeft < mapDisplayPictureBox.Parent.ClientSize.Width - mapDisplayPictureBox.Width)
            newLeft = mapDisplayPictureBox.Parent.ClientSize.Width - mapDisplayPictureBox.Width;
        mapDisplayPictureBox.Top = newTop;
        mapDisplayPictureBox.Left = newLeft;
    }

    private static byte[] ConvertMagickImageToDDS(IMagickImage image)
    {
        MemoryStream ogDdsStream = new();
        image.Write(ogDdsStream, MagickFormat.Dds);
        Surface ddsSurface = DdsFile.Load(ogDdsStream.ToArray());
        MemoryStream recomDdsStream = new();
        DdsFile.Save(recomDdsStream, DdsFileFormat.BC7, DdsErrorMetric.Perceptual, BC7CompressionSpeed.Fast,
            false, false, ResamplingAlgorithm.Bicubic, ddsSurface, null);
        return recomDdsStream.ToArray();
    }

    private class Matrix
    {
        private readonly Dictionary<string, int> Data = new();
        public int this[int x, int y]
        {
            get
            {
                string key = GetKey(x, y);
                return Data.ContainsKey(key) ? Data[key] : -1;
            }
            set
            {
                string key = GetKey(x, y);
                Data[key] = value;
            }
        }

        private static string GetKey(int x, int y)
        {
            return string.Join(",", new[] { x, y });
        }
    }
}