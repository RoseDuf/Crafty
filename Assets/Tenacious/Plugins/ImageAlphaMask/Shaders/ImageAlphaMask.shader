
Shader "Tenacious/ImageAlphaMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0

		_MaskTex("Mask Texture", 2D) = "white" {}
		_MaskValue("Mask Value", Range(0,1)) = 0.5
		_MaskColor("Mask Color", Color) = (0,0,0,1)
		[Toggle(INVERT_MASK)] _INVERT_MASK("Mask Invert", Float) = 0
		[Toggle(MASK_IGNORE_IMAGE_ALPHA)] _Mask_Ignore_Image_Alpha("Mask Ignore Image Alpha", Float) = 0
    }
    SubShader
    {
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off // No culling
		Lighting Off
		ZWrite Off // No depth
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			#pragma shader_feature INVERT_MASK

            struct appdata
            {
                float4 vertex : POSITION;
				float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
				float4 vertex        : SV_POSITION;
				fixed4 color         : COLOR;
				float2 uv            : TEXCOORD0;
				float4 world_position : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
			fixed4 _Color;
			fixed4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;

			float4 _MainTex_TexelSize;

			sampler2D _MaskTex;
			float _MaskValue;
			float4 _MaskColor;
			float _Mask_Ignore_Image_Alpha;

			// vertex shader
            v2f vert (appdata data)
            {
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(data);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.world_position = data.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.world_position);

				OUT.uv = TRANSFORM_TEX(data.uv, _MainTex);

				OUT.color = data.color * _Color;

				// Vertical Texture coordinate conventions differ between two types of platforms: Direct3D-like and OpenGL-like.
				// we need to manually flip the screen Texture upside down in your Vertex Shader so that it matches the OpenGL-like coordinate standard.
				// see (https://docs.unity3d.com/Manual/SL-PlatformDifferences.html) for more info
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					OUT.uv.y = 1 - OUT.uv.y;
				#endif

				return OUT;
            }

			// fragment shader
            fixed4 frag (v2f IN) : SV_Target
            {
				half4 color = (tex2D(_MainTex, IN.uv) + _TextureSampleAdd) * IN.color;

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(IN.world_position.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip(color.a - 0.001);
				#endif

				float4 mask = tex2D(_MaskTex, IN.uv);

				// Scale mask's alpha down to (0-254) range. This is necessary so that if _MaskValue = 1 then fully opaque pixels will be masked as well.
				float alpha = mask.a * (1 - 1 / 255.0);

				// If the mask value is less than the alpha value then : weight = 1 and the color lerp below does nothing; else weight = 0 and mask is applied
				// step(x, y) is equivalent to (x <= y) but step is more performent
				float weight = step(_MaskValue, alpha);
				#if INVERT_MASK
				weight = 1 - weight;
				#endif

				// Blend in mask color depending on the weight and apply a blend between mask and mask color's alpha
				color.rgb = lerp(color.rgb, lerp(_MaskColor.rgb, color.rgb, weight), _MaskColor.a);

				// use mask's alpha if _Mask_Ignore_Image_Alpha = 1 and we applied the mask (weight = 0)
				if (_Mask_Ignore_Image_Alpha == 1 && weight == 0)
					color.a = _MaskColor.a;

				return color;
            }
            ENDCG
        }
    }
}
