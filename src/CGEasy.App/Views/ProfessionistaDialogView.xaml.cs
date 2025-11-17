using System;
using System.Windows;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;

namespace CGEasy.App.Views;

/// <summary>
/// Dialog per creazione/modifica professionista
/// </summary>
public partial class ProfessionistaDialogView : Window
{
    private readonly ProfessionistaRepository _professionistaRepository;
    private readonly bool _isEditMode;

    public Professionista Professionista { get; private set; }
    public bool IsSaved { get; private set; }
    public string WindowTitle { get; }

    /// <summary>
    /// Constructor per NUOVO professionista
    /// </summary>
    public ProfessionistaDialogView(ProfessionistaRepository professionistaRepository)
    {
        InitializeComponent();
        
        _professionistaRepository = professionistaRepository;
        _isEditMode = false;
        
        Professionista = new Professionista 
        { 
            Attivo = true,
            DataAttivazione = DateTime.Now
        };
        
        WindowTitle = "Nuovo Professionista";
        DataContext = this;
    }

    /// <summary>
    /// Constructor per MODIFICA professionista esistente
    /// </summary>
    public ProfessionistaDialogView(ProfessionistaRepository professionistaRepository, Professionista professionistaToEdit)
    {
        InitializeComponent();
        
        _professionistaRepository = professionistaRepository;
        _isEditMode = true;
        
        // Clone del professionista per editing
        Professionista = CloneProfessionista(professionistaToEdit);
        
        WindowTitle = $"Modifica Professionista: {Professionista.NomeCompleto}";
        DataContext = this;
    }

    /// <summary>
    /// Salva professionista
    /// </summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Validazione
        if (!ValidateProfessionista())
            return;

        try
        {
            if (_isEditMode)
            {
                // UPDATE
                Professionista.UpdatedAt = DateTime.Now;
                Professionista.DataModifica = DateTime.Now;
                
                var success = _professionistaRepository.Update(Professionista);
                
                if (success)
                {
                    IsSaved = true;
                    MessageBox.Show("Professionista aggiornato con successo!", 
                                  "Successo", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Errore durante l'aggiornamento del professionista.", 
                                  "Errore", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                }
            }
            else
            {
                // INSERT
                Professionista.CreatedAt = DateTime.Now;
                Professionista.UpdatedAt = DateTime.Now;
                Professionista.DataAttivazione = DateTime.Now;
                Professionista.DataModifica = DateTime.Now;
                
                var newId = _professionistaRepository.Insert(Professionista);
                
                if (newId > 0)
                {
                    Professionista.Id = newId;
                    IsSaved = true;
                    MessageBox.Show("Professionista creato con successo!", 
                                  "Successo", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Errore durante la creazione del professionista.", 
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
    /// Valida i dati del professionista
    /// </summary>
    private bool ValidateProfessionista()
    {
        // Nome obbligatorio
        if (string.IsNullOrWhiteSpace(Professionista.Nome))
        {
            MessageBox.Show("Il Nome è obbligatorio.", 
                          "Validazione", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Warning);
            return false;
        }

        // Cognome obbligatorio
        if (string.IsNullOrWhiteSpace(Professionista.Cognome))
        {
            MessageBox.Show("Il Cognome è obbligatorio.", 
                          "Validazione", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Warning);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Clona professionista per editing
    /// </summary>
    private Professionista CloneProfessionista(Professionista source)
    {
        return new Professionista
        {
            Id = source.Id,
            Nome = source.Nome,
            Cognome = source.Cognome,
            Attivo = source.Attivo,
            DataAttivazione = source.DataAttivazione,
            DataModifica = source.DataModifica,
            DataCessazione = source.DataCessazione,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }
}

