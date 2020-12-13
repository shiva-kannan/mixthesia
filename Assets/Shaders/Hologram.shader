Shader "Unlit/Hologram"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color)=(1.0,1.0,1.0,1.0)

		_ScanTiling("Scan Tiling",Range(0.01,200.0)) = 0.05
		_ScanSpeed("Scan Speed",Range(-2.0,2.0))=1.0


		_Alpha("Alpha",Range(0.0,1.0))=1.0


		_GlowTiling("Glow Tiling",Range(0.01,100.0))=0.05
		_GlowSpeed("Glow Speed",Range(-10,10))=1.0


		_RimColor("Rim Color",Color)=(1.0,1.0,1.0,1.0)
		_RimPower("Rim Power",Range(0.1,10))=5.0
		_Brightness("Brightness",Range(0.1,6.0))=3.0
		_Direction("Direction",Vector)=(0,1,0,0)

		
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Back

			ColorMask RGB

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag 
     




            #include "UnityCG.cginc"

			  sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color, _RimColor;
			float _ScanTiling, _ScanSpeed,_Alpha, _GlowTiling, _RimPower, _Brightness, _GlowSpeed;
			float4 _Direction;

            struct a2v
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal:NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
				float3 worldnormal:NORMAL; 
				float3 worldpos:TEXCOORD1;
				
            };

          

            v2f vert (a2v v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldnormal = UnityObjectToWorldNormal(v.normal);
				o.worldpos = mul(unity_ObjectToWorld, v.vertex);
				
                return o;
            }

			fixed4 frag(v2f i) : SV_Target
			{
			   fixed4 texColor = tex2D(_MainTex,i.uv);

			   float3 worldPos = normalize(i.worldpos);
			   float3 worldNormal = normalize(i.worldnormal);
			   float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldpos));

			   half dirVertex = (dot(i.worldpos, normalize(float4(_Direction.xyz, 1.0))) + 1) / 2;

			   
			   //reference from andybc 
			   float scan = 0.0; 
			   scan = step(frac(dirVertex * _ScanTiling + _Time.w * _ScanSpeed), 0.5) * 0.3;
			   

			   float glow = 0.0; 
		       glow = frac(dirVertex * _GlowTiling - _Time.x * _GlowSpeed);
		


			   half rim = 1.0 - saturate(dot(viewDir, worldNormal)); 
			   fixed4 rimColor = _RimColor * pow(rim, _RimPower);

			   fixed4 color = texColor * _Color + (glow * 0.35 * _Color) + rimColor;
			   color.a = texColor.a * _Alpha * (scan + rim + glow);

			   color.rgb *= _Brightness; 

			   return color;
			   

			 

			   
            }
            ENDCG
        }
    }
}
