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
    private static string[] mapTileMaskImagePaths = { };
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

    private async Task GenerateMaps()
    {
        // TODO: Might need to make grid size dynamic: grid size seems to decrease by 10?
        const int gridSize = 41;
        foreach (BinderFile file in mapTileMaskBnd.Files)
        {
            string fileName = Path.GetFileName(file.Name);
            progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing map tile mask {fileName}..."));
            await Task.Delay(1000);
            uint[,] vals = new uint[gridSize, gridSize];
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
                vals[x, y] = uint.Parse(node.Attributes[2].Value);
            }
            MagickImage image = new(MagickColors.Black, 256 * gridSize, 256 * gridSize);
            foreach (BinderFile tpfFile in mapTileTpfBhd.Files)
            {
                TPF.Texture texFile = TPF.Read(tpfFile.Bytes).Textures[0];
                progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Parsing texture file {texFile.Name}..."));
                if (!texFile.Name.ToLower().StartsWith("menu_maptile")) continue;
                string[] words = texFile.Name.Split('_');
                // TODO: Account for the map level and zoom level during texture insertion
                string mapLevel = words[2];
                string zoomLevel = words[3];
                uint x = ushort.Parse(words[4]);
                uint y = uint.Parse(words[5]);
                uint val = uint.Parse(words[6], NumberStyles.HexNumber);
                if (vals[x, y] != val) continue;
                MagickImage tile = new(texFile.Bytes);
                image.Draw(new Drawables().Composite(x * 256, image.Height - y * 256 - 256, tile));
            }
            string outputFileName = $"{Path.GetFileNameWithoutExtension(file.Name)}.tga";
            string outputFilePath = $"{outputFolderPath}\\{outputFileName}";
            progressLabel.Invoke(new Action(() => progressLabel.Text = $@"Writing {outputFileName} to file..."));
            await Task.Delay(1000);
            await image.WriteAsync(outputFilePath);
        }
    }

    private async void AutomateButton_Click(object sender, EventArgs e)
    {
        await Task.Run(GenerateMaps);
    }
}