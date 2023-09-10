using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;

namespace WorkInProgress
{
    public sealed class FFTSpectrum : MonoBehaviour
    {
        private const int FFTHistoryMin = 1;

        private const int FFTHistoryMax = 1024;

        [SerializeField]
        [HideInInspector]
        private float[] FFTArray;

        [SerializeField]
        private FFTSize FFTSize;

        [SerializeField]
        private FFTWindow FFTWindow;

        [SerializeField]
        [Min(0)]
        private int FFTChannel;

        [SerializeField]
        [Range(FFTHistoryMin, FFTHistoryMax)]
        private int FFTHistory = FFTHistoryMin;

        [SerializeField] // TODO hide
        private int FFTHistoryIndex;

        [SerializeField] // TODO hide
        private Texture2D FFTTexture;

        private NativeArray<float> FFTArrayNative;

        private void Reset()
        {
            FFTSize = FFTSize._512;

            FFTWindow = FFTWindow.BlackmanHarris;

            FFTChannel = 0;

            FFTHistory = Mathf.NextPowerOfTwo((FFTHistoryMax - FFTHistoryMin) / 2);
        }

        private void Update()
        {
            UpdateFFT();
        }

        private void OnEnable()
        {
            UpdateFFTVars();
        }

        private void OnDisable()
        {
            FFTArray = null;

            FFTArrayNative.Dispose();

            Destroy(FFTTexture);

            FFTTexture = null;
        }

        private void OnValidate()
        {
            SetFFTChannel(FFTChannel);
            SetFFTHistory(FFTHistory);
            SetFFTSize(FFTSize);
            SetFFTWindow(FFTWindow);
        }

        [PublicAPI]
        public void SetFFTChannel(int channel)
        {
            var configuration = AudioSettings.GetConfiguration();

            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            var channelMax = configuration.speakerMode switch
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

            if (Application.isPlaying) // don't pollute console unnecessarily
            {
                if (channel < 0 || channel >= channelMax)
                {
                    Debug.LogError($"FFT channel index is invalid : {channel}.", this);
                }
            }

            FFTChannel = Mathf.Clamp(channel, 0, channelMax - 1);
        }

        [PublicAPI]
        public void SetFFTHistory(int history)
        {
            if (history is < FFTHistoryMin or > FFTHistoryMax)
            {
                throw new ArgumentOutOfRangeException(nameof(history));
            }

            FFTHistory = history;

            UpdateFFTVars();
        }

        [PublicAPI]
        public void SetFFTSize(FFTSize size)
        {
            if (!Enum.IsDefined(typeof(FFTSize), size))
            {
                throw new InvalidEnumArgumentException(nameof(size), (int)size, typeof(FFTSize));
            }

            FFTSize = size;

            UpdateFFTVars();
        }

        [PublicAPI]
        public void SetFFTWindow(FFTWindow window)
        {
            if (!Enum.IsDefined(typeof(FFTWindow), window))
            {
                throw new InvalidEnumArgumentException(nameof(window), (int)window, typeof(FFTWindow));
            }

            FFTWindow = window;
        }

        private void UpdateFFT()
        {
            AudioListener.GetSpectrumData(FFTArray, FFTChannel, FFTWindow);

            NativeArray<float>.Copy(FFTArray, 0, FFTArrayNative, FFTArray.Length * FFTHistoryIndex, FFTArray.Length);

            // TODO https://docs.unity3d.com/ScriptReference/Texture2D.GetRawTextureData.html

            FFTTexture.LoadRawTextureData(FFTArrayNative);

            FFTTexture.Apply();

            FFTHistoryIndex = FFTHistoryIndex + 1 & FFTHistory - 1;
        }

        private void UpdateFFTVars()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            // TODO can't this be simplified somehow?

            var size = (int)FFTSize;

            if (FFTArray == null || FFTArray.Length != size)
            {
                FFTArray = new float[size];
            }

            if (FFTArrayNative.Length != size * FFTHistory)
            {
                FFTArrayNative.Dispose();

                FFTArrayNative = new NativeArray<float>(size * FFTHistory, Allocator.Persistent);
            }

            if (FFTTexture == null || FFTTexture.width != size || FFTTexture.height != FFTHistory)
            {
                Destroy(FFTTexture);

                FFTTexture = new Texture2D(size, FFTHistory, TextureFormat.RFloat, false)
                {
                    name = "FFT Texture"
                };
            }
        }
    }
}