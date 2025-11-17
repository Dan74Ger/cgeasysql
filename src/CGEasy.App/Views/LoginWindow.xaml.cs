using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using CGEasy.App.ViewModels;

namespace CGEasy.App.Views;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : Window
{
    private readonly LoginViewModel _viewModel;
    private readonly DispatcherTimer _animationTimer;
    private double _angle1 = 0;
    private double _angle2 = 90;
    private double _angle3 = 180;
    private double _angle4 = 270;

    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Timer per animazione cerchi
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(30) // ~33 FPS
        };
        _animationTimer.Tick += AnimationTimer_Tick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Avvia animazioni
        _animationTimer.Start();
        
        // Focus su username
        UsernameBox.Focus();
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        // Incrementa angoli per movimento continuo
        _angle1 += 2.0;
        _angle2 += 3.0;
        _angle3 += 4.0;
        _angle4 += 2.5;

        // Calcola nuove posizioni con movimento circolare AMPIO
        var newX1 = 100 + Math.Cos(_angle1 * Math.PI / 180) * 200;
        var newY1 = 200 + Math.Sin(_angle1 * Math.PI / 180) * 200;
        
        var newX2 = 300 + Math.Cos(_angle2 * Math.PI / 180) * 250;
        var newY2 = 300 + Math.Sin(_angle2 * Math.PI / 180) * 250;
        
        var newX3 = 200 + Math.Cos(_angle3 * Math.PI / 180) * 180;
        var newY3 = 400 + Math.Sin(_angle3 * Math.PI / 180) * 180;
        
        var newX4 = 250 + Math.Cos(_angle4 * Math.PI / 180) * 220;
        var newY4 = 250 + Math.Sin(_angle4 * Math.PI / 180) * 200;

        // Applica le posizioni direttamente (senza animazione smooth per test)
        Canvas.SetLeft(Circle1, newX1);
        Canvas.SetTop(Circle1, newY1);
        
        Canvas.SetLeft(Circle2, newX2);
        Canvas.SetTop(Circle2, newY2);
        
        Canvas.SetLeft(Circle3, newX3);
        Canvas.SetTop(Circle3, newY3);
        
        Canvas.SetLeft(Circle4, newX4);
        Canvas.SetTop(Circle4, newY4);
    }

    private void AnimateCircle(System.Windows.Shapes.Ellipse circle, double toX, double toY)
    {
        var animX = new DoubleAnimation
        {
            To = toX,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var animY = new DoubleAnimation
        {
            To = toY,
            Duration = TimeSpan.FromMilliseconds(500),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        circle.BeginAnimation(System.Windows.Controls.Canvas.LeftProperty, animX);
        circle.BeginAnimation(System.Windows.Controls.Canvas.TopProperty, animY);
    }

    private void LoginButton_Click(object sender, RoutedEventArgs e)
    {
        // Passa la password al ViewModel
        _viewModel.LoginCommand.Execute(PasswordBox.Password);
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        // Login con INVIO
        if (e.Key == Key.Enter)
        {
            _viewModel.LoginCommand.Execute(PasswordBox.Password);
            e.Handled = true;
        }
    }

    private void TextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // Username focus: sfondo bianco, testo nero
        UsernameBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
        UsernameBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
    }

    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Username perde focus: torna al colore blu
        UsernameBorder.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4A90E2"));
        UsernameBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
    }

    private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
    {
        // Password focus: sfondo bianco, testo nero
        PasswordBorder.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
        PasswordBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Black);
    }

    private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
    {
        // Password perde focus: torna al colore blu
        PasswordBorder.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#4A90E2"));
        PasswordBox.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White);
    }

    protected override void OnClosed(EventArgs e)
    {
        // Ferma timer
        _animationTimer?.Stop();
        base.OnClosed(e);
    }
}

