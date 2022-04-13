Shader "Custom/Skybox/Fantasy-Skybox"
{
    Properties
    {
        [Header(General)]
        _Seed("Seed", float) = 68.89
        _HorizonLevel("Horizon Level", Range(-1,1)) = 0

        [Header(Background settings)]
        [Space]
        _BackgroundColorOffset("Background Color Offset", Range(-1,1)) = 0
        _BackgroundColorBottom("Background Color Bottom", Color) = (0.0, 0.0, 0.0, 1.0)
        _BackgroundColorTop("Background Color Top", Color) = (0.0, 0.0, 0.0, 1.0)

        [Header(Stars settings)]
        [Space]
        _DensityX("Density X", Range(10,200)) = 100.0
        _DensityY("Density Y", Range(10,200)) = 30.0
        _StarSize("Star Size", Range(10,200)) = 100.0
        _StarsColor("Stars Color", Color) = (1.0, 1.0, 1.0, 1.0)
        
        [Header(Moon settings)]
        _MoonSize("Moon Size", Range(0,1)) = 0.2
        _MoonColor("Moon Color", Color) = (1.0, 1.0, 1.0, 1.0)

        [Header(Moon mask settings)]
        _MoonMaskSize("Moon mask Size", Range(0,1)) = 0.2
        _MoonMaskOffset("Moon mask offset", vector) = (0.0, 0.0, 0.0, 1.0)
        
        [Header(Clouds)]
        _CloudsNoise ("Clouds Noise texture", 2D) = "white" {}
        _CloudSpeed("Could Speed", Range(0,50)) = 1
        _CloudLevel("Could Level", Range(-1,1)) = 1

        [Header(Fog)]
        _FogColor("Fog Color Fog", Color) = (1.0, 1.0, 1.0, 1.0)
        _FogLevel("Fog Level", Range(-1,1)) = 1

    }
    SubShader
    {
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
    
        
        CBUFFER_START(UnityPerMaterial)
             half _Seed;
             half _DensityX;
             half _DensityY;
             half _StarSize;
             half _MoonSize;
             half _BackgroundColorOffset;
             half _MoonMaskSize;
             half4 _BackgroundColorBottom;
             half4 _BackgroundColorTop;
             half4 _MoonMaskOffset;
             half4 _StarsColor;
             half4 _MoonColor;
             half4 _FogColor;
             half _HorizonLevel;
             half _FogLevel;
             half _CloudSpeed;
             half _CloudLevel;
             half4 _CloudsNoise_ST;
        CBUFFER_END
        
        TEXTURE2D(_CloudsNoise);
        SAMPLER(sampler_CloudsNoise);
        
        struct appdata
        {
            half4 vertex : POSITION;
        };

        struct v2f
        {
            half4 clipPosition : SV_POSITION;
            half3 worldPosition : TEXCOORD0;
        };
        
        ENDHLSL

        Pass
        {
            Cull Back
                   
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #define TAU 6.28318530718
                        
            half2 DirToRectilinear(half3 direction)
            {
                half3 worldPosNorm = normalize(direction);
                return half2(atan2(worldPosNorm.x, worldPosNorm.z) / TAU, 4*asin(worldPosNorm.y)/ TAU);
                //return half2( atan2(direction.z,direction.x) / TAU + 0.5 , direction.y * 0.5 + 0.5);
            }
            
            half2 N22(half2 p)
            {
                half3 a = frac(p.xyx*half3(123.34,234.52,345.65));
                a += dot(a,a+34.32);
                return frac(half2(a.x*a.y,a.y*a.z));
            }
            
            half VornoiNoise(half2 uv)
            {
                half minDist = 10000.;
                
                half2 gv = frac(uv) - 0.5;
                half2 id = floor(uv);
                half2 cellId = half2(0,0);
                
                for(half y = -1.; y <= 1.; y++)
                {
                    for(half x = -1.; x <= 1.; x++)
                    {
                        half2 offset = half2(x,y);
                        half2 n = N22(id + offset + _Seed);
                        half2 p = gv-(offset + n);
                        
                        half d = abs(p.x) + abs(p.y);
                        
                        if(d<minDist)
                        {
                            minDist = d;
                            cellId = id + offset;
                        }
                    }
                }
                
                return minDist;
            }
            
            half Stars(half2 uv)
            {
                uv *= half2(_DensityX, _DensityY);

                half vornoi = 1.0 - VornoiNoise(uv);
                half clampedVornoi = clamp(vornoi, 0, 1);
                return pow(clampedVornoi, _StarSize);
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                o.clipPosition = TransformObjectToHClip(v.vertex.xyz);
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex.xyz).xyz;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {   
                half2 uv = DirToRectilinear(i.worldPosition);
                                
                half starsMask = Stars(uv);
                half backgroundGradient = uv.y - _HorizonLevel - _BackgroundColorOffset;
                half3 backgroundColor = lerp(_BackgroundColorBottom.xyz, _BackgroundColorTop.xyz, backgroundGradient);
                
                //Mixing background with stars
                half3 color = lerp(backgroundColor, _StarsColor.xyz, starsMask);     
                
                half3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPosition);
                
                Light light = GetMainLight();
                half sunEpsilon = 0.002;
                                
                half3 lightDir = normalize(-light.direction);
                half sunColorDot = acos(dot(lightDir,viewDir));
                half sunColorMask = smoothstep(sunColorDot - sunEpsilon, sunColorDot + sunEpsilon, _MoonSize);
                
                half3 maskLightDir = normalize(-light.direction + _MoonMaskOffset.xyz);
                half sunMaskDot = acos(dot(maskLightDir,viewDir));
                
                sunColorMask -= smoothstep(sunMaskDot - sunEpsilon, sunMaskDot + sunEpsilon, _MoonMaskSize);
                
                sunColorMask = clamp(sunColorMask, 0, 1);
                
                //Mixing moon with the stars and background
                color = lerp(color, _MoonColor.xyz, sunColorMask);  
                
                half cloudEpsilon = 0.005;
                       
                half cloudGradient = smoothstep(_HorizonLevel, _CloudLevel, uv.y);
                half2 cloudUv = half2(uv.x, uv.y - _Time.x * _CloudSpeed) * _CloudsNoise_ST.xy + _CloudsNoise_ST.zw;
                half cloudNoise = SAMPLE_TEXTURE2D_LOD(_CloudsNoise, sampler_CloudsNoise, cloudUv,0).r;  
                half cloudMask = smoothstep(cloudGradient - cloudEpsilon, cloudGradient + cloudEpsilon, cloudNoise);
                
                //Mixing cloudn with moon, stars, and background
                color = lerp(color, _MoonColor.xyz, cloudMask);
                
                half fogGradient = smoothstep(_HorizonLevel, _FogLevel, uv.y);
                
                color = lerp(_FogColor.xyz, color, fogGradient);
                
                return half4(color,1.);
            }
                   
            ENDHLSL
        }
    }
}
