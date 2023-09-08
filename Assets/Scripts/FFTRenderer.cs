using System;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FFTRenderer : MonoBehaviour
{
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

    private float[] FFT;

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
    }

    private void UpdateGraph(float[] fft)
    {
        var length = fft.Length;

        Renderer.positionCount = length;

        Renderer.widthMultiplier = Thickness;

        // scale either mode so both are friendly to use

        Span<float> logs = stackalloc float[fft.Length];

        const float logAbsMin = 0.0001f;

        var logMinVal = math.log(logAbsMin);
        var logMaxVal = math.log(1.0f);

        var logRange = logMaxVal - logMinVal;

        for (var i = 0; i < logs.Length; i++)
        {
            var binVal = fft[i];

            var binLog = math.log(binVal + logAbsMin);

            binLog = math.clamp(binLog, logMinVal, logMaxVal);

            logs[i] = binLog;
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

                y = (y - logMinVal) / logRange;

                y -= yPosition;

                y /= yScale;

                y = math.clamp(y, 0, 1);

                var xf = BinToLogX((int)FFTSize, x, nf);

                Renderer.SetPosition(x, new Vector3(xf, y, 0.0f));
            }
        }
        else
        {
            var scale = new Vector2(1.0f / length, 1.0f);

            for (var x = 0; x < length - 1; x++)
            {
                var y = FFTUtility.DbToLinear((FFTUtility.LinearToDb(fft[x]) - logMinVal) / logRange);

                y -= yPosition;

                y /= yScale;

                y = math.clamp(y, 0, 1);

                Renderer.SetPosition(x, new Vector3(x, y, 0.0f) * scale);
            }
        }

        Renderer.SetPosition(fft.Length - 1, Vector3.right);
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