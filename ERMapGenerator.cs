using System.Drawing.Drawing2D;
using System.Globalization;
using System.Xml;
using ImageMagick;
using SoulsFormats;

namespace ERMapGenerator;

public partial class ERMapGenerator : Form
{
    private const string version = "1.4";
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
    private static BXF4 mapTileBhd = new();
    private float mapDisplayMinZoomLevel = -1;
    private float mapDisplayZoomLevel;
    private string mapImageFilePath = "";
    private Bitmap savedMapImage = null!;
    private CancellationTokenSource? cancellationTokenSource;

    private const int L0_SIZE = 10496;
    private const int L1_SIZE = 7936;
    private const int L2_SIZE = 2816;

    private const int L1_SCALE_SUBTRACT = 78;
    private const int L1_OFFSET_X = 1;
    private const int L1_OFFSET_Y = 1;

    private const int L2_SCALE_SUBTRACT = 731;
    private const int L2_OFFSET_X = 4;
    private const int L2_OFFSET_Y = 516;

    private Bitmap? scaledL1Image = null;
    private Bitmap? scaledL2Image = null;

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
        if (!doesExist)
            ShowInformationDialog($"{dispName} wasn't found. "
                + "Please ensure it's located in the menu folder in your game/mod files.");
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
        if (!ResourceExists(mapTileMaskBndPath, "71_maptile.mtmskbnd.dcx")) return;
        if (!ResourceExists(mapTileTpfBhdPath, "71_maptile.tpfbhd")) return;
        if (!ResourceExists(mapTileTpfBtdPath, "71_maptile.tpfbdt")) return;
        gameModFolderPathLabel.Text = Path.GetDirectoryName(mapTileTpfBhdPath);
        mapTileMaskBnd = BND4.Read(mapTileMaskBndPath);
        mapTileTpfBhd = BXF4.Read(mapTileTpfBhdPath, mapTileTpfBtdPath);
        outputFolderGroupBox.Enabled = true;
        mapImageGroupBox.Enabled = true;
    }

    private void OutputFolderButton_Click(object sender, EventArgs e)
    {
        FolderBrowserDialog dialog = new();
        if (dialog.ShowDialog() != DialogResult.OK) return;
        outputFolderPath = dialog.SelectedPath;
        outputFolderPathLabel.Text = outputFolderPath;
        // TODO: Cleanup
        drawTileDebugInfoCheckBox.Enabled = true;
        PopulateGroundLevels();
        PopulateZoomLevels();
    }

    private static MagickImage CreateMapGrid(int gridSizeX, int gridSizeY, int tileSize)
    {
        return new MagickImage(MagickColors.Black, tileSize * gridSizeX, tileSize * gridSizeY);
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
            Flags[-1, x, y] = int.Parse(node.Attributes[2].Value);
        }
    }

    // TODO: This is temporary...

    private static void SetFlagsForExportTiles()
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
            int zoomLevel = int.Parse(coord[..1]);
            int x = int.Parse(coord.Substring(1, 2));
            int y = int.Parse(coord.Substring(3, 2));
            Flags[zoomLevel, x, y] = int.Parse(node.Attributes[2].Value);
        }
    }

    private async Task WriteStitchedMap(IMagickImage grid, string path)
    {
        string outputFileName = $"{path}.tga";
        string outputFilePath = $"{outputFolderPath}\\{outputFileName}";
        progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Writing {outputFileName} to file..."));
        await Task.Delay(1000);
        await grid.WriteAsync(outputFilePath);
    }

    private async Task ReadMapTileMaskRoot(string groundLevel)
    {
        BinderFile? file = mapTileMaskBnd.Files.Find(i => i.Name.Contains(groundLevel));
        if (file != null)
        {
            string fileName = Path.GetFileName(file.Name);
            progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing map tile mask {fileName}..."));
            await Task.Delay(1000);
            XmlDocument doc = new();
            doc.Load(new MemoryStream(file.Bytes));
            MapTileMaskRoot = doc.LastChild;
            if (MapTileMaskRoot == null) ShowInformationDialog($@"{fileName} contains no root XML node.");
            else SetFlags(0);
        }
    }

    private async Task UnpackStitchMap(string startingGroundLevel = "M00", string startingZoomLevel = "L0")
    {
        if (string.IsNullOrEmpty(outputFolderPath)) return;
        bool zoomLevelReached = false;
        await ReadMapTileMaskRoot(startingGroundLevel);
        int gridSizeX = GetZoomLevels().GetValueOrDefault(startingZoomLevel);
        const int tileSize = 256;
        MagickImage grid = CreateMapGrid(gridSizeX, gridSizeX, tileSize);
        string rawOutputFileName = "";
        await ReadMapTileMaskRoot(startingGroundLevel);
        SetFlagsForExportTiles();
        for (int i = 0; i < mapTileTpfBhd.Files.Count; i++)
        {
            BinderFile tpfFile = mapTileTpfBhd.Files[i];
            TPF.Texture texFile = TPF.Read(tpfFile.Bytes).Textures[0];
            string[] tokens = texFile.Name.Split('_');
            if (!(tokens[0].ToLower() == "menu" && tokens[1].ToLower() == "maptile")) continue;
            progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing texture file {texFile.Name}..."));
            string groundLevel = tokens[2];
            string zoomLevel = tokens[3];
            if (zoomLevel != startingZoomLevel && zoomLevelReached)
            {
                await WriteStitchedMap(grid, rawOutputFileName);
                return;
            }
            if (tokens[4] == "00" && tokens[5] == "00" && tokens[6] == "00000000") rawOutputFileName = texFile.Name;
            if (groundLevel != startingGroundLevel || zoomLevel != startingZoomLevel) continue;
            zoomLevelReached = true;
            int x = int.Parse(tokens[4]);
            int y = int.Parse(tokens[5]);
            int flag = int.Parse(tokens[6], NumberStyles.HexNumber);
            if (flag != Flags[int.Parse(startingZoomLevel[1..]), x, y]) continue;
            MagickImage tile = new(texFile.Bytes);
            tile.Resize(tileSize, tileSize);
            if (drawTileDebugInfoCheckBox.Checked)
            {
                tile.BorderColor = MagickColors.Black;
                tile.Border(5);
            }
            int adjustedX = x * tileSize;
            int adjustedY = grid.Height - y * tileSize - tileSize;
            grid.Draw(new Drawables().Composite(adjustedX, adjustedY, tile));
            if (drawTileDebugInfoCheckBox.Checked)
            {
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
            if (i != mapTileTpfBhd.Files.Count - 1) continue;
            await WriteStitchedMap(grid, rawOutputFileName);
            return;
        }
    }

    private async Task GenerateMaps(CancellationToken cancellationToken = default)
    {
        List<string> groundLevels = GetGroundLevels();
        List<string> zoomLevels = GetZoomLevels().Keys.ToList();
        int groundLevelIndex = groundLevelComboBox.Invoke(() => groundLevelComboBox.SelectedIndex);
        int zoomLevelIndex = zoomLevelComboBox.Invoke(() => zoomLevelComboBox.SelectedIndex);
        groundLevels = GetFilteredGroundLevels(groundLevelIndex, groundLevels).ToList();
        zoomLevels = GetFilteredZoomLevels(zoomLevelIndex, zoomLevels).ToList();
        foreach (string groundLevel in groundLevels)
        {
            foreach (string zoomLevel in zoomLevels)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await UnpackStitchMap(groundLevel, zoomLevel);
            }
        }
    }

    private Bitmap GetScaledMapForZoomLevel(string zoomLevel)
    {
        if (savedMapImage == null) return null!;
        int targetSize = zoomLevel switch
        {
            "L0" => L0_SIZE,
            "L1" => L1_SIZE,
            "L2" => L2_SIZE,
            _ => L0_SIZE
        };
        if (zoomLevel == "L0" && savedMapImage.Width == L0_SIZE && savedMapImage.Height == L0_SIZE)
        {
            return new Bitmap(savedMapImage);
        }
        if (zoomLevel == "L1" && scaledL1Image != null)
        {
            return new Bitmap(scaledL1Image);
        }
        if (zoomLevel == "L2" && scaledL2Image != null)
        {
            return new Bitmap(scaledL2Image);
        }
        Bitmap scaledImage = new Bitmap(targetSize, targetSize);
        using (Graphics g = Graphics.FromImage(scaledImage))
        {
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;
            if (zoomLevel == "L1")
            {
                int scaledSize = targetSize - L1_SCALE_SUBTRACT;
                g.DrawImage(savedMapImage, L1_OFFSET_X, L1_OFFSET_Y, scaledSize, scaledSize);
            }
            else if (zoomLevel == "L2")
            {
                int scaledSize = targetSize - L2_SCALE_SUBTRACT;
                g.DrawImage(savedMapImage, L2_OFFSET_X, L2_OFFSET_Y, scaledSize, scaledSize);
            }
            else
            {
                g.DrawImage(savedMapImage, 0, 0, targetSize, targetSize);
            }
        }
        if (zoomLevel == "L1")
        {
            scaledL1Image?.Dispose();
            scaledL1Image = new Bitmap(scaledImage);
        }
        else if (zoomLevel == "L2")
        {
            scaledL2Image?.Dispose();
            scaledL2Image = new Bitmap(scaledImage);
        }
        return scaledImage;
    }

    private async Task RepackTileMap(CancellationToken cancellationToken = default)
    {
        List<string> groundLevels = GetGroundLevels();
        List<string> zoomLevels = GetZoomLevels().Keys.ToList();
        int groundLevelIndex = groundLevelComboBox.Invoke(() => groundLevelComboBox.SelectedIndex);
        int zoomLevelIndex = zoomLevelComboBox.Invoke(() => zoomLevelComboBox.SelectedIndex);
        groundLevels = GetFilteredGroundLevels(groundLevelIndex, groundLevels).ToList();
        zoomLevels = GetFilteredZoomLevels(zoomLevelIndex, zoomLevels).ToList();
        mapTileBhd = new BXF4();
        progressLabel.Invoke(() => progressLabel.Text = @"Preparing scaled map images...");
        if (zoomLevels.Contains("L1"))
        {
            scaledL1Image?.Dispose();
            scaledL1Image = GetScaledMapForZoomLevel("L1");
        }
        if (zoomLevels.Contains("L2"))
        {
            scaledL2Image?.Dispose();
            scaledL2Image = GetScaledMapForZoomLevel("L2");
        }
        foreach (string groundLevel in groundLevels)
        {
            foreach (string zoomLevel in zoomLevels)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ExportTiles(groundLevel, zoomLevel, cancellationToken);
            }
        }
        await FinalizeRepack();
        scaledL1Image?.Dispose();
        scaledL1Image = null;
        scaledL2Image?.Dispose();
        scaledL2Image = null;
    }

    private async Task FinalizeRepack()
    {
        progressLabel.Invoke(() => progressLabel.Text = @"Finalizing repack...");
        await Task.Delay(500);
        IEnumerable<int> files = mapTileBhd.Files.Select(file =>
            mapTileTpfBhd.Files.FindIndex(i =>
                string.Equals(i.Name, file.Name, StringComparison.OrdinalIgnoreCase)));
        foreach (int i in files.Where(index => index != -1))
            mapTileTpfBhd.Files.RemoveAt(i);
        mapTileTpfBhd.Files.AddRange(mapTileBhd.Files);
        mapTileTpfBhd.Files = mapTileTpfBhd.Files.OrderBy(i => i.Name).ToList();
        for (int i = 0; i < mapTileTpfBhd.Files.Count; i++)
            mapTileTpfBhd.Files[i].ID = i;
        progressLabel.Invoke(() => progressLabel.Text = @"Writing to disk...");
        await Task.Run(() => mapTileTpfBhd.Write(mapTileTpfBhdPath, mapTileTpfBtdPath));
    }

    private IEnumerable<string> GetFilteredGroundLevels(int groundLevelIndex, IReadOnlyList<string> groundLevels)
    {
        return automationModeTabControl.Invoke(() => automationModeTabControl.SelectedIndex == 0)
            ? groundLevelIndex == 0 ? groundLevels.Skip(1) : new[] { groundLevels[groundLevelIndex] }
            : new[] { groundLevels[groundLevelIndex] };
    }

    private IEnumerable<string> GetFilteredZoomLevels(int zoomLevelIndex, IReadOnlyList<string> zoomLevels)
    {
        // TODO: Function
        return automationModeTabControl.Invoke(() => automationModeTabControl.SelectedIndex == 0)
            ? zoomLevelIndex == 0 ? zoomLevels.Skip(1) : new[] { zoomLevels[zoomLevelIndex] }
            : new[] { zoomLevels[zoomLevelIndex] };
    }

    private void WriteTile(Bitmap tileImage, string tileName)
    {
        progressLabel.Invoke(() => progressLabel.Text = $@"Writing {tileName}...");
        byte[] ddsBytes = DdsHelper.ConvertBitmapToDDS(tileImage);
        TPF.Texture texture = new()
        {
            Bytes = ddsBytes,
            Name = tileName,
            Format = 0x66
        };
        TPF tpf = new() { Compression = DCX.Type.DCX_KRAK };
        tpf.Textures.Add(texture);
        byte[] tpfBytes = tpf.Write();
        BinderFile file = new()
        {
            Name = $"71_MapTile\\{tileName}.tpf.dcx",
            Bytes = tpfBytes
        };
        mapTileBhd.Files.Add(file);
    }

    private async Task ExportTiles(string groundLevel, string zoomLevel, CancellationToken cancellationToken = default)
    {
        if (savedMapImage == null) return;
        int gridSize = GetZoomLevels().GetValueOrDefault(zoomLevel);
        const int tileSize = 256;
        using Bitmap mapImage = GetScaledMapForZoomLevel(zoomLevel);
        await ReadMapTileMaskRoot(groundLevel);
        SetFlagsForExportTiles();
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                cancellationToken.ThrowIfCancellationRequested();
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
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    g.DrawImage(mapImage, new Rectangle(0, 0, tileSize, tileSize), tileRect, GraphicsUnit.Pixel);
                }
                string tileXPos = x.ToString("D2");
                string tileYPos = (gridSize - y - 1).ToString("D2");
                string tileName = $"MENU_MapTile_{groundLevel}_{zoomLevel}_{tileXPos}_{tileYPos}";
                string bitFlags = Flags[int.Parse(zoomLevel[1..]), x, gridSize - y - 1].ToString("X8");
                if (bitFlags == "FFFFFFFF") continue;
                string newTileName = $"{tileName}_{bitFlags}";
                WriteTile(tileImage, newTileName);
            }
        }
    }

    private void ToggleAllControls(bool wantsEnabled)
    {
        RefreshMapImage();
        mapConfigurationGroupBox.Enabled = wantsEnabled;
        automationModeTabControl.Enabled = wantsEnabled;
        automateButton.Enabled = wantsEnabled;
        automateButton.Visible = wantsEnabled;
        stopButton.Enabled = !wantsEnabled;
        stopButton.Visible = !wantsEnabled;
        gameModFolderGroupBox.Enabled = wantsEnabled;
    }

    private async void AutomateButton_Click(object sender, EventArgs e)
    {
        cancellationTokenSource = new CancellationTokenSource();
        ToggleAllControls(false);
        try
        {
            if (automationModeTabControl.SelectedIndex == 0)
                await Task.Run(() => GenerateMaps(cancellationTokenSource.Token));
            else
                await Task.Run(() => RepackTileMap(cancellationTokenSource.Token));
            progressLabel.Invoke(new Action(() => progressLabel.Text = @"Automation complete!"));
        }
        catch (OperationCanceledException)
        {
            progressLabel.Invoke(new Action(() => progressLabel.Text = @"Stopped by user. Repacking tiles..."));
            await Task.Delay(1000);
            await FinalizeRepack();
            progressLabel.Invoke(new Action(() => progressLabel.Text = @"Repacking complete!"));
        }
        await Task.Delay(1000);
        progressLabel.Text = @"Waiting...";
        ToggleAllControls(true);
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = null;
    }

    private void StopButton_Click(object sender, EventArgs e)
    {
        cancellationTokenSource?.Cancel();
        stopButton.Enabled = false;
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

    private List<string> GetGroundLevels()
    {
        List<string> groundLevels = new()
        {
            "All",
            "M00",
            "M01",
            "M10"
        };
        if (automationModeTabControl.Invoke(()
                => automationModeTabControl.SelectedIndex == 1))
            groundLevels.RemoveAt(0);
        return groundLevels;
    }

    private void PopulateGroundLevels()
    {
        string previousSelectedItem = groundLevelComboBox.SelectedItem?.ToString() ?? "";
        mapConfigurationGroupBox.Enabled = true;
        automateButton.Enabled = true;
        List<string> groundLevels = GetGroundLevels();
        groundLevelComboBox.Items.Clear();
        List<string> suffixes = new() { "", " (Overworld)", " (Underworld)", " (DLC)" };
        if (automationModeTabControl.SelectedIndex == 1)
            suffixes.RemoveAt(0);
        for (int i = 0; i < groundLevels.Count; i++)
        {
            if (i < suffixes.Count) groundLevels[i] += suffixes[i];
            groundLevelComboBox.Items.Add(groundLevels[i]);
        }
        int index = groundLevelComboBox.Items.IndexOf(previousSelectedItem);
        groundLevelComboBox.SelectedIndex = index != -1 ? index : 0;
    }

    private Dictionary<string, int> GetZoomLevels()
    {
        Dictionary<string, int> dict = new()
        {
            { "All", -1 },
            { "L0", 41 },
            { "L1", 31 },
            { "L2", 11 }
        };
        if (automationModeTabControl.Invoke(()
                => automationModeTabControl.SelectedIndex == 1))
            dict.Remove("All");
        return dict;
    }

    private void PopulateZoomLevels()
    {
        // TODO: Function
        string previousSelectedItem = zoomLevelComboBox.SelectedItem?.ToString() ?? "";
        Dictionary<string, int> zoomLevels = GetZoomLevels();
        zoomLevelComboBox.Items.Clear();
        List<string> zoomLevelKeys = zoomLevels.Keys.ToList();
        List<string> suffixes = new() { "", " (41x41)", " (31x31)", " (11x11)", " (6x6)", " (3x3)" };
        if (automationModeTabControl.SelectedIndex == 1)
            suffixes.RemoveAt(0);
        for (int i = 0; i < zoomLevelKeys.Count; i++)
        {
            if (i < suffixes.Count) zoomLevelKeys[i] += suffixes[i];
            zoomLevelComboBox.Items.Add(zoomLevelKeys[i]);
        }
        int index = zoomLevelComboBox.Items.IndexOf(previousSelectedItem);
        zoomLevelComboBox.SelectedIndex = index != -1 ? index : 0;
    }

    private void RefreshMapImage(bool resetPosition = false)
    {
        if (savedMapImage == null) return;
        mapDisplayPictureBox.Image?.Dispose();
        mapDisplayPictureBox.Image = new Bitmap(savedMapImage);
        mapDisplayPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        if (resetPosition) UpdateMapImagePosition(0, 0);
        mapDisplayMinZoomLevel = -1;
        scaledL1Image?.Dispose();
        scaledL1Image = null;
        scaledL2Image?.Dispose();
        scaledL2Image = null;
    }

    private bool LoadMapImage(string path)
    {
        Bitmap mapImage;
        try
        {
            using MagickImage magickImage = new(path);
            using MemoryStream memoryStream = new();
            magickImage.Write(memoryStream, MagickFormat.Bmp);
            memoryStream.Position = 0;
            mapImage = new Bitmap(memoryStream);
        }
        catch
        {
            ShowInformationDialog("The map image could not be read.");
            return false;
        }
        savedMapImage = new Bitmap(mapImage);
        RefreshMapImage(true);
        return true;
    }

    private void BrowseMapImageButton_Click(object sender, EventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Filter = @"TGA File (*.tga)|*.tga",
            Title = @"Select Map TGA Image"
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;
        // TODO: Function
        string[] gameModFolderFiles = GetAllFolderFiles(gameModFolderPath);
        mapTileMaskBndPath = gameModFolderFiles.FirstOrDefault(i => i.Contains(".mtmskbnd.dcx")) ?? "";
        if (!ResourceExists(mapTileMaskBndPath, "71_maptile.mtmskbnd.dcx")) return;
        mapTileMaskBnd = BND4.Read(mapTileMaskBndPath);
        if (!LoadMapImage(dialog.FileName)) return;
        mapImageFilePath = dialog.FileName;
        mapImageFilePathLabel.Text = mapImageFilePath;
        mapDisplayOpenMapImageLabel.Visible = false;
        mapDisplayGroupBox.Enabled = true;
        PopulateGroundLevels();
        PopulateZoomLevels();

        // TODO: Cleanup
        try
        {
            string mapImageFileName = Path.GetFileName(mapImageFilePath);
            string[] tokens = mapImageFileName.Split('_');
            groundLevelComboBox.SelectedItem = groundLevelComboBox.Items.Cast<string>().FirstOrDefault(i => i.StartsWith(tokens[2]));
            zoomLevelComboBox.SelectedItem = zoomLevelComboBox.Items.Cast<string>().FirstOrDefault(i => i.StartsWith(tokens[3]));
        }
        catch { }
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
        outputFolderGroupBox.Enabled = false;
        drawTileDebugInfoCheckBox.Enabled = false;
        automateButton.Enabled = false;
        mapImageGroupBox.Enabled = false;
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

    private void AutomationModeTabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshMapImage();
        if (groundLevelComboBox.Items.Count <= 0 || zoomLevelComboBox.Items.Count <= 0) return;
        PopulateGroundLevels();
        PopulateZoomLevels();
    }

    private class Matrix
    {
        private readonly Dictionary<string, int> Data = new();
        public int this[int zoomLevel, int x, int y]
        {
            get
            {
                string key = GetKey(zoomLevel, x, y);
                return Data.ContainsKey(key) ? Data[key] : -1;
            }
            set
            {
                string key = GetKey(zoomLevel, x, y);
                Data[key] = value;
            }
        }

        private static string GetKey(int zoomLevel, int x, int y)
        {
            return string.Join(",", new[] { zoomLevel, x, y });
        }
    }
}