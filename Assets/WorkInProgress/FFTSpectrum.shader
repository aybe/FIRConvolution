Shader"Custom/FFT Spectrum"
{
	Properties
	{
		[NoScaleOffset]
		_MainTex ("Texture", 2D) = "white" {}
		_Height("Height", float) = 1.0
		_Blend("Blend", float) = 0.5
		_Color0 ("Color 0", Color) = (1.0, 0.0, 0.0, 0.125)
		_Color1 ("Color 1", Color) = (1.0, 0.5, 0.0, 0.250)
		_Color2 ("Color 2", Color) = (1.0, 1.0, 0.0, 0.375)
		_Color3 ("Color 3", Color) = (0.0, 1.0, 0.0, 0.500)
		_Color4 ("Color 4", Color) = (0.0, 1.0, 1.0, 0.625)
		_Color5 ("Color 5", Color) = (0.0, 0.0, 1.0, 0.750)
		_Color6 ("Color 6", Color) = (0.5, 0.0, 1.0, 0.875)
		_Color7 ("Color 7", Color) = (1.0, 0.0, 1.0, 1.000)
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float _Height;
			float _Blend;
			float4 _Color0;
			float4 _Color1;
			float4 _Color2;
			float4 _Color3;
			float4 _Color4;
			float4 _Color5;
			float4 _Color6;
			float4 _Color7;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			float4 SampleGradient(const float time)
			{
				const float4 colors[8] =
				{
					_Color0,
					_Color1,
					_Color2,
					_Color3,
					_Color4,
					_Color5,
					_Color6,
					_Color7,
				};

				float3 color = colors[0].rgb;

				[unroll]
				for (int c = 1; c < 8; c++)
				{
					const float pos =
						saturate((time - colors[c - 1].w) / (colors[c].w - colors[c - 1].w)) *
						step(c, 8 - 1);

					color = lerp(color, colors[c].rgb, lerp(pos, step(0.01, pos), _Blend));
				}

				return float4(color, 1);
			}

			v2f vert(appdata v)
			{
				v2f o;

				const float height = tex2Dlod(_MainTex, float4(v.uv, 0, 0)).r;

				v.vertex.y += height * _Height;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = SampleGradient(height);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}