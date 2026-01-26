using GarageApp.ViewModels;

public class MainViewModel
{
    public LoginViewModel CurrentView { get; set; }

    public MainViewModel()
    {
        CurrentView = new LoginViewModel();
    }
}
