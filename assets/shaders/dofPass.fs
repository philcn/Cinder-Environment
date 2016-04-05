#version 150

in vec2 vTexCoord0;
out vec4 oColor;

uniform sampler2D uRenderedTexture;
uniform sampler2D uDepthTexture;
uniform float uRenderedTextureWidth;
uniform float uRenderedTextureHeight;

uniform bool uUseAutofocus = true; // disable if you use external uFocalDepth value
uniform float uFocalDepth; // external focal point value, but you may use autofocus option below
uniform vec2 uAutofocusCenter = vec2(0.5, 0.5); // (0.0,0.0 - left lower corner, 1.0,1.0 - upper right)
uniform float uAperture = 4.0; // controls the focal range
uniform float uMaxBlur = 1.25;

uniform int uNumRings = 5;
uniform int uNumSamples = 4;
uniform float uHighlightThreshold = 0.5;
uniform float uHighlightGain = 20.0;
uniform float uBokehEdgeBias = 0.4;
uniform float uBokehFringe = 0.5; // bokeh chromatic aberration/fringing

const bool useNoiseDithering = true; // use noise instead of pattern for sample dithering
const float ditherAmount = 0.00001;
const bool useBdepth = true; // blur the depth buffer?
const float depthBufferBlurSize = 0.0;

float width = uRenderedTextureWidth;
float height = uRenderedTextureHeight;
vec2 texel = vec2(1.0 / width, 1.0 / height);

#define PI 3.14159265

const float zFar = 200;
const float zNear = 0.9;
const bool useLinearDepth = false;

float linearDepth(float depthSample) {
  depthSample = 2.0 * depthSample - 1.0;
  float zLinear = 2.0 * zNear * zFar / (zFar + zNear - depthSample * (zFar - zNear));
  return zLinear;
}

float bdepth(vec2 coord) {
  float d = 0.0;
  float kernel[9];
  vec2 offset[9];

  vec2 wh = vec2(texel.x, texel.y) * depthBufferBlurSize;

  offset[0] = vec2(-wh.x,-wh.y);
  offset[1] = vec2( 0.0, -wh.y);
  offset[2] = vec2( wh.x -wh.y);

  offset[3] = vec2(-wh.x,  0.0);
  offset[4] = vec2( 0.0,   0.0);
  offset[5] = vec2( wh.x,  0.0);

  offset[6] = vec2(-wh.x, wh.y);
  offset[7] = vec2( 0.0,  wh.y);
  offset[8] = vec2( wh.x, wh.y);

  kernel[0] = 1.0 / 16.0;   kernel[1] = 2.0 / 16.0;   kernel[2] = 1.0 / 16.0;
  kernel[3] = 2.0 / 16.0;   kernel[4] = 4.0 / 16.0;   kernel[5] = 2.0 / 16.0;
  kernel[6] = 1.0 / 16.0;   kernel[7] = 2.0 / 16.0;   kernel[8] = 1.0 / 16.0;

  for (int i = 0; i < 9; i++) {
    float tmp = texture(uDepthTexture, coord + offset[i]).r;
    d += tmp * kernel[i];
  }

  return d;
}

// processing the sample
vec3 color(vec2 coord, float blurAmount) {
  vec3 col = vec3(0.0);

  col.r = texture(uRenderedTexture, coord + vec2(0.0, 1.0) * texel * uBokehFringe * blurAmount).r;
  col.g = texture(uRenderedTexture, coord + vec2(-0.866, -0.5) * texel * uBokehFringe * blurAmount).g;
  col.b = texture(uRenderedTexture, coord + vec2(0.866, -0.5) * texel * uBokehFringe * blurAmount).b;

  vec3 lumcoeff = vec3(0.299, 0.587, 0.114);
  float lum = dot(col.rgb, lumcoeff);
  float thresh = max((lum - uHighlightThreshold) * uHighlightGain, 0.0);
  return col + mix(vec3(0.0), col, thresh * blurAmount);
}

// generating noise/pattern texture for dithering
vec2 rand(in vec2 coord) {
  float noiseX;
  float noiseY;

  if (useNoiseDithering) {
    noiseX = clamp(fract(sin(dot(coord, vec2(12.9898, 78.233))) * 43758.5453), 0.0, 1.0) * 2.0 - 1.0;
    noiseY = clamp(fract(sin(dot(coord, vec2(12.9898, 78.233) * 2.0)) * 43758.5453), 0.0, 1.0) * 2.0 - 1.0;
  } else {
    noiseX = ((fract(1.0 - coord.s * (width / 2.0)) * 0.25) + (fract(coord.t * (height / 2.0)) * 0.75)) * 2.0 - 1.0;
    noiseY = ((fract(1.0 - coord.s * (width / 2.0)) * 0.75) + (fract(coord.t * (height / 2.0)) * 0.25)) * 2.0 - 1.0;
  }

  return vec2(noiseX, noiseY);
}

void main() {
  float pixelDepth;
  float focalDepth;
  float blurAmount;

  if (useBdepth) {
    pixelDepth = bdepth(vTexCoord0.xy);
  } else {
    pixelDepth = texture(uDepthTexture, vTexCoord0.xy).x;
  }

  if (uUseAutofocus) {
    focalDepth = texture(uDepthTexture, uAutofocusCenter).x;
  } else {
    focalDepth = uFocalDepth;
    if (useLinearDepth) {
      pixelDepth = linearDepth(pixelDepth);
    }
  }
  
  // TODO: use real dof formula to correct
  blurAmount = clamp((abs(pixelDepth - focalDepth) / uAperture) * 100.0, -uMaxBlur, uMaxBlur);

  vec2 noise = rand(vTexCoord0.xy) * ditherAmount * blurAmount;

  float w = (1.0 / width) * blurAmount + noise.x;
  float h = (1.0 / height) * blurAmount + noise.y;

  vec3 col = texture(uRenderedTexture, vTexCoord0.xy).rgb;
  float s = 1.0;

  bool leaking = false;
  const float leakingDepthThreshold = 0.2;
  
  for (int j = 0; j < uNumRings; j++) {
    float step = PI * 2.0 / float(uNumRings);
    float pw = (cos(float(j) * step) * float(uNumSamples));
    float ph = (sin(float(j) * step) * float(uNumSamples));
    float tapDepth = texture(uDepthTexture, vTexCoord0.xy + vec2(pw * w, ph * h)).x;
    if (abs(tapDepth - pixelDepth) > leakingDepthThreshold) {
      leaking = true;
      break;
    }
  }
  
  for (int i = 1; i <= uNumRings; i++) {
    int ringSamples = i * uNumSamples;
    for (int j = 0; j < ringSamples ; j++) {
      float step = PI * 2.0 / float(ringSamples);
      float pw = (cos(float(j) * step) * float(i));
      float ph = (sin(float(j) * step) * float(i));
      float p = 1.0;
      if (leaking) {
        float tapDepth = texture(uDepthTexture, vTexCoord0.xy + vec2(pw * w, ph * h)).x;
        if (abs(tapDepth - pixelDepth) > leakingDepthThreshold) {
          continue;
        }
      }
      col += color(vTexCoord0.xy + vec2(pw * w, ph * h), blurAmount) * mix(1.0, (float(i)) / (float(uNumRings)), uBokehEdgeBias) * p;
      s += 1.0 * mix(1.0, (float(i)) / (float(uNumRings)), uBokehEdgeBias) * p;
    }
  }

  col /= s;

  oColor.rgb = col;
  oColor.a = 1.0;

  // DEBUG show depth buffer
//  oColor.rgb = texture(uDepthTexture, vTexCoord0.xy).rgb;
}
