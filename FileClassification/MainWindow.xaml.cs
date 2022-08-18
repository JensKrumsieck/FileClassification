using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ChemSharp.Mathematics;
using ChemSharp.Molecules.HelixToolkit;
using FileClassification.ViewModel;
using HelixToolkit.Wpf;
using Microsoft.Win32;

namespace FileClassification;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        ViewModel = new MainViewModel();
        DataContext = ViewModel;
        ViewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.CurrentMolecule))
                Viewport3D.CameraController.CameraTarget =
                    MathV.Centroid(ViewModel.CurrentMolecule.Atoms.Select(a => a.Location).ToList()).ToPoint3D();
        };
    }

    public MainViewModel ViewModel { get; }

    private void Search_OnClick(object sender, RoutedEventArgs e)
    {
        var ofd = new OpenFileDialog
        {
            FileName = "Select Folder.",
            InitialDirectory = PathTextBox.Text,
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = false
        };
        if (ofd.ShowDialog(this) != true) return;
        PathTextBox!.Text = Path.GetDirectoryName(ofd.FileName) ?? "";
        PathTextBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
    }

    /// <summary>
    ///     Handles Atom selection
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Viewport3D_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        var hits = Viewport3D.Viewport.FindHits(e.GetPosition(Viewport3D));
        var shift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
        var alt = (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt;
        foreach (var hit in hits.OrderBy(s => s.Distance))
        {
            if (hit.Visual.GetType() != typeof(Atom3D)) continue;
            if (hit.Visual is not Atom3D av3d) continue;

            //write data
            if (shift && alt) ViewModel.CoordNo++;
            else if (shift) ViewModel.AxialLigand = av3d.Atom.Symbol;
            else if (alt) ViewModel.SubstNo++;
            else
            {
                ViewModel.Group = av3d.Atom.Block != "f-Block" ? av3d.Atom.Group.ToString() : "Ln";
                ViewModel.Metal = av3d.Atom.Symbol;
            }
        }
    }

    private void SubmitClick(object sender, RoutedEventArgs e)
    {
        var m = ViewModel.Metal;
        var clazz = ClassComboBox.Text;
        ViewModel.Class = clazz;
        if (!ViewModel.Classes.Contains(clazz)) ViewModel.Classes.Add(clazz);
        var path = $"{ViewModel.WorkingDir}\\{clazz}\\{m}\\";
        if (!File.Exists(path)) Directory.CreateDirectory(path);

        var filename = ViewModel.Files.Pop();
        var rawFilename = Path.GetFileName(filename);

        File.Move(filename, path + rawFilename);
        File.WriteAllText(path + rawFilename + ".meta.json",
                          JsonSerializer.Serialize(ViewModel, new JsonSerializerOptions {WriteIndented = true}));
        ViewModel.Next();
    }
}