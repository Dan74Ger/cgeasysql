using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using CGEasy.App.ViewModels;
using CGEasy.Core.Data;

namespace CGEasy.App.Views;

public partial class LicenseManagerView : Window
{
    public LicenseManagerView()
    {
        InitializeComponent();
        
        try
        {
            // Ottiene il context Singleton dall'app tramite DI
            var app = (App)Application.Current;
            var serviceProvider = app.Services;
            
            if (serviceProvider == null)
            {
                MessageBox.Show("Errore: ServiceProvider non disponibile", "Errore", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            var context = serviceProvider.GetRequiredService<LiteDbContext>();
            
            // Inizializza ViewModel con context condiviso
            DataContext = new LicenseManagerViewModel(context);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Errore durante l'inizializzazione della gestione licenze:\n\n{ex.Message}\n\n" +
                $"Stack Trace:\n{ex.StackTrace}",
                "Errore",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            
            Close();
        }
    }
}

