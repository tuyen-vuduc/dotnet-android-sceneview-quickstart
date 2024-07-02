using Android.Views;
using AndroidX.Lifecycle;
using Com.Google.Android.Filament;
using Com.Google.Android.Filament.Gltfio;
using Com.Google.Android.Filament.Utils;
using IO.Github.Sceneview;
using IO.Github.Sceneview.Loaders;
using IO.Github.Sceneview.Model;
using IO.Github.Sceneview.Node;
using Kotlin.Coroutines;
using Kotlin.Jvm.Functions;
using Xamarin.KotlinX.Coroutines;
using View = Android.Views.View;

namespace ModelViewerQs;

public partial class MainFragment : AndroidX.Fragment.App.Fragment
{
    SceneView sceneView;
    View loadingView;
    private LifecycleCoroutineScope lifeCycleScope;

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        return inflater.Inflate(Resource.Layout.fragment_main, container, false);
    }

    public override void OnViewCreated(View view, Bundle? savedInstanceState)
    {
        base.OnViewCreated(view, savedInstanceState);

        sceneView = view.FindViewById<SceneView>(Resource.Id.sceneView);
        sceneView.Lifecycle = Lifecycle;

        loadingView = view.FindViewById(Resource.Id.loadingView);

        lifeCycleScope = AndroidX.Lifecycle.LifecycleOwnerKt.GetLifecycleScope(this);
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


class XContinuation : Java.Lang.Object, IContinuation
{
    private readonly Action<Java.Lang.Object> action;

    public ICoroutineContext Context { get; private set; }

    public XContinuation(ICoroutineContext context, Action<Java.Lang.Object> action)
    {
        this.action = action;
        this.Context = context;
    }

    public void ResumeWith(Java.Lang.Object result)
    {
        action?.Invoke(result);
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

        sceneView.EnvironmentLoader.LoadHDREnvironment(hdrFile, true, options, true, new XContinuation(
            this.lifeCycleScope.CoroutineContext,
            obj =>
        {
            var env = obj as IO.Github.Sceneview.Environment.Environment;
            sceneView.IndirectLight = env?.IndirectLight;
            sceneView.Skybox = env?.Skybox;
        }));
        sceneView.CameraNode.Position = new Dev.Romainguy.Kotlin.Math.Float3(0, 0, 4);

        var modelFile = "models/MaterialSuite.glb";
        var modelInstance = sceneView.ModelLoader.CreateModelInstance(modelFile, new XResult(obj =>
        {

        }));

        var modelNode = new ModelNode(
                modelInstance,
                true,
                new Java.Lang.Float(2.0f),
                null
            );
        modelNode.Scale = new Dev.Romainguy.Kotlin.Math.Float3(3f, 0, 0);
        sceneView.AddChildNode(modelNode);
        loadingView.Visibility = ViewStates.Gone;
        return null;
    }
}