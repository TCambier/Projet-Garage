using System;
using System.Collections.Generic;
using System.Windows.Controls;

public class NavigationService
{
    private readonly Frame _frame;

    // --- Nouveau dictionnaire pour enregistrer les pages ---
    private readonly Dictionary<string, Func<Page>> _routes =
        new Dictionary<string, Func<Page>>();

    public NavigationService(Frame frame)
    {
        _frame = frame;
    }

    // --- Navigation classique (déjà existante) ---
    public void Navigate(object page) => _frame.Navigate(page);

    public void GoBack()
    {
        if (_frame.CanGoBack)
            _frame.GoBack();
    }

    // --- AJOUT #1 : enregistrer une page ---
    public void Register(string key, Func<Page> ctor)
    {
        _routes[key] = ctor;
    }

    // --- AJOUT #2 : naviguer depuis une clé ---
    public void NavigateTo(string key)
    {
        if (_routes.TryGetValue(key, out var ctor))
        {
            Navigate(ctor());
        }
        else
        {
            throw new Exception($"No route registered for '{key}'");
        }
    }
}
