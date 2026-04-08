using System.Windows;
using System.Windows.Controls;

namespace ReqForge.Helpers;

public static class BindablePasswordBox
{
    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(BindablePasswordBox),
            new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

    public static readonly DependencyProperty BindPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BindPassword",
            typeof(bool),
            typeof(BindablePasswordBox),
            new PropertyMetadata(false, OnBindPasswordChanged));

    private static readonly DependencyProperty UpdatingPasswordProperty =
        DependencyProperty.RegisterAttached(
            "UpdatingPassword",
            typeof(bool),
            typeof(BindablePasswordBox),
            new PropertyMetadata(false));

    public static string GetBoundPassword(DependencyObject dp) =>
        (string)dp.GetValue(BoundPasswordProperty);

    public static void SetBoundPassword(DependencyObject dp, string value) =>
        dp.SetValue(BoundPasswordProperty, value);

    public static bool GetBindPassword(DependencyObject dp) =>
        (bool)dp.GetValue(BindPasswordProperty);

    public static void SetBindPassword(DependencyObject dp, bool value) =>
        dp.SetValue(BindPasswordProperty, value);

    private static bool GetUpdatingPassword(DependencyObject dp) =>
        (bool)dp.GetValue(UpdatingPasswordProperty);

    private static void SetUpdatingPassword(DependencyObject dp, bool value) =>
        dp.SetValue(UpdatingPasswordProperty, value);

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox || !GetBindPassword(passwordBox))
            return;

        passwordBox.PasswordChanged -= HandlePasswordChanged;
        if (!GetUpdatingPassword(passwordBox))
            passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
        passwordBox.PasswordChanged += HandlePasswordChanged;
    }

    private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox passwordBox)
            return;

        if ((bool)e.OldValue)
            passwordBox.PasswordChanged -= HandlePasswordChanged;

        if ((bool)e.NewValue)
            passwordBox.PasswordChanged += HandlePasswordChanged;
    }

    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox passwordBox)
            return;

        SetUpdatingPassword(passwordBox, true);
        SetBoundPassword(passwordBox, passwordBox.Password);
        SetUpdatingPassword(passwordBox, false);
    }
}
