using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ChemSharp.Molecules;
using ChemSharp.Molecules.HelixToolkit;
using TinyMVVM;

namespace FileClassification.ViewModel;

public class MainViewModel : BaseViewModel
{
    public ObservableCollection<string> Classes { get; } = new();

    private Stack<string> _files;
    private string _workingDir;

    private Molecule _currentMolecule;

    public Molecule CurrentMolecule
    {
        get => _currentMolecule;
        set => Set(ref _currentMolecule, value);
    }

    private string _selectedAtom = "";
    public string SelectedAtom
    {
        get => _selectedAtom;
        set => Set(ref _selectedAtom, value);
    }
    
    
    public string WorkingDir
    {
        get => _workingDir;
        set => Set(ref _workingDir, value, LoadFiles);
    }

    public Stack<string> Files
    {
        get => _files;
        set => Set(ref _files, value);
    }

    /// <summary>
    ///     3D Representation of Atoms
    /// </summary>
    public ObservableCollection<Atom3D> Atoms3D { get; } = new();

    /// <summary>
    ///     3D Representation of Bonds
    /// </summary>
    public ObservableCollection<Bond3D> Bonds3D { get; } = new();

    private void Load(string filename)
    {
        Atoms3D.Clear();
        Bonds3D.Clear();
        CurrentMolecule = MoleculeFactory.Create(filename);
        foreach (var atom in CurrentMolecule.Atoms) Atoms3D.Add(new Atom3D(atom));
        foreach (var bond in CurrentMolecule.Bonds) Bonds3D.Add(new Bond3D(bond));
    }
    private void LoadFiles()
    {
        var supported = new[] {"cif", "xyz", "mol2"};
        if (!Directory.Exists(WorkingDir) || string.IsNullOrEmpty(WorkingDir)) return;
        Files = new Stack<string>(supported.Select(ext => "*." + ext)
                                           .SelectMany(f => Directory.EnumerateFiles(WorkingDir, f,
                                                           SearchOption.TopDirectoryOnly)).Reverse()
                                 );
        Next();
    }

    public void Next()
    {
        Load(Files.Peek());
        OnPropertyChanged(nameof(Files));
    }
}