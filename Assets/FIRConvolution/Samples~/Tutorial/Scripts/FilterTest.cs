#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIRConvolution.Samples.Tutorial
{
    public unsafe class FilterTest : MonoBehaviour
    {
        [SerializeField]
        private FilterType FilterType = FilterType.ScalarFull;

        private AudioConfiguration AudioConfiguration;

        private Filter[]? FilterData;

        private FilterMethodHandler FilterPass = null!;

        private FilterProcHandler FilterProc = null!;

        private static MemoryAllocator Allocator { get; } = MemoryAllocatorUnity.Instance;

        private void OnEnable()
        {
            AudioConfiguration = AudioSettings.GetConfiguration();

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
                Filter.Free(ref filters[i], Allocator);
            }

            FilterData = null;
        }

        [SuppressMessage("ReSharper", "SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault")]
        private void FilterInit()
        {
            var sampleRate = AudioConfiguration.sampleRate;

            var channels = AudioConfiguration.speakerMode switch
            {
                AudioSpeakerMode.Mono        => 1,
                AudioSpeakerMode.Stereo      => 2,
                AudioSpeakerMode.Quad        => 4,
                AudioSpeakerMode.Surround    => 5,
                AudioSpeakerMode.Mode5point1 => 6,
                AudioSpeakerMode.Mode7point1 => 8,
                AudioSpeakerMode.Prologic    => 2,
                _                            => throw new ArgumentOutOfRangeException()
            };

            FilterType.GetHandlers(out var create, out var method);

            var lp64 = FilterUtility.LowPass(sampleRate, sampleRate / 4.0d, sampleRate / 100.0d, FilterWindow.Blackman);
            var lp32 = Array.ConvertAll(lp64, Convert.ToSingle);

            FilterData = new Filter[channels];

            for (var i = 0; i < channels; i++)
            {
                FilterData[i] = create(lp32, Allocator);
            }

            FilterPass = method;
        }

        private delegate void FilterProcHandler(float[] data, int channels);

#if UNITY_EDITOR
        [CustomEditor(typeof(FilterTest))]
        private sealed class FilterTestEditor : Editor
        {
            // some more Unity shit -> no VU meter unless class has an editor!

            public override void OnInspectorGUI()
            {
                if (EditorApplication.isPlaying)
                {
                    EditorGUILayout.HelpBox("When profiling, collapse this component to avoid VU meter overhead.", MessageType.Warning, true);
                }

                base.OnInspectorGUI();
            }
        }
#endif
    }
}