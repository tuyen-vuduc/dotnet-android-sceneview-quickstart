using Android.Views;
using AndroidX.AppCompat.App;
using AndroidX.Core.Graphics;
using AndroidX.Core.View;

namespace SceneViewQs
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Theme = "@style/Theme.SceneViewSample")]
    public class MainActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity);

            setFullScreen(
                FindViewById(Resource.Id.rootView),
                fullScreen: true,
                hideSystemBars: false,
                fitsSystemWindows: false
            );

            var toolbar = FindViewById<AndroidX.AppCompat.Widget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            toolbar.doOnApplyWindowInsets((systemBarsInsets) =>
            {
                (toolbar.LayoutParameters as ViewGroup.MarginLayoutParams).TopMargin = systemBarsInsets.Top;
                toolbar.Title = string.Empty;
            });

            var fragmentTransaction = SupportFragmentManager.BeginTransaction();
            fragmentTransaction.Add(Resource.Id.containerFragment, new MainFragment(), new Bundle());
            fragmentTransaction.Commit();
        }

        void setFullScreen(
            View rootView,
            bool fullScreen = true,
            bool hideSystemBars = true,
            bool fitsSystemWindows = true
        )
        {
            rootView.ViewTreeObserver.WindowFocusChange += (sender, e) =>
            {
                if (e.HasFocus)
                {
                    WindowCompat.SetDecorFitsSystemWindows(Window, fitsSystemWindows);
                    var wicc = new WindowInsetsControllerCompat(Window, rootView);

                    if (hideSystemBars)
                    {
                        if (fullScreen)
                        {
                            wicc.Hide(
                                WindowInsetsCompat.Type.StatusBars() |
                                WindowInsetsCompat.Type.NavigationBars()
                            );
                        }
                        else
                        {
                            wicc.Show(
                                WindowInsetsCompat.Type.StatusBars() |
                                WindowInsetsCompat.Type.NavigationBars()
                            );
                        }

                        wicc.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
                    }
                }
            };
        }
    }

    public static class ViewExtensions
    {
        public static void doOnApplyWindowInsets(this View view, Action<Insets> action)
        {
            view.ViewAttachedToWindow += (s, e) =>
            {
                ViewCompat.SetOnApplyWindowInsetsListener(view, new XOnApplyWindowInsetsListener(action));
            };
        }

        sealed class XOnApplyWindowInsetsListener : Java.Lang.Object, IOnApplyWindowInsetsListener
        {
            private Action<Insets> action;

            public XOnApplyWindowInsetsListener(Action<Insets> action)
            {
                this.action = action;
            }

            public WindowInsetsCompat OnApplyWindowInsets(View v, WindowInsetsCompat insets)
            {
                action(insets.GetInsets(WindowInsetsCompat.Type.SystemBars()));

                return WindowInsetsCompat.Consumed;
            }
        }
    }
}