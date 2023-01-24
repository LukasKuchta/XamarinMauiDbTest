using DatabaseTest;

namespace LiteDbTest;

public partial class MainPage : ContentPage
{	

	public MainPage()
	{
		InitializeComponent();
	}

	private async void OnCounterClicked(object sender, EventArgs e)
	{
		// run test
		await Task.Run(() => {
            PerformanceTest.TestDatabase();
        });
		
    }
}

