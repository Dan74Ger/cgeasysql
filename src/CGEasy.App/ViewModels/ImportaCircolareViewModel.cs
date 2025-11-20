using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;

namespace CGEasy.App.ViewModels
{
    /// <summary>
    /// ViewModel per importazione circolari
    /// </summary>
    public partial class ImportaCircolareViewModel : ObservableObject
    {
        private readonly CircolariService _service;
        private readonly ArgomentiRepository _argomentiRepo;
        private readonly AuditLogService _auditService;

        [ObservableProperty]
        private ObservableCollection<Argomento> _argomentiDisponibili = new();

        [ObservableProperty]
        private Argomento? _argomentoSelezionato;

        [ObservableProperty]
        private string _descrizione = string.Empty;

        [ObservableProperty]
        private int _anno = DateTime.Now.Year;

        [ObservableProperty]
        private string? _fileSelezionato;

        [ObservableProperty]
        private string _nomeFileSelezionato = "Nessun file selezionato";

        public ImportaCircolareViewModel()
        {
            // Constructor per XAML Designer
            var app = (App)Application.Current;
            var context = app.Services!.GetRequiredService<CGEasyDbContext>();
            _service = new CircolariService(context);
            _argomentiRepo = new ArgomentiRepository(context);
            _auditService = new AuditLogService(context);

            LoadArgomenti();
        }

        public ImportaCircolareViewModel(CGEasyDbContext context)
        {
            _service = new CircolariService(context);
            _argomentiRepo = new ArgomentiRepository(context);
            _auditService = new AuditLogService(context);

            LoadArgomenti();
        }

        private void LoadArgomenti()
        {
            try
            {
                var lista = _argomentiRepo.GetAll();
                ArgomentiDisponibili = new ObservableCollection<Argomento>(lista);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore caricamento argomenti:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Ricarica lista argomenti (chiamato quando cambia tab)
        /// </summary>
        public void RefreshArgomenti()
        {
            LoadArgomenti();
        }

        [RelayCommand]
        private void SelezionaFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Seleziona file da importare",
                Filter = "Tutti i file supportati|*.pdf;*.docx;*.doc;*.xlsx;*.xls|" +
                         "PDF (*.pdf)|*.pdf|" +
                         "Word (*.docx;*.doc)|*.docx;*.doc|" +
                         "Excel (*.xlsx;*.xls)|*.xlsx;*.xls|" +
                         "Tutti i file (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FileSelezionato = openFileDialog.FileName;
                NomeFileSelezionato = Path.GetFileName(FileSelezionato);
            }
        }

        [RelayCommand]
        private void ImportaCircolare()
        {
            // Validazione
            if (ArgomentoSelezionato == null)
            {
                MessageBox.Show("Seleziona un argomento", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(Descrizione))
            {
                MessageBox.Show("Inserisci una descrizione", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Anno < 1900 || Anno > 2100)
            {
                MessageBox.Show("Inserisci un anno valido", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(FileSelezionato) || !File.Exists(FileSelezionato))
            {
                MessageBox.Show("Seleziona un file valido", 
                    "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var userId = SessionManager.CurrentUser?.Id ?? 0;

                var circolareId = _service.ImportaCircolare(
                    ArgomentoSelezionato.Id,
                    Descrizione.Trim(),
                    Anno,
                    FileSelezionato,
                    userId
                );

                _auditService.Log(
                    userId,
                    SessionManager.CurrentUsername,
                    "CIRCOLARE_IMPORT",
                    "Circolari",
                    circolareId,
                    $"Importata circolare: {NomeFileSelezionato} - {Descrizione}"
                );

                MessageBox.Show($"Circolare '{NomeFileSelezionato}' importata con successo!\n\n" +
                    $"Argomento: {ArgomentoSelezionato.Nome}\n" +
                    $"Anno: {Anno}", 
                    "Successo", MessageBoxButton.OK, MessageBoxImage.Information);

                // Reset form
                Descrizione = string.Empty;
                FileSelezionato = null;
                NomeFileSelezionato = "Nessun file selezionato";
                ArgomentoSelezionato = null;
                Anno = DateTime.Now.Year;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante l'importazione:\n{ex.Message}", 
                    "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Reset()
        {
            Descrizione = string.Empty;
            FileSelezionato = null;
            NomeFileSelezionato = "Nessun file selezionato";
            ArgomentoSelezionato = null;
            Anno = DateTime.Now.Year;
        }
    }
}

