Shader "TackleApparatus/UI/RotatingReelSurface"
{
    Properties
    {
        [PerRendererData] _MainTex ("Preserved Drum / Decoration Source", 2D) = "white" {}
        _PaperTex ("Flat Paper", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Phase ("Rotation Radians", Float) = 0
        _Radius ("Cylinder Radius", Float) = 172
        _CylinderSize ("Cylinder Size", Vector) = (320,365,0,0)
        _PlaqueUV ("Plaque Source XY / WH", Vector) = (.119,.173,.767,.161)
        _DividerUV ("Divider Source XY / WH", Vector) = (.083,.796,.835,.020)
        _Plaque ("Plaque Half Width / Half Height / Center Y", Vector) = (123,26,-95,0)
        _Dividers ("Divider Half Width / Half Height / Upper Y / Lower Y", Vector) = (134,2.8,112,-48)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; float4 local:TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
            sampler2D _MainTex, _PaperTex;
            fixed4 _Color;
            float _Phase, _Radius;
            float4 _CylinderSize, _PlaqueUV, _DividerUV, _Plaque, _Dividers, _ClipRect;
            v2f vert(appdata v)
            {
                v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.local=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color*_Color; return o;
            }
            fixed4 frag(v2f i):SV_Target
            {
                const float pi=3.14159265359;
                fixed4 rim=tex2D(_MainTex,i.uv);
                float2 p=(i.uv-.5)*_CylinderSize.xy;
                float height=clamp(p.y/_Radius,-1,1);
                float theta=asin(height);
                float surfaceAngle=theta-_Phase;
                // Inverse of the native sprite/TMP cylinder projection. Four equally spaced faces share this phase.
                float faceAngle=frac((surfaceAngle+pi*.25)/(pi*.5))*(pi*.5)-pi*.25;
                float faceY=sin(faceAngle)*_Radius;
                // Mirror the flat tile into a continuous periodic strip; generated texture edges never form a seam.
                float paperV=abs(frac(surfaceAngle/(2*pi))*2-1);
                fixed3 paper=tex2D(_PaperTex,float2(i.uv.x,paperV)).rgb;
                float2 plaqueUV=float2(p.x/(2*_Plaque.x)+.5,(faceY-_Plaque.z)/(2*_Plaque.y)+.5);
                float plaqueMask=step(abs(p.x),_Plaque.x)*step(abs(faceY-_Plaque.z),_Plaque.y);
                fixed3 plaque=tex2D(_MainTex,_PlaqueUV.xy+saturate(plaqueUV)*_PlaqueUV.zw).rgb;
                paper=lerp(paper,plaque,plaqueMask);
                float upper=abs(faceY-_Dividers.z), lower=abs(faceY-_Dividers.w);
                float lineCenter=upper<lower?_Dividers.z:_Dividers.w;
                float lineMask=step(abs(p.x),_Dividers.x)*step(abs(faceY-lineCenter),_Dividers.y);
                float2 lineUV=float2(p.x/(2*_Dividers.x)+.5,(faceY-lineCenter)/(2*_Dividers.y)+.5);
                fixed3 dividerColor=tex2D(_MainTex,_DividerUV.xy+saturate(lineUV)*_DividerUV.zw).rgb;
                paper=lerp(paper,dividerColor,lineMask);
                // Lighting belongs to the fixed cylinder, never to the scrolling paper or its decorations.
                float facing=sqrt(saturate(1-height*height));
                paper*= (.04+.96*facing*facing)*(.80+.20*(1-abs(i.uv.x-.5)*2));
                float interior=smoothstep(.070,.105,i.uv.x)*(1-smoothstep(.900,.935,i.uv.x));
                interior*=smoothstep(.025,.080,i.uv.y)*(1-smoothstep(.920,.975,i.uv.y));
                fixed4 color=fixed4(lerp(rim.rgb,paper,interior),rim.a)*i.color;
                #ifdef UNITY_UI_CLIP_RECT
                color.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a-.001);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
