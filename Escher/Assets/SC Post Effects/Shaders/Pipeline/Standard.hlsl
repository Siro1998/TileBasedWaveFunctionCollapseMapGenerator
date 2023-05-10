//Libraries

//Not using the libraries that come with the Post Processing package, since these don't support stereo matrices, otherwise required for full VR compability
#include "UnityCG.cginc"
#include "../Blending.hlsl"

//Use shared texture samplers, which is in line with SRP methods
SamplerState sampler_LinearClamp;
SamplerState sampler_LinearRepeat;
#define Clamp sampler_LinearClamp
#define Repeat sampler_LinearRepeat

//Note: OpenGLES 2.0 will not support texture samplers. This will cause build errors
//"#pragma exclude_renderers gles" must be added to each pass
#ifdef SHADER_API_GLES
#pragma warning "The OpenGLES 2.0 graphics API is not supported."
#endif

//Wrappers so the same macros used in URP/HDRP can be used
//Not using the macros defined in HLSLSupport.cginc since this prefixes "sampler_" to the sampler name
#define SAMPLER(samplerName) SamplerState samplerName
#define SAMPLE_TEXTURE2D(textureName, sampler, uv) textureName.Sample(sampler, uv)
#define SAMPLE_TEXTURE2D_LOD(textureName, sampler, uv, lod) textureName.Sample(sampler, uv, lod)

#define TEXTURE2D(textureName) Texture2D textureName
#define TEXTURE2D_X(textureName) TEXTURE2D(textureName) //Post processing framework does not sample from a texture array. Otherwise this should resolve to UNITY_DECLARE_SCREENSPACE_TEXTURE
#define SAMPLE_TEXTURE2D_X(textureName, sampler, uv) SAMPLE_TEXTURE2D(textureName, sampler, uv)
#define SAMPLE_TEXTURE2D_X_LOD(textureName, sampler, uv, lod) SAMPLE_TEXTURE2D_LOD(textureName, sampler, uv, lod)

//Actually swapped
#define TEXTURE2D_PARAM(textureName, samplerName) Texture2D textureName, SamplerState samplerName
#define TEXTURE2D_ARGS(textureName, samplerName) textureName, samplerName
//Again, no actual texture array usage
#define TEXTURE2D_X_PARAM(textureName, samplerName) TEXTURE2D_PARAM(textureName, samplerName)
#define TEXTURE2D_X_ARGS(textureName, samplerName) TEXTURE2D_ARGS(textureName, samplerName)

//Post processing framework does not sample from a texture array. Instead it does multi-pass style rendering
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
float4 _MainTex_TexelSize;

//Shorthand for sampling MainTex
#define SCREEN_COLOR(uv) SAMPLE_TEXTURE2D(_MainTex, Clamp, uv);

float4 ScreenColor(float2 uv)
{
	return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
}

float4 ScreenColorTiled(float2 uv) {
	return SAMPLE_TEXTURE2D(_MainTex, Repeat, uv);
}

//Automatically unrolls to a texture array if VR is used
UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthNormalsTexture);

//Not defined when STEREO_INSTANCING_ON is enabled, while it should be!
float _DepthSlice;

#define SAMPLE_DEPTH(uv) SAMPLE_RAW_DEPTH_TEXTURE(_CameraDepthTexture, uv)
#define LINEAR_DEPTH(depth) Linear01Depth(depth)
#define LINEAR_EYE_DEPTH(depth) RawToLinearDepth(depth)
#define SAMPLE_DEPTH_NORMALS(uv) UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthNormalsTexture, uv);

//Generic functions
#include "../SCPE.hlsl"
#define WorldViewDirection -unity_MatrixV[2].xyz

//Fragment
#define UV input.uv

float RawToLinearDepth(float rawDepth)
{
	if(unity_OrthoParams.w == 0) //Perspective
	{
		return LinearEyeDepth(rawDepth);
	}
	
	//Ortho
	#if UNITY_REVERSED_Z
	return ((_ProjectionParams.z - _ProjectionParams.y) * (1.0 - rawDepth) + _ProjectionParams.y);
	#else
	return ((_ProjectionParams.z - _ProjectionParams.y) * (rawDepth) + _ProjectionParams.y);
	#endif
}

//Structs (same names as URP/HDRP)
struct Attributes
{
	float3 positionOS : POSITION;
	float2 uv : TEXCOORD0;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;

	UNITY_VERTEX_OUTPUT_STEREO
};

// Vertex manipulation
float2 TransformTriangleVertexToUV(float2 vertex)
{
	float2 uv = (vertex + 1.0) * 0.5;
	return uv;
}

Varyings Vert(Attributes input)
{
	Varyings output;

	//Not needed, no actual instancing is being used
	UNITY_SETUP_INSTANCE_ID(input);
	
	//Post processing framework does not actually use real instanced rendering
	//Instead set this with the faux eye index set up through script
	//This overwrites the behaviour of the UNITY_SETUP_INSTANCE_ID macro
	#if defined(UNITY_STEREO_INSTANCING_ENABLED)
	unity_StereoEyeIndex = (uint)_DepthSlice;
	#endif
	
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

	output.positionCS = float4(input.positionOS.xy, 0.0, 1.0);
	output.uv = TransformTriangleVertexToUV(input.positionOS.xy).xy;

	#if UNITY_UV_STARTS_AT_TOP
	output.uv = output.uv * float2(1.0, -1.0) + float2(0.0, 1.0);
	#endif

	return output;
}

/////////////
//Functions declared in the Post Processing package's "StdLib.hlsl" library (otherwise available in the Core SRP package)
/////////////

#define FLT_EPSILON     1.192092896e-07 // Smallest positive number, such that 1.0 + FLT_EPSILON != 1.0

float3 RgbToHsv(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
	float d = q.x - min(q.w, q.y);
	float e = EPSILON;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HsvToRgb(float3 c)
{
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

float3 PositivePow(float3 base, float3 power)
{
	return pow(max(abs(base), float3(FLT_EPSILON, FLT_EPSILON, FLT_EPSILON)), power);
}

half3 LinearToSRGB(half3 c)
{
	#if USE_VERY_FAST_SRGB
	return sqrt(c);
	#elif USE_FAST_SRGB
	return max(1.055 * PositivePow(c, 0.416666667) - 0.055, 0.0);
	#else
	half3 sRGBLo = c * 12.92;
	half3 sRGBHi = (PositivePow(c, half3(1.0 / 2.4, 1.0 / 2.4, 1.0 / 2.4)) * 1.055) - 0.055;
	half3 sRGB = (c <= 0.0031308) ? sRGBLo : sRGBHi;
	return sRGB;
	#endif
}

half3 SRGBToLinear(half3 c)
{
	#if USE_VERY_FAST_SRGB
	return c * c;
	#elif USE_FAST_SRGB
	return c * (c * (c * 0.305306011 + 0.682171111) + 0.012522878);
	#else
	half3 linearRGBLo = c / 12.92;
	half3 linearRGBHi = PositivePow((c + 0.055) / 1.055, half3(2.4, 2.4, 2.4));
	half3 linearRGB = (c <= 0.04045) ? linearRGBLo : linearRGBHi;
	return linearRGB;
	#endif
}
