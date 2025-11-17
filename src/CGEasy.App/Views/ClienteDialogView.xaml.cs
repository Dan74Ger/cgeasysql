using System;
using System.Windows;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;

namespace CGEasy.App.Views;

/// <summary>
/// Dialog per creazione/modifica cliente
/// </summary>
public partial class ClienteDialogView : Window
{
    private readonly ClienteRepository _clienteRepository;
    private readonly bool _isEditMode;

    public Cliente Cliente { get; private set; }
    public bool IsSaved { get; private set; }
    public string WindowTitle { get; }

    /// <summary>
    /// Constructor per NUOVO cliente
    /// </summary>
    public ClienteDialogView(ClienteRepository clienteRepository)
    {
        InitializeComponent();
        
        _clienteRepository = clienteRepository;
        _isEditMode = false;
        
        Cliente = new Cliente 
        { 
            Attivo = true,
            DataAttivazione = DateTime.Now
        };
        
        WindowTitle = "Nuovo Cliente";
        DataContext = this;
    }

    /// <summary>
    /// Constructor per MODIFICA cliente esistente
    /// </summary>
    public ClienteDialogView(ClienteRepository clienteRepository, Cliente clienteToEdit)
    {
        InitializeComponent();
        
        _clienteRepository = clienteRepository;
        _isEditMode = true;
        
        // Clone del cliente per editing
        Cliente = CloneCliente(clienteToEdit);
        
        WindowTitle = $"Modifica Cliente: {Cliente.NomeCliente}";
        DataContext = this;
    }

    /// <summary>
    /// Salva cliente
    /// </summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Validazione
        if (!ValidateCliente())
            return;

        try
        {
            if (_isEditMode)
            {
                // UPDATE
                Cliente.UpdatedAt = DateTime.Now;
                Cliente.DataModifica = DateTime.Now;
                
                var success = _clienteRepository.Update(Cliente);
                
                if (success)
                {
                    IsSaved = true;
                    MessageBox.Show("Cliente aggiornato con successo!", 
                                  "Successo", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Errore durante l'aggiornamento del cliente.", 
                                  "Errore", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Error);
                }
            }
            else
            {
                // INSERT
                Cliente.CreatedAt = DateTime.Now;
                Cliente.UpdatedAt = DateTime.Now;
                Cliente.DataAttivazione = DateTime.Now;
                Cliente.DataModifica = DateTime.Now;
                
                var newId = _clienteRepository.Insert(Cliente);
                
                if (newId > 0)
                {
                    Cliente.Id = newId;
                    IsSaved = true;
                    MessageBox.Show("Cliente creato con successo!", 
                                  "Successo", 
                                  MessageBoxButton.OK, 
                                  MessageBoxImage.Information);
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Errore durante la creazione del cliente.", 
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
    /// Valida i dati del cliente
    /// </summary>
    private bool ValidateCliente()
    {
        // Nome Cliente obbligatorio
        if (string.IsNullOrWhiteSpace(Cliente.NomeCliente))
        {
            MessageBox.Show("Il Nome Cliente è obbligatorio.", 
                          "Validazione", 
                          MessageBoxButton.OK, 
                          MessageBoxImage.Warning);
            return false;
        }

        // Email format (se presente)
        if (!string.IsNullOrWhiteSpace(Cliente.MailCliente))
        {
            if (!IsValidEmail(Cliente.MailCliente))
            {
                MessageBox.Show("L'indirizzo email non è valido.", 
                              "Validazione", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return false;
            }
        }

        // P.IVA format (se presente) - deve essere 11 cifre
        if (!string.IsNullOrWhiteSpace(Cliente.PivaCliente))
        {
            var piva = Cliente.PivaCliente.Replace("IT", "").Trim();
            if (piva.Length != 11 || !IsNumeric(piva))
            {
                MessageBox.Show("La Partita IVA deve essere di 11 cifre numeriche.", 
                              "Validazione", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return false;
            }
        }

        // CF format (se presente) - deve essere 16 caratteri
        if (!string.IsNullOrWhiteSpace(Cliente.CfCliente))
        {
            if (Cliente.CfCliente.Length != 16)
            {
                MessageBox.Show("Il Codice Fiscale deve essere di 16 caratteri.", 
                              "Validazione", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return false;
            }
        }

        // CAP format (se presente) - deve essere 5 cifre
        if (!string.IsNullOrWhiteSpace(Cliente.Cap))
        {
            if (Cliente.Cap.Length != 5 || !IsNumeric(Cliente.Cap))
            {
                MessageBox.Show("Il CAP deve essere di 5 cifre numeriche.", 
                              "Validazione", 
                              MessageBoxButton.OK, 
                              MessageBoxImage.Warning);
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Valida email
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifica se stringa è numerica
    /// </summary>
    private bool IsNumeric(string value)
    {
        return long.TryParse(value, out _);
    }

    /// <summary>
    /// Clona cliente per editing
    /// </summary>
    private Cliente CloneCliente(Cliente source)
    {
        return new Cliente
        {
            Id = source.Id,
            NomeCliente = source.NomeCliente,
            IdProfessionista = source.IdProfessionista,
            MailCliente = source.MailCliente,
            CfCliente = source.CfCliente,
            PivaCliente = source.PivaCliente,
            CodiceAteco = source.CodiceAteco,
            Indirizzo = source.Indirizzo,
            Citta = source.Citta,
            Provincia = source.Provincia,
            Cap = source.Cap,
            LegaleRappresentante = source.LegaleRappresentante,
            CfLegaleRappresentante = source.CfLegaleRappresentante,
            Attivo = source.Attivo,
            DataAttivazione = source.DataAttivazione,
            DataModifica = source.DataModifica,
            DataCessazione = source.DataCessazione,
            CreatedAt = source.CreatedAt,
            UpdatedAt = source.UpdatedAt
        };
    }
}

