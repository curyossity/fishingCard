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
        _AnimateEdges ("Rotate Drum Edge Details", Float) = 0
        _EdgeLayer ("Outer Drum Layer", Float) = 0
        _EdgeStyle ("Drum Motion Style", Float) = 0
        _EdgeBandUV ("Left/Right Drum Material Strips", Vector) = (.025,.060,.944,.980)
        _DrumTexelSize ("Preserved Drum Texel Size", Vector) = (.002427184,.002040816,0,0)
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
            float _Phase, _Radius, _AnimateEdges, _EdgeLayer, _EdgeStyle;
            float4 _CylinderSize, _PlaqueUV, _DividerUV, _Plaque, _Dividers, _ClipRect, _EdgeBandUV;
            float4 _DrumTexelSize;
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
                float facing=sqrt(saturate(1-height*height));
                if (_AnimateEdges>.5)
                {
                    // The circumference follows the same angle as the paper; the outline and illumination stay fixed.
                    float inset=.062+.10*pow(abs(height),6);
                    float sideDistance=min(i.uv.x,1-i.uv.x);
                    float bandRegion=1-smoothstep(inset*.90,inset*1.15,sideDistance);
                    bandRegion=max(bandRegion,1-smoothstep(.04,.10,min(i.uv.y,1-i.uv.y)));
                    if (bandRegion>0)
                    {
                        float across=saturate(sideDistance/inset);
                        float stripU=i.uv.x<.5 ? lerp(_EdgeBandUV.x,_EdgeBandUV.y,across) : lerp(_EdgeBandUV.w,_EdgeBandUV.z,across);
                        fixed3 band;
                        if (_EdgeStyle>.5)
                        {
                            // Eight physical plates travel around the cylinder, compressing naturally at the rounded ends.
                            float plateTurn=surfaceAngle/(pi*.25);
                            float plateIndex=floor(frac(surfaceAngle/(2*pi))*8);
                            float joint=(frac(plateTurn+.5)-.5)*(pi*.25)*_Radius;
                            float shadow=1-smoothstep(1.1,3.0,abs(joint));
                            float bevel=1-smoothstep(.45,1.25,abs(joint-3.2));
                            band=fixed3(.32,.235,.145)*(.94+.06*sin(plateIndex*1.7));
                            band*=.55+.45*saturate(1-abs(across-.65)*2);
                            float railBevel=1-smoothstep(.04,.12,abs(across-.86));
                            band+=fixed3(.07,.05,.025)*railBevel;
                            band*=lerp(1,.28,shadow);
                            band+=fixed3(.35,.25,.12)*bevel;
                            float rivetX=abs(across-.65)*inset*_CylinderSize.x;
                            float rivetY=abs(frac(plateTurn)-.5)*(pi*.25)*_Radius;
                            float rivet=step(rivetX,1.2)*step(rivetY,1.6);
                            band=lerp(band,fixed3(.70,.49,.24),rivet);
                            band*=.15+.85*facing;
                        }
                        else
                        {
                            // Preserve the preceding continuous-wear animation as a selectable alternative.
                            float edgeV=frac(surfaceAngle/(2*pi));
                            band=tex2D(_MainTex,float2(stripU,lerp(.20,.80,edgeV))).rgb;
                            float seamDistance=min(abs(faceY-_Dividers.z),abs(faceY-_Dividers.w));
                            float seam=1-smoothstep(1,2.8,seamDistance);
                            band*=lerp(1,.48,seam);
                            float seamBevel=1-smoothstep(.7,1.6,abs(seamDistance-2.4));
                            band+=fixed3(.16,.12,.07)*seamBevel;
                            band*=.065+.585*facing;
                        }
                        // Keep the outer two-texel socket lip in the supplied art instead of scrolling its silhouette.
                        float2 dx=float2(_DrumTexelSize.x*2,0),dy=float2(0,_DrumTexelSize.y*2);
                        float innerAlpha=min(min(tex2D(_MainTex,i.uv+dx).a,tex2D(_MainTex,i.uv-dx).a),
                            min(tex2D(_MainTex,i.uv+dy).a,tex2D(_MainTex,i.uv-dy).a));
                        innerAlpha=min(innerAlpha,rim.a);
                        // The supplied metal is mostly alpha 251/255; an opaque-only cutoff freezes that entire band.
                        rim.rgb=lerp(rim.rgb,band,bandRegion*step(.08,innerAlpha));
                    }
                }
                if (_EdgeLayer>.5)
                {
                    fixed4 edgeColor=rim*i.color;
                    #ifdef UNITY_UI_CLIP_RECT
                    edgeColor.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
                    #endif
                    #ifdef UNITY_UI_ALPHACLIP
                    clip(edgeColor.a-.001);
                    #endif
                    return edgeColor;
                }
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
