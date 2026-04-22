Shader "Custom/FloorTriplanar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TileSize ("Tile Size (world units)", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float _TileSize;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Blending weights based on world-space normal
            float3 blendWeights = abs(IN.worldNormal);
            blendWeights = pow(blendWeights, 4);
            blendWeights /= (blendWeights.x + blendWeights.y + blendWeights.z);

            // Sample texture from each axis using world position.
            // XZ offset +0.5 : floor edges always at x=-0.5 and z=-0.5.
            // Y offset +0.02 : floor top at y=0.98, aligns to tile boundaries
            //                  (0.98, -0.02, -1.02 ... -5.02) = 6 tuiles complètes.
            float2 uvX = float2(IN.worldPos.z + 0.5, IN.worldPos.y + 0.02) / _TileSize;
            float2 uvY = (IN.worldPos.xz + 0.5) / _TileSize;
            float2 uvZ = float2(IN.worldPos.x + 0.5, IN.worldPos.y + 0.02) / _TileSize;

            float4 colX = tex2D(_MainTex, uvX);
            float4 colY = tex2D(_MainTex, uvY);
            float4 colZ = tex2D(_MainTex, uvZ);

            // Blend
            o.Albedo = (colX * blendWeights.x + colY * blendWeights.y + colZ * blendWeights.z).rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
