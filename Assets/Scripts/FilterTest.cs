#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using FIRConvolution;
using UnityEngine;
using UnityEngine.Assertions;

public unsafe class FilterTest : MonoBehaviour
{
    [SerializeField]
    private FilterType FilterType = FilterType.ScalarFull; // TODO this should be public

    private Filter[]? FilterData;

    private FilterMethodHandler FilterPass = null!;

    private FilterProc FilterProc = null!;

    private static MemoryAllocator Allocator { get; } = MemoryAllocatorUnity.Instance;

    private void OnEnable()
    {
        var configuration = AudioSettings.GetConfiguration();

        if (configuration.sampleRate != 44100)
        {
            Debug.LogError("Sample rate must be 44100Hz.");
            enabled = false;
            return;
        }

        if (configuration.speakerMode != AudioSpeakerMode.Stereo)
        {
            Debug.LogError("Speaker mode must be stereo.");
            enabled = false;
            return;
        }

        FilterProc = AudioFilterSwap;
    }

    private void OnDisable()
    {
        FilterProc = AudioFilterFree;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        FilterProc(data, channels);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            FilterProc = AudioFilterSwap;
        }
    }

    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void AudioFilterFree(float[] data, int channels)
    {
        FilterFree();

        FilterProc = AudioFilterNope;
    }

    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Local")]
    private void AudioFilterNope(float[] data, int channels)
    {
        // this pattern fixes the possible exceptions when switching the algorithm

        // since it runs on another thread, filter state becomes null at some point

        // by using multiple handlers, we can fix that but also avoid some branching
    }

    private void AudioFilterSwap(float[] data, int channels)
    {
        FilterFree();
        FilterInit();

        FilterProc = AudioFilterWork;
    }

    private void AudioFilterWork(float[] data, int channels)
    {
        Assert.IsNotNull(FilterData);

        Assert.AreEqual(0, data.Length % 4);

        Assert.AreEqual(2, channels);

        var samples = data.Length / channels;

        fixed (float* temp = data)
        {
            for (var i = 0; i < channels; i++)
            {
                ref var filter = ref FilterData![i];

                FilterPass(temp, temp, samples, channels, i, ref filter);
            }
        }
    }

    private void FilterFree()
    {
        var filters = FilterData;

        if (filters == null)
        {
            return;
        }

        for (var i = 0; i < filters.Length; i++)
        {
            ref var filter = ref filters[i];

            Allocator.Free(new IntPtr(filter.H));
            Allocator.Free(new IntPtr(filter.Z));

            filter = default;
        }

        FilterData = null;
    }

    private void FilterInit()
    {
        FilterType.GetHandlers(out var create, out var method);

        var lp64 = FilterUtility.LowPass(44100.0d, 11025.0d, 441.0d, FilterWindow.Blackman);
        var lp32 = Array.ConvertAll(lp64, Convert.ToSingle);

        FilterData = new[]
        {
            create(lp32, Allocator),
            create(lp32, Allocator)
        };

        FilterPass = method;
    }
}