Shader "WaveSimulationMaterialShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Main Texture", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "UnityCG.cginc"

           
            StructuredBuffer<float3> vertices;
            StructuredBuffer<float2> uvs;

            sampler2D _MainTex;
            half _Smoothness;
            half _Metallic;
            fixed4 _Color;

            struct mesh_data
            {
                uint vertex_id : SV_VertexID;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed3 normal : TEXCOORD0;
                fixed2 uv : TEXCOORD1;
            };
           
            v2f vert (const mesh_data data)
            {
                v2f o;
                o.pos = mul(unity_MatrixMVP, float4(vertices[data.vertex_id], 1.0));
                o.uv = uvs[data.vertex_id];
                
                const float3 vector0 = o.pos;
                const float3 vector1 = data.vertex_id > 0 ? mul(unity_MatrixMVP, float4(vertices[data.vertex_id - 1], 1.0)) :
                    mul(unity_MatrixMVP, float4(vertices[data.vertex_id + 1], 1.0));
                o.normal = normalize(cross(vector0, vector1));
                
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }
            
           
            fixed4 frag (const v2f i) : SV_Target
            {
                const fixed4 tex_color = tex2D(_MainTex, i.uv);
                const fixed4 blend_color = _Color;
                const fixed3 base_color = tex_color.rgb * blend_color.rgb;
                const fixed metallic = _Metallic;
                const fixed smoothness = _Smoothness;
        
                fixed4 result;
                result.rgb = base_color;
        
                // Metallic
                result.rgb *= (1.0 - smoothness) * (1.0 - metallic) + metallic;
        
                // Smoothness
                result.a = smoothness;
        
                // Alpha
                result.a *= blend_color.a;

                return result;
            }
            ENDCG
        }
    }
}