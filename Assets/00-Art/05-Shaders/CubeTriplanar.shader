Shader "Custom/CubeTriplanar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _TileSize ("Tile Size (world units)", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert

        sampler2D _MainTex;
        float4 _Color;
        float _TileSize;

        struct Input
        {
            float3 objPos;
            float3 objNormal;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.objPos = v.vertex.xyz;
            o.objNormal = v.normal;
        }

        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 objectPos = IN.objPos;
            float3 objectNormal = normalize(IN.objNormal);

            float3 blendWeights = abs(objectNormal);
            blendWeights = pow(blendWeights, 4);
            blendWeights /= max(blendWeights.x + blendWeights.y + blendWeights.z, 0.0001);

            float3 alignedPos = objectPos + 0.5;
            float2 uvX = float2(alignedPos.z, alignedPos.y) / _TileSize;
            float2 uvY = float2(alignedPos.x, alignedPos.z) / _TileSize;
            float2 uvZ = float2(alignedPos.x, alignedPos.y) / _TileSize;

            float4 colX = tex2D(_MainTex, uvX);
            float4 colY = tex2D(_MainTex, uvY);
            float4 colZ = tex2D(_MainTex, uvZ);

            float4 col = (colX * blendWeights.x + colY * blendWeights.y + colZ * blendWeights.z) * _Color;
            o.Albedo = col.rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
