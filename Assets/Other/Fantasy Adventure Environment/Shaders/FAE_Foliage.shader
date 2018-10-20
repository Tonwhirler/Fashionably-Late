// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FAE/Foliage"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[NoScaleOffset]_MainTex("MainTex", 2D) = "white" {}
		[NoScaleOffset][Normal]_BumpMap("BumpMap", 2D) = "bump" {}
		_WindTint("WindTint", Range( -0.5 , 0.5)) = 0.1
		_AmbientOcclusion("AmbientOcclusion", Range( 0 , 1)) = 0
		_TransmissionSize("Transmission Size", Range( 0 , 20)) = 1
		_TransmissionAmount("Transmission Amount", Range( 0 , 10)) = 2.696819
		_MaxWindStrength("Max Wind Strength", Range( 0 , 1)) = 0.126967
		_WindSwinging("WindSwinging", Range( 0 , 1)) = 0
		_BendingInfluence("BendingInfluence", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "AlphaTest+0" }
		Cull Off
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "VS_InstancedIndirect.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma instancing_options assumeuniformscaling lodfade maxcount:50 procedural:setupScale
		#pragma multi_compile GPU_FRUSTUM_ON __
		#pragma exclude_renderers xbox360 psp2 n3ds wiiu 
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows nolightmap  nodirlightmap dithercrossfade vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 worldPos;
		};

		uniform sampler2D _BumpMap;
		uniform sampler2D _MainTex;
		uniform float _WindSpeed;
		uniform float4 _WindDirection;
		uniform float _WindSwinging;
		uniform float _MaxWindStrength;
		uniform float _WindStrength;
		uniform float _WindTint;
		uniform float _TransmissionSize;
		uniform float _TransmissionAmount;
		uniform float _WindDebug;
		uniform float _AmbientOcclusion;
		uniform float4 _ObstaclePosition;
		uniform float _BendingStrength;
		uniform float _BendingRadius;
		uniform float _BendingInfluence;
		uniform float _Cutoff = 0.5;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
			float2 appendResult518 = (float2(_WindDirection.x , _WindDirection.z));
			float3 temp_output_524_0 = sin( ( ( ( _WindSpeed * _Time.w ) * ase_objectScale ) * float3( appendResult518 ,  0.0 ) ) );
			float3 temp_cast_1 = (-1.0).xxx;
			float3 lerpResult544 = lerp( (float3( 0,0,0 ) + (temp_output_524_0 - temp_cast_1) * (float3( 1,0,0 ) - float3( 0,0,0 )) / (float3( 1,0,0 ) - temp_cast_1)) , temp_output_524_0 , _WindSwinging);
			float2 appendResult531 = (float2(lerpResult544.x , 0));
			float2 myVarName10535 = ( appendResult531 * _MaxWindStrength * _WindStrength * v.color.r );
			float2 Wind84 = myVarName10535;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 normalizeResult184 = normalize( ( _ObstaclePosition - float4( ase_worldPos , 0.0 ) ) );
			float temp_output_186_0 = ( _BendingStrength * 0.1 );
			float3 appendResult468 = (float3(temp_output_186_0 , 0 , temp_output_186_0));
			float clampResult192 = clamp( ( distance( _ObstaclePosition , float4( ase_worldPos , 0.0 ) ) / _BendingRadius ) , 0 , 1.0 );
			float4 Bending201 = ( v.color.r * -( ( ( normalizeResult184 * float4( appendResult468 , 0.0 ) ) * ( 1.0 - clampResult192 ) ) * _BendingInfluence ) );
			float2 VertexOffset330 = (( float4( Wind84, 0.0 , 0.0 ) + Bending201 )).xz;
			v.vertex.xyz += float3( VertexOffset330 ,  0.0 );
			v.normal = float3(0,1,0);
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_BumpMap172 = i.uv_texcoord;
			float3 Normals174 = UnpackScaleNormal( tex2D( _BumpMap, uv_BumpMap172 ) ,1.0 );
			o.Normal = Normals174;
			float2 uv_MainTex97 = i.uv_texcoord;
			float4 tex2DNode97 = tex2D( _MainTex, uv_MainTex97 );
			float4 temp_cast_0 = (2.0).xxxx;
			float3 ase_objectScale = float3( length( unity_ObjectToWorld[ 0 ].xyz ), length( unity_ObjectToWorld[ 1 ].xyz ), length( unity_ObjectToWorld[ 2 ].xyz ) );
			float2 appendResult518 = (float2(_WindDirection.x , _WindDirection.z));
			float3 temp_output_524_0 = sin( ( ( ( _WindSpeed * _Time.w ) * ase_objectScale ) * float3( appendResult518 ,  0.0 ) ) );
			float3 temp_cast_2 = (-1.0).xxx;
			float3 lerpResult544 = lerp( (float3( 0,0,0 ) + (temp_output_524_0 - temp_cast_2) * (float3( 1,0,0 ) - float3( 0,0,0 )) / (float3( 1,0,0 ) - temp_cast_2)) , temp_output_524_0 , _WindSwinging);
			float2 appendResult531 = (float2(lerpResult544.x , 0));
			float2 myVarName10535 = ( appendResult531 * _MaxWindStrength * _WindStrength * i.vertexColor.r );
			float2 Wind84 = myVarName10535;
			float lerpResult271 = lerp( Wind84.x , 0 , ( 1.0 - i.vertexColor.r ));
			float WindTint548 = ( ( lerpResult271 * _WindTint ) * 2.0 );
			float4 lerpResult273 = lerp( tex2DNode97 , temp_cast_0 , WindTint548);
			float4 Color161 = lerpResult273;
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			float dotResult141 = dot( -ase_worldViewDir , ase_worldlightDir );
			float lerpResult151 = lerp( ( pow( max( dotResult141 , 0 ) , _TransmissionSize ) * _TransmissionAmount ) , 0 , ( ( 1.0 - i.vertexColor.r ) * 1.33 ));
			float clampResult152 = clamp( lerpResult151 , 0 , 1.0 );
			float Subsurface153 = clampResult152;
			float4 lerpResult106 = lerp( Color161 , ( Color161 * 2.0 ) , Subsurface153);
			float4 FinalColor205 = lerpResult106;
			float4 lerpResult310 = lerp( FinalColor205 , float4( Wind84, 0.0 , 0.0 ) , _WindDebug);
			o.Albedo = lerpResult310.rgb;
			float clampResult302 = clamp( ( ( i.vertexColor.r * 1.33 ) * _AmbientOcclusion ) , 0 , 1.0 );
			float lerpResult115 = lerp( 1.0 , clampResult302 , _AmbientOcclusion);
			float AmbientOcclusion207 = lerpResult115;
			o.Occlusion = AmbientOcclusion207;
			o.Alpha = 1;
			float Alpha98 = tex2DNode97.a;
			float lerpResult313 = lerp( Alpha98 , 1.0 , _WindDebug);
			clip( lerpResult313 - _Cutoff );
		}

		ENDCG
	}
	Fallback "Nature/SpeedTree"
	CustomEditor "FAE.FoliageShaderGUI"
}
/*ASEBEGIN
Version=15001
1927;29;1906;1004;3476.571;-1191.444;1.138772;True;False
Node;AmplifyShaderEditor.CommentaryNode;507;-4087.935,-1526.998;Float;False;3795.727;1203.024;;21;84;535;534;531;527;16;385;529;544;248;526;543;524;520;517;518;514;516;513;511;319;Wind motion;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;319;-3803.496,-1456.072;Float;False;Global;_WindSpeed;_WindSpeed;11;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;513;-3736.842,-1357.696;Float;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;514;-3436.436,-1421.398;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;516;-3712.776,-1058.774;Float;False;Global;_WindDirection;_WindDirection;9;0;Create;True;0;0;False;0;1,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ObjectScaleNode;511;-3468.923,-1213.643;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;518;-3439.602,-1038.073;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;517;-3224.339,-1317.736;Float;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;520;-2936.371,-1055.272;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;543;-2654.021,-962.5554;Float;False;Constant;_Float2;Float 2;13;0;Create;True;0;0;False;0;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode;524;-2734.072,-1082.773;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;248;-2542.704,-830.3746;Float;False;Property;_WindSwinging;WindSwinging;8;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;526;-2436.502,-1065.218;Float;False;5;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;1,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;544;-2180.525,-1029.739;Float;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;529;-1964.893,-1022.705;Float;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;385;-1620.053,-461.7477;Float;False;Global;_WindStrength;_WindStrength;19;0;Create;True;0;0;False;0;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;202;-3197.001,-177.1511;Float;False;2627.3;775.1997;Bending;23;181;183;186;188;184;194;189;191;192;193;195;196;197;200;198;201;231;232;234;386;387;468;506;Foliage bending away from obstacle;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1691.323,-870.3198;Float;False;Property;_MaxWindStrength;Max Wind Strength;7;0;Create;True;0;0;False;0;0.126967;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;531;-1586.803,-1026.842;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;527;-1598.728,-742.9743;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;534;-1221.109,-942.2045;Float;False;4;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node;231;-3149.54,-21.90026;Float;False;Global;_ObstaclePosition;_ObstaclePosition;18;1;[HideInInspector];Create;True;0;0;False;0;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;181;-3132.901,260.3462;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;234;-2752.54,198.0997;Float;False;Global;_BendingStrength;_BendingStrength;15;1;[HideInInspector];Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;373;-2588.356,831.4046;Float;False;2020.167;388.1052;Comment;11;239;307;274;407;271;101;502;240;86;93;548;Color through wind;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;160;-3251.288,1459.145;Float;False;2711.621;557.9603;Subsurface scattering;17;153;152;380;151;149;147;148;146;145;150;141;143;139;140;138;503;550;Subsurface color simulation;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;232;-2770.54,494.1013;Float;False;Global;_BendingRadius;_BendingRadius;14;1;[HideInInspector];Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;386;-2733.566,277.5881;Float;False;Constant;_Float10;Float 10;19;0;Create;True;0;0;False;0;0.1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;189;-2728.102,360.0503;Float;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;535;-881.995,-953.4777;Float;False;myVarName10;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;93;-2538.357,881.4036;Float;False;84;0;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;387;-2491.766,512.8883;Float;False;Constant;_Float11;Float 11;19;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;138;-3105.49,1513.545;Float;False;World;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.VertexColorNode;86;-2249.954,1022.705;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;183;-2716.801,11.44478;Float;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;191;-2514.301,406.0505;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;84;-592.6204,-963.1284;Float;False;Wind;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-2524.901,207.147;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;192;-2343.301,406.0505;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;139;-2909.05,1510.851;Float;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;140;-3203.488,1675.545;Float;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;184;-2438.904,10.64699;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;468;-2318.499,188.0509;Float;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;240;-2290.947,884.5606;Float;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.OneMinusNode;502;-1979.193,1046.495;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;188;-2078.9,17.14789;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DotProductOpNode;141;-2743.491,1573.545;Float;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;193;-2099.301,408.0505;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;101;-1761.713,1083.204;Float;False;Property;_WindTint;WindTint;3;0;Create;True;0;0;False;0;0.1;0;-0.5;0.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;271;-1787.723,909.8607;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;148;-2149.892,1710.745;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;143;-2757.122,1735.206;Float;False;Property;_TransmissionSize;Transmission Size;5;0;Create;True;0;0;False;0;1;0;0;20;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;550;-2540.499,1578.627;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;274;-1430.625,905.5606;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;407;-1311.77,1046.111;Float;False;Constant;_Float13;Float 13;20;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;194;-1841.1,176.6488;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;195;-1833.604,412.4233;Float;False;Property;_BendingInfluence;BendingInfluence;9;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;150;-2075.692,1892.346;Float;False;Constant;_TransmissionHeight;TransmissionHeight;12;0;Create;True;0;0;False;0;1.33;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;145;-2350.844,1569.851;Float;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;196;-1526.547,180.2964;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;236;-2419.377,3192.074;Float;False;1901.952;536.7815;SSS Blending with color;11;205;106;547;296;295;161;549;98;273;497;97;Final color;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;307;-1206.763,921.5057;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;503;-1959.698,1730.817;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;146;-2465.291,1732.945;Float;False;Property;_TransmissionAmount;Transmission Amount;6;0;Create;True;0;0;False;0;2.696819;0;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;149;-1789.491,1741.945;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;159;-2367.693,2262.087;Float;False;1813.59;398.8397;AO;11;207;115;114;117;301;118;113;111;302;381;382;Ambient Occlusion by Red vertex color channel;1,1,1,1;0;0
Node;AmplifyShaderEditor.VertexColorNode;198;-1395.148,-4.388111;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;548;-987.9922,910.64;Float;False;WindTint;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;549;-2251.758,3568.538;Float;False;548;0;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;197;-1352.842,182.2973;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;497;-2240.694,3458.267;Float;False;Constant;_Float0;Float 0;20;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;147;-2142.892,1575.345;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;97;-2366.277,3258.45;Float;True;Property;_MainTex;MainTex;1;1;[NoScaleOffset];Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;200;-1163.417,146.3718;Float;False;2;2;0;FLOAT;0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexColorNode;111;-2317.692,2312.087;Float;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;273;-1928.839,3336.538;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;151;-1589.291,1574.945;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;382;-2037.86,2424.16;Float;False;Constant;_Float6;Float 6;19;0;Create;True;0;0;False;0;1.33;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;380;-1241.679,1774.337;Float;False;Constant;_Float4;Float 4;19;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode;506;-1007.616,142.1316;Float;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;374;254.2972,-61.15241;Float;False;1417.88;276.6575;Comment;5;330;393;203;204;85;Vertex function layer blend;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;296;-1579.436,3466.872;Float;False;Constant;_Float1;Float 1;21;0;Create;True;0;0;False;0;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;301;-1798.006,2332.809;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;113;-2249.473,2520.067;Float;False;Property;_AmbientOcclusion;AmbientOcclusion;4;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;161;-1738.614,3342.875;Float;False;Color;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;152;-1035.492,1567.445;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;381;-1573.458,2569.56;Float;False;Constant;_Float5;Float 5;19;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;237;-1533.39,2770.484;Float;False;978.701;287.5597;;3;174;172;419;Normal map;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;547;-1256.909,3553.238;Float;False;153;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;85;339.7766,-11.15247;Float;False;84;0;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;204;307.2693,79.6245;Float;False;201;0;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;153;-858.9927,1570.345;Float;False;Subsurface;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;295;-1333.547,3428.735;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;118;-1480.372,2538.067;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;201;-818.7435,139.0677;Float;False;Bending;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;114;-1595.175,2357.266;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;302;-1362.306,2340.009;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;419;-1420.771,2899.029;Float;False;Constant;_Float18;Float 18;21;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;203;749.0701,5.023552;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;235;2843.666,889.9761;Float;False;452.9371;811.1447;Final;5;99;208;175;206;331;Outputs;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;117;-1442.274,2477.367;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;375;2964.505,1790.556;Float;False;352;249.0994;Comment;2;312;311;Debug switch;1,1,1,1;0;0
Node;AmplifyShaderEditor.LerpOp;106;-965.9405,3349.727;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;311;3014.505,1924.656;Float;False;Global;_WindDebug;_WindDebug;20;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;206;3072.166,941.4243;Float;False;205;0;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;406;3416.104,1297.26;Float;False;Constant;_Float12;Float 12;20;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;98;-1934.471,3256.42;Float;False;Alpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;99;3096.573,1235.245;Float;False;98;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;312;3073.705,1840.556;Float;False;84;0;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;172;-1182.389,2825.043;Float;True;Property;_BumpMap;BumpMap;2;2;[NoScaleOffset];[Normal];Create;True;0;0;False;0;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode;393;962.9843,21.27726;Float;False;FLOAT2;0;2;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;115;-1080.073,2357.267;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;205;-771.8661,3343.871;Float;False;FinalColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;208;3025.767,1141.224;Float;False;207;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;330;1192.521,16.59738;Float;False;VertexOffset;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;174;-797.689,2820.483;Float;False;Normals;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;451;3155.699,2077.05;Float;False;Constant;_Vector0;Vector 0;21;0;Create;True;0;0;False;0;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;207;-860.4883,2350.822;Float;False;AmbientOcclusion;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;331;3064.599,1369.667;Float;False;330;0;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;239;-2013.148,883.1597;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;175;3082.283,1039.971;Float;False;174;0;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;313;3587.307,1254.955;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;310;3589.109,973.5546;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3894.866,1047.348;Float;False;True;2;Float;FAE.FoliageShaderGUI;0;0;Standard;FAE/Foliage;False;False;False;False;False;False;True;False;True;False;False;False;True;False;False;False;True;False;False;Off;0;False;-1;0;False;-1;False;0;0;False;0;Custom;0.5;True;True;0;True;Opaque;;AlphaTest;All;True;True;True;True;True;True;True;False;True;True;False;False;False;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;Nature/SpeedTree;0;-1;-1;-1;1;VS_InstancedIndirect.cginc;0;2;instancing_options assumeuniformscaling lodfade maxcount:50 procedural:setupScale;multi_compile GPU_FRUSTUM_ON __;False;0;0;0;False;-1;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;514;0;319;0
WireConnection;514;1;513;4
WireConnection;518;0;516;1
WireConnection;518;1;516;3
WireConnection;517;0;514;0
WireConnection;517;1;511;0
WireConnection;520;0;517;0
WireConnection;520;1;518;0
WireConnection;524;0;520;0
WireConnection;526;0;524;0
WireConnection;526;1;543;0
WireConnection;544;0;526;0
WireConnection;544;1;524;0
WireConnection;544;2;248;0
WireConnection;529;0;544;0
WireConnection;531;0;529;0
WireConnection;534;0;531;0
WireConnection;534;1;16;0
WireConnection;534;2;385;0
WireConnection;534;3;527;1
WireConnection;189;0;231;0
WireConnection;189;1;181;0
WireConnection;535;0;534;0
WireConnection;183;0;231;0
WireConnection;183;1;181;0
WireConnection;191;0;189;0
WireConnection;191;1;232;0
WireConnection;84;0;535;0
WireConnection;186;0;234;0
WireConnection;186;1;386;0
WireConnection;192;0;191;0
WireConnection;192;2;387;0
WireConnection;139;0;138;0
WireConnection;184;0;183;0
WireConnection;468;0;186;0
WireConnection;468;2;186;0
WireConnection;240;0;93;0
WireConnection;502;0;86;1
WireConnection;188;0;184;0
WireConnection;188;1;468;0
WireConnection;141;0;139;0
WireConnection;141;1;140;0
WireConnection;193;0;192;0
WireConnection;271;0;240;0
WireConnection;271;2;502;0
WireConnection;550;0;141;0
WireConnection;274;0;271;0
WireConnection;274;1;101;0
WireConnection;194;0;188;0
WireConnection;194;1;193;0
WireConnection;145;0;550;0
WireConnection;145;1;143;0
WireConnection;196;0;194;0
WireConnection;196;1;195;0
WireConnection;307;0;274;0
WireConnection;307;1;407;0
WireConnection;503;0;148;1
WireConnection;149;0;503;0
WireConnection;149;1;150;0
WireConnection;548;0;307;0
WireConnection;197;0;196;0
WireConnection;147;0;145;0
WireConnection;147;1;146;0
WireConnection;200;0;198;1
WireConnection;200;1;197;0
WireConnection;273;0;97;0
WireConnection;273;1;497;0
WireConnection;273;2;549;0
WireConnection;151;0;147;0
WireConnection;151;2;149;0
WireConnection;506;0;200;0
WireConnection;301;0;111;1
WireConnection;301;1;382;0
WireConnection;161;0;273;0
WireConnection;152;0;151;0
WireConnection;152;2;380;0
WireConnection;153;0;152;0
WireConnection;295;0;161;0
WireConnection;295;1;296;0
WireConnection;118;0;113;0
WireConnection;201;0;506;0
WireConnection;114;0;301;0
WireConnection;114;1;113;0
WireConnection;302;0;114;0
WireConnection;302;2;381;0
WireConnection;203;0;85;0
WireConnection;203;1;204;0
WireConnection;117;0;118;0
WireConnection;106;0;161;0
WireConnection;106;1;295;0
WireConnection;106;2;547;0
WireConnection;98;0;97;4
WireConnection;172;5;419;0
WireConnection;393;0;203;0
WireConnection;115;0;381;0
WireConnection;115;1;302;0
WireConnection;115;2;117;0
WireConnection;205;0;106;0
WireConnection;330;0;393;0
WireConnection;174;0;172;0
WireConnection;207;0;115;0
WireConnection;239;0;240;0
WireConnection;239;1;240;1
WireConnection;313;0;99;0
WireConnection;313;1;406;0
WireConnection;313;2;311;0
WireConnection;310;0;206;0
WireConnection;310;1;312;0
WireConnection;310;2;311;0
WireConnection;0;0;310;0
WireConnection;0;1;175;0
WireConnection;0;5;208;0
WireConnection;0;10;313;0
WireConnection;0;11;331;0
WireConnection;0;12;451;0
ASEEND*/
//CHKSM=F1C4E540FFEBBC28337571FB374E15A86E52177C