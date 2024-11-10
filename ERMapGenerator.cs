using System.Globalization;
using System.Reflection;
using System.Text.Json.Nodes;
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
    private static BXF4 mapTileBhd = new();
    private JsonNode? extrasJson;
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
        // if (!ResourceExists(mapTileMaskBndPath, "map tile mask BND")) return;
        if (!ResourceExists(mapTileTpfBhdPath, "map tile TPF BHD")) return;
        if (!ResourceExists(mapTileTpfBtdPath, "map tile TPF BTD")) return;
        gameModFolderPathLabel.Text = Path.GetDirectoryName(mapTileTpfBhdPath);
        if (!string.IsNullOrEmpty(mapTileMaskBndPath))
            mapTileMaskBnd = BND4.Read(mapTileMaskBndPath);
        mapTileTpfBhd = BXF4.Read(mapTileTpfBhdPath, mapTileTpfBtdPath);
        outputFolderGroupBox.Enabled = true;
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
            // TODO: We should be able to use int.Parse(startingZoomLevel[1..])...
            else SetFlags(0);
        }
    }

    private async Task UnpackStitchMap(string startingGroundLevel = "M00", string startingZoomLevel = "L0")
    {
        if (string.IsNullOrEmpty(outputFolderPath)) return;
        bool zoomLevelReached = false;
        if (mapTileMaskBnd.Files.Count > 0) await ReadMapTileMaskRoot(startingGroundLevel);
        int gridSizeX = GetZoomLevels().GetValueOrDefault(startingZoomLevel);
        const int tileSize = 256;
        MagickImage grid = CreateMapGrid(gridSizeX, gridSizeX, tileSize);
        string rawOutputFileName = "";

        // TODO: WIP
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

    private async Task GenerateMaps()
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
                await UnpackStitchMap(groundLevel, zoomLevel);
        }
    }

    private async Task RepackTileMap()
    {
        // TODO: Function
        List<string> groundLevels = GetGroundLevels();
        List<string> zoomLevels = GetZoomLevels().Keys.ToList();
        int groundLevelIndex = groundLevelComboBox.Invoke(() => groundLevelComboBox.SelectedIndex);
        int zoomLevelIndex = zoomLevelComboBox.Invoke(() => zoomLevelComboBox.SelectedIndex);
        groundLevels = GetFilteredGroundLevels(groundLevelIndex, groundLevels).ToList();
        zoomLevels = GetFilteredZoomLevels(zoomLevelIndex, zoomLevels).ToList();
        foreach (string groundLevel in groundLevels)
        {
            foreach (string zoomLevel in zoomLevels)
                await ExportTiles(groundLevel, zoomLevel);
        }
        mapTileBhd = new BXF4();
    }

    private static IEnumerable<string> GetFilteredGroundLevels(int groundLevelIndex, IReadOnlyList<string> groundLevels)
    {
        return groundLevelIndex == 0 ? groundLevels.Skip(1) : new[] { groundLevels[groundLevelIndex] };
    }

    private static IEnumerable<string> GetFilteredZoomLevels(int zoomLevelIndex, IReadOnlyList<string> zoomLevels)
    {
        return zoomLevelIndex == 0 ? zoomLevels.Skip(1) : new[] { zoomLevels[zoomLevelIndex] };
    }

    private void ReadExtrasJSON()
    {
        string extrasJsonFilePath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\extras.json";
        extrasJson = JsonNode.Parse(File.ReadAllText(extrasJsonFilePath));
    }

    private void WriteTile(IDisposable tileImage, string tileName)
    {
        progressLabel.Invoke(() => progressLabel.Text = $@"Writing {tileName}...");
        TPF.Texture texture = new();
        byte[] bytes = (byte[])new ImageConverter().ConvertTo(tileImage, typeof(byte[]))!;
        IMagickImage<ushort> image = MagickImage.FromBase64(Convert.ToBase64String(bytes));
        texture.Bytes = ConvertMagickImageToDDS(image);
        texture.Name = tileName;
        texture.Format = 0x66;
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

    private async Task ExportTiles(string groundLevel, string zoomLevel)
    {
        int gridSize = GetZoomLevels().GetValueOrDefault(zoomLevel);
        const int tileSize = 256;
        string outputDirectory = Path.Combine(Path.GetDirectoryName(mapImageFilePathLabel.Text)!, "mod\\menu");
        if (!Directory.Exists(outputDirectory)) Directory.CreateDirectory(outputDirectory);
        // TODO: Cleanup
        using Bitmap mapImage = new(savedMapImage);
        /*
        using (Bitmap originalMapImage = new(savedMapImage))
        {
            Size targetSize = GetTargetSizeForZoomLevel(zoomLevel);
            mapImage = originalMapImage.Size != targetSize ? new Bitmap(originalMapImage, targetSize) : new Bitmap(originalMapImage);
        }
        */
        string bhdPath = Path.Combine(outputDirectory, "71_maptile.tpfbhd");
        string bdtPath = bhdPath.Replace(".tpfbhd", ".tpfbdt");
        // TODO: WIP
        await ReadMapTileMaskRoot(groundLevel);
        SetFlagsForExportTiles();
        if (zoomLevel is "L3" or "L4") ReadExtrasJSON();
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
                string tileName = $"MENU_MapTile_{groundLevel}_{zoomLevel}_{tileXPos}_{tileYPos}";
                if (zoomLevel != "L3" && zoomLevel != "L4")
                {
                    string bitFlags = Flags[int.Parse(zoomLevel[1..]), x, gridSize - y - 1].ToString("X8");
                    if (bitFlags == "FFFFFFFF") continue;
                    string newTileName = $"{tileName}_{bitFlags}";
                    WriteTile(tileImage, newTileName);
                }
                else
                {
                    JsonNode? bitFlagsNode = extrasJson?[$"{groundLevel},{zoomLevel}"]?[$"{tileXPos},{tileYPos}"];
                    List<string> bitFlagsList = bitFlagsNode?.AsArray().Select(i => i?.ToString() ?? "").ToList() ?? new List<string>();
                    foreach (string? bitFlags in bitFlagsList.Where(flag => !string.IsNullOrEmpty(flag)))
                    {
                        string newTileName = $"{tileName}_{bitFlags}";
                        Console.WriteLine(newTileName);
                        WriteTile(tileImage, newTileName);
                    }
                }
            }
        }
        mapTileBhd.Files = mapTileBhd.Files.OrderBy(i => i.Name).ToList();
        for (int i = 0; i < mapTileBhd.Files.Count; i++)
            mapTileBhd.Files[i].ID = i;
        mapTileBhd.Write(bhdPath, bdtPath);
    }

    // TODO: Implement autosizing feature when using all zoom levels...

    private static Size GetTargetSizeForZoomLevel(string zoomLevel)
    {
        return zoomLevel switch
        {
            "L0" => new Size(10496, 10496),
            "L1" => new Size(7936, 7936),
            // TODO: WIP
            "L2" => new Size(2816, 2816),
            // These aren't used by M00...
            // "L3" => new Size(1536, 1536),
            // "L4" => new Size(768, 768),
            _ => new Size(10496, 10496)
        };
    }

    private void ToggleAllControls(bool wantsEnabled)
    {
        RefreshMapImage();
        mapConfigurationGroupBox.Enabled = wantsEnabled;
        automationModeTabControl.Enabled = wantsEnabled;
        automateButton.Enabled = wantsEnabled;
    }

    private async void AutomateButton_Click(object sender, EventArgs e)
    {
        ToggleAllControls(false);
        if (automationModeTabControl.SelectedIndex == 0) await Task.Run(GenerateMaps);
        else await Task.Run(RepackTileMap);
        progressLabel.Invoke(new Action(() => progressLabel.Text = @"Automation complete!"));
        await Task.Delay(1000);
        progressLabel.Text = @"Waiting...";
        ToggleAllControls(true);
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

    private static List<string> GetGroundLevels()
    {
        return new List<string>
        {
            "All",
            "M00",
            "M01",
            "M10"
            // "M11"
        };
    }

    private void PopulateGroundLevels()
    {
        mapConfigurationGroupBox.Enabled = true;
        automateButton.Enabled = true;
        List<string> groundLevels = GetGroundLevels();
        groundLevelComboBox.Items.Clear();
        groundLevels[1] += " (Overworld)";
        groundLevels[2] += " (Underworld)";
        groundLevels[3] += " (DLC)";
        // groundLevels[4] += " (Unused)";
        groundLevels.ForEach(i => groundLevelComboBox.Items.Add(i));
        groundLevelComboBox.SelectedIndex = 0;
    }

    private Dictionary<string, int> GetZoomLevels()
    {
        Dictionary<string, int> dict = new()
        {
            { "All", -1 },
            { "L0", 41 },
            { "L1", 31 },
            { "L2", 11 }
            // { "L3", 6 },
            // { "L4", 3 }
        };
        /*
        if (groundLevelComboBox
            .Invoke(() =>
                groundLevelComboBox.SelectedIndex != 2))
            return dict;
        dict.Remove("L3");
        dict.Remove("L4");
        */
        return dict;
    }

    private void PopulateZoomLevels()
    {
        Dictionary<string, int> zoomLevels = GetZoomLevels();
        zoomLevelComboBox.Items.Clear();
        List<string> zoomLevelKeys = zoomLevels.Keys.ToList();
        string[] suffixes = { "", " (41x41)", " (31x31)", " (11x11)", " (6x6)", " (3x3)" };
        for (int i = 0; i < zoomLevelKeys.Count; i++)
        {
            if (i < suffixes.Length) zoomLevelKeys[i] += suffixes[i];
            zoomLevelComboBox.Items.Add(zoomLevelKeys[i]);
        }
        zoomLevelComboBox.SelectedIndex = 0;
    }

    private void RefreshMapImage(bool resetPosition = false)
    {
        if (savedMapImage == null) return;
        mapDisplayPictureBox.Image?.Dispose();
        mapDisplayPictureBox.Image = new Bitmap(savedMapImage);
        mapDisplayPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        if (resetPosition) UpdateMapImagePosition(0, 0);
        mapDisplayMinZoomLevel = -1;
    }

    private bool LoadMapImage(string path)
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
        RefreshMapImage(true);
        return true;
    }

    private void BrowseMapImageButton_Click(object sender, EventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Filter = @"DDS File (*.dds)|*.dds",
            Title = @"Select Map DDS Image"
        };
        if (dialog.ShowDialog() != DialogResult.OK) return;

        // TODO: Function
        string[] mapImageFolderFiles = GetAllFolderFiles(Path.GetDirectoryName(dialog.FileName) ?? "");
        mapTileMaskBndPath = mapImageFolderFiles.FirstOrDefault(i => i.Contains(".mtmskbnd.dcx")) ?? "";
        if (!ResourceExists(mapTileMaskBndPath, "map tile mask BND")) return;
        mapTileMaskBnd = BND4.Read(mapTileMaskBndPath);
        if (!LoadMapImage(dialog.FileName)) return;
        mapImageFilePath = dialog.FileName;
        mapImageFilePathLabel.Text = mapImageFilePath;
        mapDisplayOpenMapImageLabel.Visible = false;
        mapDisplayGroupBox.Enabled = true;
        PopulateGroundLevels();
        PopulateZoomLevels();
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

    private void AutomationModeTabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        RefreshMapImage();
    }

    private void GroundLevelComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
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