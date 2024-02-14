using Android.Views;
using Com.Google.Android.Filament;
using Com.Google.Android.Filament.Gltfio;
using Com.Google.Android.Filament.Utils;
using IO.Github.Sceneview;
using IO.Github.Sceneview.Loaders;
using IO.Github.Sceneview.Nodes;
using Kotlin.Coroutines;
using Kotlin.Jvm.Functions;
using Xamarin.KotlinX.Coroutines;
using View = Android.Views.View;

namespace ModelViewerQs;

public partial class MainFragment : AndroidX.Fragment.App.Fragment
{
    SceneView sceneView;
    View loadingView;

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        return inflater.Inflate(Resource.Layout.fragment_main, container, false);
    }

    public override void OnViewCreated(View view, Bundle? savedInstanceState)
    {
        base.OnViewCreated(view, savedInstanceState);

        sceneView = view.FindViewById<SceneView>(Resource.Id.sceneView);
        sceneView.SetLifecycle(Lifecycle);

        loadingView = view.FindViewById(Resource.Id.loadingView);

        var lifeCycleScope = AndroidX.Lifecycle.LifecycleOwnerKt.GetLifecycleScope(this);

        lifeCycleScope.LaunchWhenCreated(this);
    }
}

class HdrSkyboxBuilder : Java.Lang.Object, IFunction1
{
    public Java.Lang.Object? Invoke(Java.Lang.Object? p0)
    {
        if (p0 is not Skybox.Builder builder) return null;

        return builder.Intensity(50_000f);
    }
}

class HdrIndirectLightBuilder : Java.Lang.Object, IFunction1
{
    public Java.Lang.Object? Invoke(Java.Lang.Object? p0)
    {
        if (p0 is not IndirectLight.Builder builder) return null;

        return builder.Intensity(30_000f);
    }
}

class XResult : Java.Lang.Object, IFunction1
{
    private readonly Action<Java.Lang.Object> action;

    public XResult(Action<Java.Lang.Object> action = default)
    {
        this.action = action;
    }

    public Java.Lang.Object? Invoke(Java.Lang.Object? p0)
    {
        action?.Invoke(p0);

        return null;
    }
}

partial class XLoadModel : Java.Lang.Object, IFunction1
{
    private readonly string baseFileName;

    public XLoadModel(string baseFileName)
    {
        this.baseFileName = baseFileName;
    }

    public Java.Lang.Object? Invoke(Java.Lang.Object? p0)
    {
        return $"{baseFileName.TrimEnd('/')}/{p0}";
    }
}

partial class MainFragment : IFunction2
{
    public Java.Lang.Object? Invoke(Java.Lang.Object? p0, Java.Lang.Object? p1)
    {
        var hdrFile = "environments/studio_small_09_2k.hdr";

        var options = new HDRLoader.Options();
        HDRLoaderKt.LoadHdrIndirectLightAsync(sceneView, hdrFile, options, true, new HdrIndirectLightBuilder(), new XResult(_ =>
        {
            HDRLoaderKt.LoadHdrSkyboxAsync(sceneView, hdrFile, options, new HdrSkyboxBuilder(), new XResult(_ =>
            {
                sceneView.ModelLoader.LoadModelAsync("models/MaterialSuite.glb", new XLoadModel(hdrFile), new XResult(xmodel =>
                {
                    if (xmodel is not FilamentInstance model) return;

                    var modelNode = new ModelNode(sceneView, model);

                    modelNode.InvokeTransform(
                        new Dev.Romainguy.Kotlin.Math.Float3(0, 0, -4.0f),
                        new Dev.Romainguy.Kotlin.Math.Float3(15.0f, 0, 0),
                        modelNode.Scale,
                        false,
                        1.0f);

                    modelNode.ScaleToUnitsCube(2.0f);
                    modelNode.PlayAnimation(0, true);
                    sceneView.AddChildNode(modelNode);

                    var viewNode = new ViewNode(
                        sceneView,
                        Resource.Layout.view_node_layout,
                        false,
                        false
                    );
                    viewNode.InvokeTransform(
                        new Dev.Romainguy.Kotlin.Math.Float3(0, 0, -4.0f),
                        new Dev.Romainguy.Kotlin.Math.Float3(0f, 0, 0),
                        viewNode.Scale,
                        false,
                        1.0f
                        );
                    sceneView.AddChildNode(viewNode);

                    loadingView.Visibility = ViewStates.Invisible;
                }));
            }));
        }));
        
        return null;
    }
}