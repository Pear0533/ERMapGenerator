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

    private static MagickImage CreateMapGrid(int gridSizeX, int gridSizeY)
    {
        return new MagickImage(MagickColors.White, 256 * gridSizeX, 256 * gridSizeY);
    }

    private static string[] GetTextureFileNameTokens(int index)
    {
        BinderFile? tpfFile = mapTileTpfBhd.Files.ElementAtOrDefault(index);
        if (tpfFile == null) return Array.Empty<string>();
        TPF.Texture texFile = TPF.Read(tpfFile.Bytes).Textures[0];
        string[] tokens = texFile.Name.Split('_');
        return tokens;
    }

    private async Task GenerateMaps()
    {
        foreach (BinderFile file in mapTileMaskBnd.Files)
        {
            string fileName = Path.GetFileName(file.Name);
            progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing map tile mask {fileName}..."));
            await Task.Delay(1000);
            // TODO: Eliminate using the map tile masks, they should be unnecessary...
            Matrix flags = new();
            XmlDocument doc = new();
            doc.Load(new MemoryStream(file.Bytes));
            XmlNode? root = doc.LastChild;
            if (root == null)
            {
                ShowInformationDialog($@"{fileName} contains no root XML node.");
                continue;
            }
            for (int i = 0; i < root.ChildNodes.Count; ++i)
            {
                XmlNode? node = root.ChildNodes[i];
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
                if (!coord.StartsWith("0")) continue;
                int x = int.Parse(coord.Substring(1, 2));
                int y = int.Parse(coord.Substring(3, 2));
                flags[x, y] = uint.Parse(node.Attributes[2].Value);
            }
            int previousZoomLevel = 0;
            int flagOR = -1;
            int gridSizeX = 41;
            int gridSizeY = 41;
            int tileSize = 256;
            MagickImage grid = CreateMapGrid(gridSizeX, gridSizeY);
            List<int[]> drawnTiles = new();
            for (int i = 0; i < mapTileTpfBhd.Files.Count; i++)
            {
                BinderFile tpfFile = mapTileTpfBhd.Files[i];
                TPF.Texture texFile = TPF.Read(tpfFile.Bytes).Textures[0];
                string[] tokens = texFile.Name.Split('_');
                if (!(tokens[0].ToLower() == "menu" && tokens[1].ToLower() == "maptile")) continue;
                progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing texture file {texFile.Name}..."));
                int zoomLevel = int.Parse(tokens[3][1..]);
                if (zoomLevel != previousZoomLevel)
                {
                    string outputFileName = $"{texFile.Name.Replace($"L{zoomLevel}", $"L{previousZoomLevel}")}.tga";
                    string outputFilePath = $"{outputFolderPath}\\{outputFileName}";
                    progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Writing {outputFileName} to file..."));
                    await Task.Delay(1000);
                    await grid.WriteAsync(outputFilePath);
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
                    grid = CreateMapGrid(gridSizeX, gridSizeY);
                    tileSize = gridSizeX * 256 / (gridSizeX * (zoomLevel + 1));
                }
                int x = int.Parse(tokens[4]);
                int y = int.Parse(tokens[5]);
                // TODO: This coordinate string assignment is really messy...
                string coordinateString = $"M0{mapTileMaskBnd.Files.IndexOf(file)}_L{zoomLevel}_{x.ToString().PadLeft(2, '0')}_{y.ToString().PadLeft(2, '0')}_";
                int flag = int.Parse(tokens[6], NumberStyles.HexNumber);
                if (flagOR == -1) flagOR = flag;
                string[] nextTexFileTokens = GetTextureFileNameTokens(i + 1);
                if (nextTexFileTokens.Length > 0)
                {
                    flagOR |= int.Parse(nextTexFileTokens[6], NumberStyles.HexNumber);
                    string flagORString = flagOR.ToString("X8");
                    flagOR = -1;
                    List<BinderFile> tileVariants = mapTileTpfBhd.Files.Where(t => t.Name.Contains(coordinateString)).ToList();
                    bool isNextNewTile = int.Parse(nextTexFileTokens[4]) != x || int.Parse(nextTexFileTokens[5]) != y;
                    int index = tileVariants.FindIndex(f => f.Name.Contains(flagORString));
                    bool doesTileVariantExist = index != -1 && !isNextNewTile || drawnTiles.FindIndex(c => c[0] == x && c[1] == y) != -1;
                    if (flag == 0 && !isNextNewTile || doesTileVariantExist) continue;
                }
                drawnTiles.Add(new[] { x, y });
                MagickImage tile = new(texFile.Bytes);
                tile.Resize(tileSize, tileSize);
                tile.BorderColor = MagickColors.Black;
                tile.Border(2);
                int adjustedX = x * 256;
                int adjustedY = grid.Height - y * 256 - 256;
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
        private readonly Dictionary<string, uint> Data = new();
        public uint this[int x, int y]
        {
            get
            {
                string key = GetKey(x, y);
                return Data.ContainsKey(key) ? Data[key] : 0;
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