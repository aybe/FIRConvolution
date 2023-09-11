Shader"Custom/FFT Spectrum"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Height("Height", float) = 0.1
		_Intensity("Intensity", float) = 500.0
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
			float _Intensity;

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

			v2f vert(appdata v)
			{
				v2f o;

				const float height = tex2Dlod(_MainTex, float4(v.uv, 0, 0)).r;

				v.vertex.y += height * _Height;

				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = float4(height * _Intensity, 0, 0, 1);
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