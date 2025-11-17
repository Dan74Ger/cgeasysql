using System;
using System.Windows;
using System.Windows.Controls;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;

namespace CGEasy.App.Views;

/// <summary>
/// Dialog per creazione/modifica tipo pratica
/// </summary>
public partial class TipoPraticaDialogView : Window
{
    private readonly TipoPraticaRepository _tipoPraticaRepository;
    private readonly bool _isEditMode;

    public TipoPratica Pratica { get; private set; }
    public bool IsSaved { get; private set; }
    public string WindowTitle { get; }

    /// <summary>
    /// Constructor per NUOVA pratica
    /// </summary>
    public TipoPraticaDialogView(TipoPraticaRepository tipoPraticaRepository)
    {
        InitializeComponent();
        
        _tipoPraticaRepository = tipoPraticaRepository;
        _isEditMode = false;
        
        Pratica = new TipoPratica 
        { 
            Attivo = true,
            PrioritaDefault = 2,
            Ordine = 0,
            Categoria = CategoriaPratica.Altra
        };
        
        WindowTitle = "Nuova Pratica";
        DataContext = this;
    }

    /// <summary>
    /// Constructor per MODIFICA pratica esistente
    /// </summary>
    public TipoPraticaDialogView(TipoPraticaRepository tipoPraticaRepository, TipoPratica praticaToEdit)
    {
        InitializeComponent();
        
        _tipoPraticaRepository = tipoPraticaRepository;
        _isEditMode = true;
        
        // Clone della pratica per editing
        Pratica = ClonePratica(praticaToEdit);
        
        WindowTitle = $"Modifica Pratica: {Pratica.NomePratica}";
        DataContext = this;
    }

    /// <summary>
    /// Salva pratica
    /// </summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Validazione
        if (!ValidatePratica())
            return;

        try
        {
            if (_isEditMode)
            {
                // UPDATE
                Pratica.UpdatedAt = DateTime.Now;
                
                var success = _tipoPraticaRepository.Update(Pratica);
                
                if (success)
                {
                    IsSaved = true;
                    MessageBox.Show("Pratica aggiornata con successo!", 
                                  "Successo", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Errore durante l'aggiornamento della pratica.", 
                                  "Errore", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                }
            }
            else
            {
                // INSERT
                Pratica.CreatedAt = DateTime.Now;
                Pratica.UpdatedAt = DateTime.Now;
                
                var newId = _tipoPraticaRepository.Insert(Pratica);
                
                if (newId > 0)
                {
                    Pratica.Id = newId;
                    IsSaved = true;
                    MessageBox.Show("Pratica creata con successo!", 
                                  "Successo", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Errore durante la creazione della pratica.", 
                                  "Errore", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore durante il salvataggio: {ex.Message}", 
                          "Errore", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Annulla modifiche
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        IsSaved = false;
        DialogResult = false;
        Close();
    }

    /// <summary>
    /// Valida i dati della pratica
    /// </summary>
    private bool ValidatePratica()
    {
        // Nome Pratica obbligatorio
        if (string.IsNullOrWhiteSpace(Pratica.NomePratica))
        {
            MessageBox.Show("Il Nome Pratica è obbligatorio.", 
                          "Validazione", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Warning);
            return false;
        }

        // Durata se specificata deve essere positiva
        if (Pratica.DurataStimataGiorni.HasValue && Pratica.DurataStimataGiorni.Value <= 0)
        {
            MessageBox.Show("La Durata Stimata deve essere maggiore di 0.", 
                          "Validazione", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Warning);
            return false;
        }

        // Priorità valida
        if (Pratica.PrioritaDefault < 1 || Pratica.PrioritaDefault > 3)
        {
            MessageBox.Show("La Priorità deve essere tra 1 e 3.", 
                          "Validazione", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Clona pratica per editing
    /// </summary>
    private TipoPratica ClonePratica(TipoPratica source)
    {
        return new TipoPratica
        {
            Id = source.Id,
            NomePratica = source.NomePratica,
            Descrizione = source.Descrizione,
            Categoria = source.Categoria,
            PrioritaDefault = source.PrioritaDefault,
            DurataStimataGiorni = source.DurataStimataGiorni,
            Attivo = source.Attivo,
            Ordine = source.Ordine,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }
}

