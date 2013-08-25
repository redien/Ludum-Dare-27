Shader "Custom/StarShader" {
	Properties {
		_Stripes("_Stripes", Float) = 10
		_StripeSpeed("_StripeSpeed", Vector) = (100, 100, 0, 0)
		_StripeFuzziness("_StripeFuzziness", Range(0.0001, 1)) = 0.0001
		_StripeSize("_StripeSize", Range(0.0,1.0)) = 0.5
		_StripeSizeMin("_StripeSizeMin", Range(0.0,1.0)) = 0
		_StripeSizeSpeed("_StripeSizeSpeed", Float) = 0
		_StripeColor("_StripeColor", Color) = (1,1,1,1)
		_BackgroundColor("_BackgroundColor", Color) = (0,0,0,1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma target 3.0

		struct Input {
			float2 full_uv;
		};
		
		float _Stripes;
		float4 _StripeSpeed;
		float _StripeFuzziness;
		float _StripeSize;
		float _StripeSizeMin;
		float _StripeSizeSpeed;
		float4 _StripeColor;
		float4 _BackgroundColor;
		
		void vert (inout appdata_full v, out Input o) {
			o.full_uv = v.texcoord.xy;
		}

		float4 cutoff(float4 input, float size, float fuzziness) {
			size = size * 2 - 1;
			fuzziness = clamp(fuzziness, 0.0001, 1);
			return clamp(clamp((input + size) * 0.5, 0.0, fuzziness) / fuzziness, 0.0, 1.0);
		}
		
		float sdTriPrism(float2 p, float2 h) {
		    float2 q = abs(p);
		    return max(-h.y,max(q.x*0.866025+p.y*0.5,-p.y)-h.x*0.5);
		}
		
		float4 makeStars (float2 coordinates, float2 pattern_offset, float circles, float size, float fuzziness) {
			coordinates = (pattern_offset + coordinates) * circles;
			float2 origin = floor(coordinates) + float2(0.5, 0.5);
			float2 p = coordinates - origin;
			
			float d = sdTriPrism(float2(p.x, -p.y), float2(1.0, 1.0));
			float d2 = sdTriPrism(float2(p.x, p.y), float2(1.0, 1.0));
			
			d = 1.0 - cutoff(float4(1,1,1,1) * d * 1 + 0.2, 1 - (size + 1) / 3.2, fuzziness * 0.1);
			d2 = 1.0 - cutoff(float4(1,1,1,1) * d2 * 1 + 0.2, 1 - (size + 1) / 3.2, fuzziness * 0.1);
			
			return d + d2;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			float4 pattern = makeStars(IN.full_uv, _Time.xx * _StripeSpeed.xy, _Stripes, _StripeSizeMin + (_StripeSize - _StripeSizeMin) * (cos(_Time.xx * _StripeSizeSpeed) + 1) * 0.5, _StripeFuzziness);
			o.Albedo = (_BackgroundColor * (1 - pattern) + _StripeColor * pattern).rgb;
			o.Alpha = _BackgroundColor.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
