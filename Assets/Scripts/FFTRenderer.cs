using System;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FFTRenderer : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    private float[] FFT;

    [SerializeField]
    [HideInInspector]
    private Vector3[] FFTPositions;

    public FFTSize FFTSize = FFTSize._1024;

    public FFTWindow FFTWindow = FFTWindow.BlackmanHarris;

    [Space]
    public LineRenderer Renderer;

    [Space]
    public bool Logarithmic;

    public float Thickness = 0.02f;

    [Range(-1.0f, +1.0f)]
    public float VerticalPosition;

    [Range(1.0f, 4.0f)]
    public float VerticalScale = 1.0f;

    private void Awake()
    {
        if (Renderer == null)
        {
            Renderer = GetComponent<LineRenderer>();
        }
    }

    private void Start()
    {
        if (Renderer == null)
        {
            Debug.LogError("Target line renderer has not been assigned, disabling this FFT renderer.", this);
            enabled = false;
            return;
        }

        if (Renderer.useWorldSpace)
        {
            Debug.LogWarning("Target line renderer uses world space, the FFT will render in a fixed position.", this);
        }
    }

    private void Update()
    {
        UpdateArray();

        AudioListener.GetSpectrumData(FFT, 0, FFTWindow);

        UpdateGraph(FFT);
    }

    private void UpdateArray()
    {
        var size = (int)FFTSize;

        if (FFT != null && FFT.Length == size)
            return;

        FFT = new float[size];

        FFTPositions = new Vector3[size];

        FFTPositions[size - 1] = Vector3.right;
    }

    private void UpdateGraph(float[] fft)
    {
        var length = fft.Length;

        Renderer.positionCount = length;

        Renderer.widthMultiplier = Thickness;

        // scale either mode so both are friendly to use

        Span<float> logs = stackalloc float[length];

        const float logAbsMin = 0.0001f;

        var logMin = math.log(logAbsMin);
        var logMax = math.log(1.0f);

        var logRange = logMax - logMin;

        for (var i = 0; i < length; i++)
        {
            logs[i] = math.clamp(math.log(fft[i] + logAbsMin), logMin, logMax);
        }

        var yPosition = -VerticalPosition;
        var yScale    = 1.0f / VerticalScale;

        if (Logarithmic)
        {
            var sr = AudioSettings.GetConfiguration().sampleRate;
            var nf = sr / 2.0f;

            for (var x = 0; x < length - 1; x++)
            {
                var y = logs[x];

                y = (y - logMin) / logRange;

                y -= yPosition;

                y /= yScale;

                y = math.clamp(y, 0, 1);

                var xf = BinToLogX((int)FFTSize, x, nf);

                FFTPositions[x] = new Vector3(xf, y, 0.0f);
            }
        }
        else
        {
            var scale = new Vector2(1.0f / length, 1.0f);

            for (var x = 0; x < length - 1; x++)
            {
                var y = FFTUtility.DbToLinear((FFTUtility.LinearToDb(fft[x]) - logMin) / logRange);

                y -= yPosition;

                y /= yScale;

                y = math.clamp(y, 0, 1);

                FFTPositions[x] = new Vector3(x, y, 0.0f) * scale;
            }
        }

        Renderer.SetPositions(FFTPositions);
    }

    private static float BinToLogX(int fftSize, int binIndex, float nyquistFrequency)
    {
        var bin2Freq = FFTUtility.BinToFrequency(fftSize, nyquistFrequency, binIndex);

        var logMax = math.log10(nyquistFrequency);

        var logFreq = math.log10(math.max(1.0f, bin2Freq));

        var x = logFreq / logMax;

        return x;
    }
}