using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Windows;

namespace CGEasy.App.ViewModels;

public partial class BilanciViewModel : ObservableObject
{
    [RelayCommand]
    private void OpenBilancioContabile()
    {
        try
        {
            var window = new Window
            {
                Title = "📊 Bilancio Contabile",
                Content = new Views.ImportBilancioView(),
                Width = 1400,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };
            
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura Bilancio Contabile:\n{ex.Message}", 
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenTemplate()
    {
        try
        {
            var window = new Window
            {
                Title = "📄 Template Riclassificazione",
                Content = new Views.ImportBilancioTemplateView(),
                Width = 1400,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };
            
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura Template:\n{ex.Message}", 
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenAssociazioni()
    {
        try
        {
            var window = new Window
            {
                Title = "🔗 Associazioni Mastrini",
                Content = new Views.AssociazioniMastriniView(),
                Width = 1400,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };
            
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura Associazioni:\n{ex.Message}", 
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenGrafici()
    {
        try
        {
            var window = new Window
            {
                Title = "📈 Grafici",
                Content = new Views.GraficiView(),
                Width = 1400,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };
            
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura Grafici:\n{ex.Message}", 
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenStatisticheCE()
    {
        try
        {
            var window = new Window
            {
                Title = "📊 Statistiche Bilanci CE",
                Content = new Views.StatisticheBilanciCEView(),
                Width = 1400,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };
            
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura Statistiche CE:\n{ex.Message}", 
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenStatisticheSP()
    {
        try
        {
            var window = new Window
            {
                Title = "📊 Statistiche Bilanci SP",
                Content = new Views.StatisticheBilanciSPView(),
                Width = 1400,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };
            
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura Statistiche SP:\n{ex.Message}", 
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void OpenIndici()
    {
        try
        {
            var window = new Window
            {
                Title = "📐 Indici di Bilancio",
                Content = new Views.IndiciDiBilancioView(),
                Width = 1400,
                Height = 800,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowState = WindowState.Maximized
            };
            
            window.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Errore apertura Indici:\n{ex.Message}", 
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ShowComingSoon(string moduleName)
    {
        MessageBox.Show($"Il modulo '{moduleName}' è in fase di sviluppo e sarà disponibile a breve!", 
            "Funzionalità in Arrivo", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}

