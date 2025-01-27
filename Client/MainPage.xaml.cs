using Client.ViewModels;

namespace Client
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            this.BindingContext = new MainVM();
        }


    }

}
