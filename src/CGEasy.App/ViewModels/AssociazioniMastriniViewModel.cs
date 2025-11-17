using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class AssociazioniMastriniViewModel : ObservableObject
{
    private readonly LiteDbContext _context;
    private readonly AssociazioneMastrinoService _service = null!;

    [ObservableProperty]
    private ObservableCollection<AssociazioneMastrino> _associazioni = new();

    [ObservableProperty]
    private AssociazioneMastrino? _associazioneSelezionata;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private int _totalAssociazioni;

    [ObservableProperty]
    private bool _isLoading;

    public AssociazioniMastriniViewModel(LiteDbContext context)
    {
        _context = context;
        
        try
        {
            var auditService = new AuditLogService(context);
            _service = new AssociazioneMastrinoService(context, auditService);

            CaricaAssociazioni();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore inizializzazione:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    // Costruttore senza parametri per XAML designer - usa Singleton
    public AssociazioniMastriniViewModel() : this(GetOrCreateContext())
    {
    }

    private static LiteDbContext GetOrCreateContext()
    {
        var context = App.GetService<LiteDbContext>();
        if (context == null)
        {
            context = new LiteDbContext();
            context.MarkAsSingleton(); // Marca anche questo come singleton
        }
        return context;
    }

    /// <summary>
    /// Carica tutte le associazioni
    /// </summary>
    [RelayCommand]
    private void CaricaAssociazioni()
    {
        try
        {
            IsLoading = true;

            var associazioni = string.IsNullOrWhiteSpace(SearchText)
                ? _service.GetAll()
                : _service.Cerca(SearchText);

            Associazioni.Clear();
            foreach (var associazione in associazioni)
            {
                Associazioni.Add(associazione);
            }

            TotalAssociazioni = Associazioni.Count;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore caricamento associazioni:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Apre dialog per nuova associazione
    /// </summary>
    [RelayCommand]
    private void NuovaAssociazione()
    {
        try
        {
            // ✅ PATTERN TODO: La View crea il ViewModel
            var dialog = new Views.AssociazioneMastrinoDialogView();
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dialog.ShowDialog();

            if (dialog.DialogResult)
            {
                CaricaAssociazioni();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dialog:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Apre dialog per modifica associazione
    /// </summary>
    [RelayCommand]
    private void ModificaAssociazione()
    {
        if (AssociazioneSelezionata == null)
        {
            MessageBox.Show("Seleziona un'associazione da modificare.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            // ✅ PATTERN TODO: La View crea il ViewModel e passa l'ID
            var dialog = new Views.AssociazioneMastrinoDialogView(AssociazioneSelezionata.Id);
            dialog.Owner = Application.Current.MainWindow;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            dialog.ShowDialog();

            if (dialog.DialogResult)
            {
                CaricaAssociazioni();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura dialog:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Elimina associazione selezionata
    /// </summary>
    [RelayCommand]
    private void EliminaAssociazione()
    {
        if (AssociazioneSelezionata == null)
        {
            MessageBox.Show("Seleziona un'associazione da eliminare.",
                "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Sei sicuro di voler eliminare l'associazione:\n\n" +
            $"{AssociazioneSelezionata.DescrizioneCompleta}?\n\n" +
            $"Questa operazione eliminerà anche tutte le mappature associate.",
            "Conferma Eliminazione",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            _service.EliminaAssociazione(AssociazioneSelezionata.Id);
            CaricaAssociazioni();

            MessageBox.Show("Associazione eliminata con successo!",
                "Successo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore eliminazione associazione:\n{ex.Message}",
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Cerca associazioni
    /// </summary>
    [RelayCommand]
    private void Cerca()
    {
        CaricaAssociazioni();
    }

    /// <summary>
    /// Pulisce ricerca
    /// </summary>
    [RelayCommand]
    private void PulisciRicerca()
    {
        SearchText = string.Empty;
        CaricaAssociazioni();
    }

    /// <summary>
    /// Gestisce doppio click su riga
    /// </summary>
    [RelayCommand]
    private void RigaDoppioClick()
    {
        if (AssociazioneSelezionata != null)
        {
            ModificaAssociazione();
        }
    }
}

