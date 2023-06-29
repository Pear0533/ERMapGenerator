using System.Globalization;
using System.Xml;
using ImageMagick;
using SoulsFormats;

namespace ERMapGenerator;

public partial class ERMapGenerator : Form
{
    private const string version = "1.0";
    private static string gameModFolderPath = "";
    private static string mapTileMaskBndPath = "";
    private static string mapTileTpfBhdPath = "";
    private static string mapTileTpfBtdPath = "";
    private static string outputFolderPath = "";
    private static BND4 mapTileMaskBnd = new();
    private static BXF4 mapTileTpfBhd = new();
    private static Matrix Flags = new();
    private static XmlNode? MapTileMaskRoot;

    public ERMapGenerator()
    {
        InitializeComponent();
        SetVersionString();
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
        gameModFolderPathLabel.Text = Path.GetDirectoryName(mapTileMaskBndPath);
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
        statusLabel.Enabled = true;
        progressLabel.Enabled = true;
        automateButton.Enabled = true;
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
        foreach (BinderFile file in mapTileMaskBnd.Files)
        {
            string fileName = Path.GetFileName(file.Name);
            progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing map tile mask {fileName}..."));
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
                progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing texture file {texFile.Name}..."));
                int zoomLevel = int.Parse(tokens[3][1..]);
                if (zoomLevel != previousZoomLevel)
                {
                    SetFlags(zoomLevel);
                    string outputFileName = $"{rawOutputFileName}.tga";
                    string outputFilePath = $"{outputFolderPath}\\{outputFileName}";
                    progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Writing {outputFileName} to file..."));
                    await Task.Delay(1000);
                    await grid.WriteAsync(outputFilePath);
                    rawOutputFileName = texFile.Name;
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
                tile.Border(2);
                int adjustedX = x * tileSize;
                int adjustedY = grid.Height - y * tileSize - tileSize;
                grid.Draw(new Drawables().Composite(adjustedX, adjustedY, tile));
                MagickReadSettings textSettings = new()
                {
                    Font = "Calibri",
                    FontPointsize = 16,
                    FillColor = MagickColors.Red,
                    TextGravity = Gravity.Northwest,
                    BackgroundColor = MagickColors.Transparent,
                    Height = 50,
                    Width = 100
                };
                MagickImage coordinateText = new($"caption:[{x},{y}]", textSettings);
                MagickImage flagText = new($"caption:0x{flag:X8}", textSettings);
                grid.Composite(coordinateText, adjustedX + 5, adjustedY + 5, CompositeOperator.Over);
                grid.Composite(flagText, adjustedX + 5, adjustedY + 30, CompositeOperator.Over);
            }
        }
    }

    private async void AutomateButton_Click(object sender, EventArgs e)
    {
        await Task.Run(GenerateMaps);
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