using Android.Views;
using IO.Github.Sceneview;
using IO.Github.Sceneview.Node;
using Kotlin.Jvm.Functions;
using static Android.Icu.Text.Transliterator;
using static Com.Google.Android.Filament.Utils.IBLPrefilterContext;

namespace SceneViewQs;

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
        sceneView.Lifecycle = Lifecycle;

        loadingView = view.FindViewById(Resource.Id.loadingView);

        var lifeCycleScope = AndroidX.Lifecycle.LifecycleOwnerKt.GetLifecycleScope(this);

        lifeCycleScope.LaunchWhenCreated(this);
    }
}

partial class MainFragment : IFunction2
{
    public Java.Lang.Object? Invoke(Java.Lang.Object? p0, Java.Lang.Object? p1)
    {
        var hdrFile = "environments/studio_small_09_2k.hdr";

        var light = sceneView.LoadHdrIndirectLight(hdrFile, specularFilter: true);
        light.Intensity = (30_000f);

        var skybox = sceneView.LoadHdrSkybox(hdrFile);
        skybox.Intensity(50_000f);

        var model = sceneView.ModelLoader.LoadModel("models/MaterialSuite.glb", null, null);
        var modelNode = new ModelNode(sceneView, model); 
        
        {
            transform(
                position = Position(z = -4.0f),
                rotation = Rotation(x = 15.0f)
            )
                scaleToUnitsCube(2.0f)
                // TODO: Fix centerOrigin
                //  centerOrigin(Position(x=-1.0f, y=-1.0f))
                playAnimation()
            }
        sceneView.addChildNode(modelNode)

            val viewNode = ViewNode(
                sceneView = sceneView,
                viewResourceId = R.layout.view_node_layout
            ).apply {
            transform(
                position = Position(z = -4f),
                rotation = Rotation()
            )
            }
        sceneView.addChildNode(viewNode)

            loadingView.isGone = true
    }
}