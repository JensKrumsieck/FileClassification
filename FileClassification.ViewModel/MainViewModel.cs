﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using ChemSharp.Molecules;
using ChemSharp.Molecules.HelixToolkit;
using TinyMVVM;

namespace FileClassification.ViewModel;

public class MainViewModel : BaseViewModel
{
    [JsonIgnore] public override string Title { get; set; } = "";

    [JsonIgnore]
    public ObservableCollection<string> Classes { get; } = new();

    private Stack<string> _files;
    private string _workingDir = "";

    private Molecule _currentMolecule;

    [JsonIgnore]
    public Molecule CurrentMolecule
    {
        get => _currentMolecule;
        set => Set(ref _currentMolecule, value);
    }

    private string _metal = "";
    
    public string Metal
    {
        get => _metal;
        set => Set(ref _metal, value);
    }

    private string _axialLigand = "";
    public string AxialLigand
    {
        get => _axialLigand;
        set => Set(ref _axialLigand, value);
    }
    private string _ligand = "";
    public string Ligand
    {
        get => _ligand;
        set => Set(ref _ligand, value);
    }
    
    private string _coSolv = "";
    public string CoSolv
    {
        get => _coSolv;
        set => Set(ref _coSolv, value);
    }

    private int _coordNo = 0;
    public  int CoordNo
    {
        get => _coordNo;
        set => Set(ref _coordNo, value);
    }
    
    public string Group { get;set; }
    
    private int _substNo = 0;
    public  int SubstNo
    {
        get => _substNo;
        set => Set(ref _substNo, value);
    }

    
    [JsonIgnore]
    public string WorkingDir
    {
        get => _workingDir;
        set => Set(ref _workingDir, value, LoadFiles);
    }

    
    [JsonIgnore]
    public Stack<string> Files
    {
        get => _files;
        set => Set(ref _files, value);
    }

    /// <summary>
    ///     3D Representation of Atoms
    /// </summary>
    /// 
    [JsonIgnore]
    public ObservableCollection<Atom3D> Atoms3D { get; } = new();

    /// <summary>
    ///     3D Representation of Bonds
    /// </summary>
    /// 
    [JsonIgnore]
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