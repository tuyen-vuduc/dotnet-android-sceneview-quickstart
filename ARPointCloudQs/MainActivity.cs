using AndroidX.AppCompat.App;

namespace ARPointCloudQs
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/Theme.SceneViewSample", Exported = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity);
        }
    }
}