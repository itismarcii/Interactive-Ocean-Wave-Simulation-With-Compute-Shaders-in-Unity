Shader "WaveSimulationMaterialShaderV2"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            Tags{ "LightMode" = "ForwardBase" }
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "UnityCG.cginc"
            
            StructuredBuffer<float3> vertices;
            int mesh_resolution = 32;

            // User defined variabls
            
            sampler2D _MainTex;
            half _Smoothness;
            half _Metallic;
            uniform fixed4 _Color;

            // Unity defined variables
            uniform float4 _LightColor0;
            
            struct mesh_data
            {
                uint vertex_id : SV_VertexID;
                fixed4 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                fixed4 col : COLOR;
                fixed2 uv : TEXCOORD1;
            };

            float3 CalculateNormal(const uint index)
            {
                float3 position = vertices[index];
            
                // Calculate adjacent vertex indices
                int left_index = index - 1;
                int right_index = index + 1;
                int top_index = index - mesh_resolution;
                int bottom_index = index + mesh_resolution;
            
                // Check for edge cases
                if (left_index < 0 || left_index % mesh_resolution == mesh_resolution - 1) left_index = index;
                if (right_index >= mesh_resolution * mesh_resolution || right_index % mesh_resolution == 0) right_index = index;
                if (top_index < 0) top_index = index;
                if (bottom_index >= mesh_resolution * mesh_resolution) bottom_index = index;
            
                // Calculate adjacent vertex positions
                const float3 left_pos = vertices[left_index];
                const float3 right_pos = vertices[right_index];
                const float3 top_pos = vertices[top_index];
                const float3 bottom_pos = vertices[bottom_index];
            
                // Calculate central difference vectors
                float3 dx = right_pos - left_pos;
                float3 dy = bottom_pos - top_pos;
            
                // Calculate normal using cross product of central difference vectors
                return normalize(cross(dy, dx));
            }
           
            v2f vert (const mesh_data data)
            {
                v2f o;
                o.pos = mul(unity_MatrixMVP, float4(vertices[data.vertex_id], 1.0));
                o.uv = o.pos.xz;
                
                float3 normal = CalculateNormal(data.vertex_id);
                
                const float3 normal_direction = normalize(mul(float4(normal, 0.0), unity_WorldToObject).xyz);
                
                const float3 light_direction = normalize(_WorldSpaceLightPos0.xyz);
                const float atten = 1.0;
                
                const float3 diffuse_reflection = atten * _LightColor0.xyz * max(0.0, dot(normal_direction, light_direction));
                const float3 light_final = diffuse_reflection + UNITY_LIGHTMODEL_AMBIENT.xyz;
                
                o.col = fixed4(light_final * _Color.xyz, 1.0);
                // o.col = float4(normal, 1);
                return o;
            }
            
           
            fixed4 frag (const v2f i) : COLOR
            {
                return i.col;
            }
            ENDCG
        }
    }
}