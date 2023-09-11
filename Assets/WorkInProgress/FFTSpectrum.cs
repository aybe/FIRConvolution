using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

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

        [SerializeField] // TODO hide
        private Mesh FFTMesh;

        [SerializeField]
        private Material FFTMaterial;

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

            Graphics.DrawMesh(FFTMesh, Matrix4x4.identity, FFTMaterial, 0);
        }

        private void OnEnable()
        {
            UpdateFFTVars();
        }

        private void OnDisable()
        {
            FFTArray = null;

            FFTArrayNative.Dispose();

            Destroy(FFTMesh);

            FFTMesh = null;

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

            var length = FFTArray.Length;

            // scale FFT using logarithm so that it looks much better

            Span<float> logs = stackalloc float[length];

            const float logAbsMin = 0.0001f; // avoid infinite values

            var logMin = math.log(logAbsMin);
            var logMax = math.log(1.0f);

            var logRange = logMax - logMin;

            for (var i = 0; i < length; i++)
            {
                logs[i] = math.clamp(math.log(FFTArray[i] + logAbsMin), logMin, logMax);
            }

            for (var i = 0; i < length; i++)
            {
                var f = FFTArray[i];
                var g = FFTUtility.LinearToDb(f);
                var h = (g - logMin) / logRange;
                var y = FFTUtility.DbToLinear(h);
                FFTArray[i] = y;
            }

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

            var vertexCount = (size + 1) * (FFTHistory + 1);

            if (FFTMesh == null || FFTMesh.vertexCount != vertexCount)
            {
                FFTMaterial.mainTexture = FFTTexture;

                var vertices = new Vector3[vertexCount];
                var uv       = new Vector2[vertices.Length];

                for (int y = 0, i = 0; y <= FFTHistory; y++)
                {
                    for (var x = 0; x <= size; x++, i++)
                    {
                        var x1 = (float)x / size;
                        var y1 = (float)y / FFTHistory;
                        vertices[i] = new Vector3(x1, 0, y1) - new Vector3(0.5f, 0.0f, 0.5f);
                        uv[i]       = new Vector2(x1, y1);
                    }
                }

                var triangles = new int[size * FFTHistory * 6];

                for (int ti = 0, vi = 0, y = 0; y < FFTHistory; y++, vi++)
                {
                    for (var x = 0; x < size; x++, ti += 6, vi++)
                    {
                        triangles[ti]     = vi;
                        triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                        triangles[ti + 4] = triangles[ti + 1] = vi + size + 1;
                        triangles[ti + 5] = vi + size + 2;
                    }
                }

                Destroy(FFTMesh);

                FFTMesh = new Mesh
                {
                    name        = "FFT Mesh",
                    indexFormat = IndexFormat.UInt32,
                    vertices    = vertices,
                    uv          = uv,
                    triangles   = triangles
                };

                FFTMesh.RecalculateNormals();
            }
        }
    }
}