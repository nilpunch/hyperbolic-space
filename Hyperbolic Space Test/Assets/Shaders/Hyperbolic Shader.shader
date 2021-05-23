Shader "Hyperbolic/Hyperbolic Shader"
{
    Properties
    {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}

        _Color ("Color", Color) = (1,1,1,1)

        [Space] [Toggle] _EnableDiffusing ("Enable Diffusing", Int) = 0
        
        [Space] [Toggle] _EnableFog ("Enable Fog", Int) = 0

        _FogColor ("Fog Color (RGB)", Color) = (0.5, 0.5, 0.5, 1.0)
        _SphericalFogStart ("Spherical Fog Start", Float) = 0.0
        _SphericalFogEnd ("Spherical Fog End", Float) = 10.0
        _HyperbolicFogStart ("Hyperbolic Fog Start", Float) = 0.0
        _HyperbolicFogEnd ("Hyperbolic Fog End", Float) = 10.0
    }


    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }

        Pass
        {
            Tags
            {
                "LightMode" = "ForwardBase"
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight

            #include <AutoLight.cginc>
            #include <UnityCG.cginc>

            #include "HyperbolicFunctions.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct vertexOutput
            {
                float2 uv : TEXCOORD0;
                SHADOW_COORDS(3)
                float4 pos : SV_POSITION;
                float fogValue : VALUE;
                float3 worldNormal : TEXCOORD1;
                float4 diffuse : COLOR0;
            };

            float4 _GlobalOffset;
            float _GlobalCurvature;
            float _GlobalBeltramiKlein;

            sampler2D _MainTex;
            float4 _Color;
            
            int _DontAffectByGlobalOffset;
            int _EnableFog;
            int _EnableDiffusing;

            fixed4 _FogColor;
            float _SphericalFogStart;
            float _SphericalFogEnd;
            float _HyperbolicFogStart;
            float _HyperbolicFogEnd;

            vertexOutput vert(appdata v)
            {
                float4 modelPosition = modelObjectPosition();
                float4 localPlanarOffset = float4(modelPosition.x, 0, modelPosition.z, 0);

                // Model Matrix Transformation without translation
                float4 modelScaleRotation = mul(unity_ObjectToWorld, v.vertex) - localPlanarOffset;

                float4 normalizedPlanarOffset;

                // Normalization step
                if (_GlobalCurvature < 0)
                {
                    normalizedPlanarOffset = fromKleinToPoincare(localPlanarOffset, _GlobalCurvature);
                }
                else
                {
                    normalizedPlanarOffset = localPlanarOffset;
                }
                modelScaleRotation = fromKleinToPoincare(modelScaleRotation, _GlobalCurvature);

                // GyroVector Translation
                float4 vertexPosition = mobiusAdd(normalizedPlanarOffset, modelScaleRotation, _GlobalCurvature);

                // Apply Global Offset, Y separately from XZ 
                float3 globalPlanarOffset = float3(_GlobalOffset.x, 0, _GlobalOffset.z);
                if (_DontAffectByGlobalOffset == 1)
                {
                    globalPlanarOffset = 0;
                }
                float3 globalHeightOffset = float3(0, _GlobalOffset.y, 0);
                float4 globalVertexPlanarPosition = mobiusAdd(globalPlanarOffset, vertexPosition, _GlobalCurvature);
                float4 globalVertexPosition = mobiusAdd(globalHeightOffset, globalVertexPlanarPosition,
                                                        _GlobalCurvature);

                // Transform Poincare coordinates to the coordinates of another model (by factor)
                float4 transformedVertexPosition = lerp(globalVertexPosition,
                                                        fromPoincareToKlein(globalVertexPosition, _GlobalCurvature),
                                                        _GlobalBeltramiKlein);

                // View Projection Matrix Transform
                float4 projectedVertexPosition = mul(UNITY_MATRIX_VP, transformedVertexPosition);

                float fogValue = 0;
                if (_EnableFog != 0)
                {
                    if (_GlobalCurvature > 0)
                    {
                        fogValue = saturate(1.0 - (_SphericalFogEnd - length(globalVertexPosition)) /
                            (_SphericalFogEnd - _SphericalFogStart));
                    }
                    else
                    {
                        fogValue = saturate(1.0 - (_HyperbolicFogEnd - length(globalVertexPosition)) /
                            (_HyperbolicFogEnd - _HyperbolicFogStart));
                    }
                }

                vertexOutput result;
                result.pos = projectedVertexPosition;


                result.uv = v.uv;
                result.fogValue = fogValue;
                result.worldNormal = v.normal;

                if (_GlobalCurvature > 0)
                {
                    result.diffuse = max(0.2f, dot(result.worldNormal, float3(0, 1, 0)));
                }
                else
                {
                    result.diffuse = max(0.5f, dot(UnityObjectToWorldNormal(v.normal), float3(0, 1, 0)));
                }

                // Shadows WIP
                //TRANSFER_SHADOW(result)


                return result;
            }

            fixed4 frag(vertexOutput i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                // Shadows WIP
                half shadow = 1; //SHADOW_ATTENUATION(i);

                if (_EnableDiffusing != 0)
                {
                    col += float4(ShadeSH9(half4(i.worldNormal, 1)), 0);
                    col *= i.diffuse;
                }
                col.a = 1;

                return lerp(col, _FogColor, i.fogValue);
            }
            ENDCG
        }

        // Shadows WIP
        Pass
        {
            Tags
            {
                "LightMode"="ShadowCaster"
            }

            ZWrite On ZTest LEqual


            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragAdd
            #include "UnityStandardCoreForward.cginc"
            #pragma multi_compile_fwdadd_fullshadows
            #include "UnityCG.cginc"
            #include "Assets/Shaders/HyperbolicFunctions.cginc"

            float4 _GlobalOffset;
            float _GlobalCurvature;
            float _GlobalBeltramiKlein;

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata_base v)
            {
                v2f o;

                float4 modelPosition = modelObjectPosition();
                float4 localPlanarOffset = float4(modelPosition.x, 0, modelPosition.z, 0);

                float4 wPos = mul(unity_ObjectToWorld, v.vertex);

                wPos = gyrovectorTransformation(wPos, _GlobalCurvature, _GlobalOffset);

                // Transform Poincare coordinates to the coordinates of another model (by factor)
                wPos = lerp(wPos,
                            fromPoincareToKlein(wPos, _GlobalCurvature),
                            _GlobalBeltramiKlein);

                if (unity_LightShadowBias.z != 0.0)
                {
                    float3 wNormal = normalize(mul(unity_ObjectToWorld, v.normal));
                    float3 wLight = UnityWorldSpaceLightDir(wPos.xyz);
                    float shadowCos = dot(wNormal, wLight);
                    float shadowSine = sqrt(1 - shadowCos * shadowCos);
                    float normalBias = unity_LightShadowBias.z * shadowSine;

                    wPos.xyz -= wNormal * normalBias;
                }


                float4 clipPos = mul(UNITY_MATRIX_VP, wPos);

                #if !(defined(SHADOWS_CUBE) && defined(SHADOWS_CUBE_IN_DEPTH_TEX))
                #if defined(UNITY_REVERSED_Z)
                clipPos.z += max(-1, min(unity_LightShadowBias.x / clipPos.w, 0));
                #else
                clipPos.z += saturate(unity_LightShadowBias.x/clipPos.w);
                #endif
                #endif

                #if defined(UNITY_REVERSED_Z)
                float clamped = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
                #else
                float clamped = max(clipPos.z, clipPos.w*UNITY_NEAR_CLIP_VALUE);
                #endif
                clipPos.z = lerp(clipPos.z, clamped, unity_LightShadowBias.y);
                o.pos = clipPos;

                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }
    }
}