using Android.Views;
using Com.Google.Android.Filament.Gltfio;
using Google.AR.Core;
using IO.Github.Sceneview.AR;
using IO.Github.Sceneview.Node;
using Kotlin.Jvm.Functions;
using Android.OS;
using Dev.Romainguy.Kotlin.Math;
using Kotlin;
using IO.Github.Sceneview.AR.Arcore;
using System.Linq;
using System;
using Android.Content;
using IO.Github.Sceneview.Utils;
using IO.Github.Sceneview;

namespace ARPointCloudQs;

public partial class MainFragment : AndroidX.Fragment.App.Fragment
{
    ARSceneView sceneView;
    TextView scoreText;

    SeekBar confidenceSeekbar;
    TextView confidenceText;

    SeekBar maxPointsSeekbar;
    TextView maxPointsText;

    View loadingView;

    private List<FilamentInstance> pointCloudModelInstances = new();
    private readonly List<PointCloudNode> pointCloudNodes = new();

    private long lastPointCloudTimestamp;
    private Frame lastPointCloudFrame;

    public bool IsLoading
    {
        get => loadingView.Visibility == ViewStates.Visible;
        set => loadingView.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
    }

    float minConfidence = 0.1f;
    private float MinConfidence
    {
        get => minConfidence;
        set
        {
            minConfidence = value;
            confidenceText.Text = this.GetString(Resource.String.min_confidence, value);
            pointCloudNodes.Where(x => x.Confidence < value)
                .ToList()
                .ForEach(x => sceneView.RemoveChildNode(x));
        }
    }
    int maxPoints = 500;
    private int MaxPoints
    {
        get => maxPoints;
        set
        {
            maxPoints = value;
            maxPointsText.Text = this.GetString(Resource.String.max_points, value);
            if (pointCloudNodes.Count > value)
            {
                pointCloudNodes.Take(value)
                    .ToList().ForEach(removePointCloudNode);
            }
        }
    }

    public override View? OnCreateView(LayoutInflater inflater, ViewGroup? container, Bundle? savedInstanceState)
    {
        return inflater.Inflate(Resource.Layout.fragment_main, container, false);
    }

    public override void OnViewCreated(View view, Bundle? savedInstanceState)
    {
        base.OnViewCreated(view, savedInstanceState);

        sceneView = view.FindViewById<ARSceneView>(Resource.Id.sceneView);
        sceneView.PlaneRenderer.Enabled = false;
        sceneView.SessionConfiguration = new XSessionConfiguration();
        sceneView.OnSessionUpdated = this;

        scoreText = view.FindViewById<TextView>(Resource.Id.scoreText);

        var topMargin = (scoreText.LayoutParameters as ViewGroup.MarginLayoutParams).TopMargin;
        scoreText.doOnApplyWindowInsets(systemBarsInsets =>
        {
            (scoreText.LayoutParameters as ViewGroup.MarginLayoutParams).TopMargin = systemBarsInsets.Top + topMargin;
        });
        scoreText.Text = GetString(Resource.String.score, 0.0f, 0);

        confidenceText = view.FindViewById<TextView>(Resource.Id.confidenceText);
        confidenceText.Text = GetString(Resource.String.min_confidence, minConfidence);

        confidenceSeekbar = view.FindViewById<SeekBar>(Resource.Id.confidenceSeekbar);
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            confidenceSeekbar.Min = 0;
        }
        confidenceSeekbar.Max = 40;
        confidenceSeekbar.Progress = (int)(minConfidence * 100);
        confidenceSeekbar.ProgressChanged += (s, e) =>
        {
            if (e.FromUser)
            {
                minConfidence = confidenceSeekbar.Progress / 100.0f;
            }
        };

        var maxPointsLayout = view.FindViewById<LinearLayout>(Resource.Id.maxPointsLayout);
        var bottomMargin = (maxPointsLayout.LayoutParameters as ViewGroup.MarginLayoutParams).BottomMargin;
        maxPointsLayout.doOnApplyWindowInsets(systemBarsInsets =>
        {
            (maxPointsLayout.LayoutParameters as ViewGroup.MarginLayoutParams).BottomMargin =
                systemBarsInsets.Bottom + bottomMargin;
        });

        maxPointsText = view.FindViewById<TextView>(Resource.Id.maxPointsText);
        maxPointsText.Text = GetString(Resource.String.max_points, maxPoints);

        maxPointsSeekbar = view.FindViewById<SeekBar>(Resource.Id.maxPointsSeekbar);
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            maxPointsSeekbar.Min = 100;
        }
        maxPointsSeekbar.Max = 1500;
        maxPointsSeekbar.Progress = maxPoints;
        maxPointsSeekbar.ProgressChanged += (s, e) =>
        {
            if (e.FromUser)
            {
                maxPoints = maxPointsSeekbar.Progress;
            }
        };

        loadingView = view.FindViewById(Resource.Id.loadingView);
    }

    private FilamentInstance getPointCloudModelInstance()
    {
        if (pointCloudModelInstances.Count == 0)
        {
            var assetsFileLocation = "models/point_cloud.glb";
            pointCloudModelInstances = sceneView.ModelLoader.CreateInstancedModel(
                assetsFileLocation,
                maxPoints,
                new CreateInstancedModelFunction1(Context, assetsFileLocation)
            ).ToList();
        }

        var lastOrDefault = pointCloudModelInstances.LastOrDefault();

        if (lastOrDefault != null)
        {
            pointCloudModelInstances.Remove(lastOrDefault);
        }

        return lastOrDefault;
    }

    public override void OnPause()
    {
        base.OnPause();
        pointCloudNodes.ToList().ForEach(removePointCloudNode);
    }

    void addPointCloudNode(int id, Float3 position, float confidence)
    {
        if (pointCloudNodes.Count < maxPoints)
        {
            var modelInstance = getPointCloudModelInstance();
            if (modelInstance != null)
            {
                var pointCloudNode = new PointCloudNode(modelInstance, id, confidence);
                pointCloudNode.Position = position;
                pointCloudNodes.Add(pointCloudNode);
                sceneView.AddChildNode(pointCloudNode);
            }
        }
        else
        {
            var pointCloudNode = pointCloudNodes.First();
            pointCloudNode.Id = id;
            pointCloudNode.WorldPosition = position;
            pointCloudNode.Confidence = confidence;

            pointCloudNodes.Remove(pointCloudNode);
            pointCloudNodes.Add(pointCloudNode);
        }
    }

    void removePointCloudNode(PointCloudNode pointCloudNode)
    {
        pointCloudNodes.Remove(pointCloudNode);
        sceneView.RemoveChildNode(pointCloudNode);
        pointCloudNode.Destroy();
    }

}

partial class CreateInstancedModelFunction1 : Java.Lang.Object, IFunction1
{
    private readonly Context context;
    private readonly string assetsFileLocation;

    public CreateInstancedModelFunction1(Context context, string assetsFileLocation)
    {
        this.context = context;
        this.assetsFileLocation = assetsFileLocation;
    }

    public Java.Lang.Object? Invoke(Java.Lang.Object? p0)
    {
        var fileLocation = $"{assetsFileLocation.Trim('/')}/{p0}";
        return FileKt.ReadBuffer(context.Assets, fileLocation);
    }
}

partial class MainFragment : IFunction2
{
    const int kMaxPointCloudPerSecond = 10;

    public Java.Lang.Object? Invoke(Java.Lang.Object? p0, Java.Lang.Object? p1)
    {
        if (p1 is not Frame frame) return null;

        if (FrameKt.Fps(frame, lastPointCloudFrame) < kMaxPointCloudPerSecond)
        {
            var pointCloud = frame.AcquirePointCloud();
            if (frame.AcquirePointCloud().Timestamp != lastPointCloudTimestamp)
            {
                if (pointCloud.Ids == null) return null;

                lastPointCloudFrame = frame; ;
                lastPointCloudTimestamp = pointCloud.Timestamp;

                var idsBuffer = pointCloud.Ids;
                var pointsSize = idsBuffer.Limit();
                var ids = new List<int>();
                var pointsBuffer = pointCloud.Points;
                for (var index = 0; index < pointsSize; index++)
                {
                    var id = idsBuffer.Get(index);
                    ids.Add(id);

                    var firstOrDefault = pointCloudNodes.FirstOrDefault(x => x.Id == id);

                    if (firstOrDefault == null)
                    {
                        var pointIndex = index * 4;
                        var position = new Float3(
                            pointsBuffer.Get(pointIndex),
                            pointsBuffer.Get(pointIndex + 1),
                            pointsBuffer.Get(pointIndex + 2)
                        );
                        var confidence = pointsBuffer.Get(pointIndex + 3);
                        if (confidence > minConfidence)
                        {
                            addPointCloudNode(id, position, confidence);
                        }
                    }
                }
                var score = pointCloudNodes.Count > 0
                    ? pointCloudNodes.Sum(x => x.Confidence) / pointCloudNodes.Count
                    : 0.0;
                scoreText.Text = GetString(Resource.String.score, score, pointCloudNodes.Count);
            }
        }

        return null;
    }
}

partial class XSessionConfiguration : Java.Lang.Object, IFunction2
{
    public Java.Lang.Object? Invoke(Java.Lang.Object? p0, Java.Lang.Object? p1)
    {
        if (p1 is Config config)
        {
            config.SetLightEstimationMode(Config.LightEstimationMode.Disabled);
        }
        return null;
    }
}

partial class PointCloudNode : ModelNode
{
    public PointCloudNode(FilamentInstance modelInstance, int id, float confidence)
        : base(modelInstance, true, null, null)
    {
        Id = id;
        Confidence = confidence;
    }

    public int Id { get; set; }
    public float Confidence { get; set; }
}