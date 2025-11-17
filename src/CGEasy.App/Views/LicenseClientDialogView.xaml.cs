using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.Core.Data;
using CGEasy.Core.Models;
using CGEasy.Core.Repositories;

namespace CGEasy.App.Views;

public partial class LicenseClientDialogView : Window
{
    public LicenseClient? Cliente { get; private set; }
    private readonly LicenseRepository _repository;
    private readonly int? _clienteId;

    /// <summary>
    /// Costruttore per NUOVO cliente
    /// </summary>
    public LicenseClientDialogView()
    {
        InitializeComponent();
        
        // Usa context Singleton dall'app
        var app = (App)Application.Current;
        var context = app.Services!.GetRequiredService<LiteDbContext>();
        _repository = new LicenseRepository(context);
        
        Cliente = new LicenseClient();
        DataContext = Cliente;
        Title = "➕ Nuovo Cliente Licenza";
    }

    /// <summary>
    /// Costruttore per MODIFICA cliente
    /// </summary>
    public LicenseClientDialogView(int clienteId) : this()
    {
        _clienteId = clienteId;
        var cliente = _repository.GetClientById(clienteId);
        if (cliente != null)
        {
            Cliente = cliente;
            DataContext = Cliente;
            Title = $"✏️ Modifica Cliente: {cliente.NomeCliente}";
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        // Validazione
        if (string.IsNullOrWhiteSpace(Cliente?.NomeCliente))
        {
            MessageBox.Show("Il nome cliente è obbligatorio!", "Attenzione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Cliente.NomeCliente.Length < 2)
        {
            MessageBox.Show("Il nome cliente deve essere di almeno 2 caratteri!", "Attenzione",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

