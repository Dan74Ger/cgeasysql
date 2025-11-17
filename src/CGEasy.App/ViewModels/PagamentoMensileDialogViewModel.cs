using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;
using CGEasy.Core.Services;

namespace CGEasy.App.ViewModels
{
    public partial class PagamentoMensileDialogViewModel : ObservableObject
    {
        private readonly LiteDbContext _context;
        private readonly int _bancaId;
        private readonly BancaPagamentoRepository _pagamentoRepo;
        private readonly AuditLogService _auditService;

        [ObservableProperty] private string _nomeFornitore = string.Empty;
        [ObservableProperty] private int _anno = DateTime.Now.Year;
        [ObservableProperty] private int _numeroMesi = 12;
        [ObservableProperty] private decimal _importoBase = 0;
        [ObservableProperty] private DateTime _dataScadenzaBase = DateTime.Now;
        [ObservableProperty] private string _descrizione = string.Empty;
        [ObservableProperty] private bool? _dialogResult;

        [ObservableProperty] private ObservableCollection<PagamentoMensileRow> _righe = new();

        public PagamentoMensileDialogViewModel(LiteDbContext context, int bancaId)
        {
            _context = context;
            _bancaId = bancaId;
            _pagamentoRepo = new BancaPagamentoRepository(_context);
            _auditService = new AuditLogService(_context);
        }

        [RelayCommand]
        private void GeneraRighe()
        {
            if (string.IsNullOrWhiteSpace(NomeFornitore))
            {
                MessageBox.Show("Inserisci il nome del fornitore.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NumeroMesi <= 0 || NumeroMesi > 60)
            {
                MessageBox.Show("Il numero di mesi deve essere compreso tra 1 e 60.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ImportoBase <= 0)
            {
                MessageBox.Show("L'importo base deve essere maggiore di zero.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Righe.Clear();

            // Genera le righe per ogni mese
            for (int i = 0; i < NumeroMesi; i++)
            {
                var dataScadenza = DataScadenzaBase.AddMonths(i);
                var nomeMese = System.Globalization.CultureInfo.GetCultureInfo("it-IT").DateTimeFormat.GetMonthName(dataScadenza.Month);
                
                Righe.Add(new PagamentoMensileRow
                {
                    Mese = dataScadenza.Month,
                    Anno = dataScadenza.Year,
                    NomeMese = $"{nomeMese} {dataScadenza.Year}",
                    Importo = ImportoBase,
                    DataScadenza = dataScadenza,
                    Descrizione = Descrizione
                });
            }
        }

        [RelayCommand]
        private void Salva(Window window)
        {
            if (Righe.Count == 0)
            {
                MessageBox.Show("Genera prima le righe dei pagamenti mensili.", "Attenzione", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Salva ogni riga come un pagamento separato
                foreach (var riga in Righe)
                {
                    var pagamento = new BancaPagamento
                    {
                        BancaId = _bancaId,
                        NomeFornitore = NomeFornitore,
                        Anno = riga.Anno,
                        Mese = riga.Mese,
                        Importo = riga.Importo,
                        DataScadenza = riga.DataScadenza,
                        Pagato = false,
                        DataPagamentoEffettivo = null,
                        Note = riga.Descrizione,
                        PercentualeAnticipo = 0,
                        DataInizioAnticipo = null,
                        DataScadenzaAnticipo = null,
                        NumeroFatturaFornitore = string.Empty,
                        DataFatturaFornitore = null
                    };

                    _pagamentoRepo.Insert(pagamento);
                }

                // Log audit
                _auditService.LogFromSession("CREATE_PAGAMENTI_MENSILI", "BANCHE", 
                    _bancaId, 
                    $"Creati {Righe.Count} pagamenti mensili per {NomeFornitore} (Anno {Anno}, Tot: {Helpers.CurrencyHelper.FormatEuro(Righe.Sum(r => r.Importo))})");

                DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio: {ex.Message}", "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void Annulla(Window window)
        {
            DialogResult = false;
            window.Close();
        }
    }

    // Classe per rappresentare una riga nella griglia di generazione
    public partial class PagamentoMensileRow : ObservableObject
    {
        [ObservableProperty] private int _mese;
        [ObservableProperty] private int _anno;
        [ObservableProperty] private string _nomeMese = string.Empty;
        [ObservableProperty] private decimal _importo;
        [ObservableProperty] private DateTime _dataScadenza;
        [ObservableProperty] private string _descrizione = string.Empty;
    }
}

