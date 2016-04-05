#version 150

uniform sampler2D uRenderedTexture;
uniform sampler2D uDepthTexture;

uniform float uMaxBlur;
uniform float uAperture;
uniform float uFocalDepth;
uniform float uAspectRatio;

in vec2 vTexCoord0;
out vec4 oColor;

void main() {
  vec2 aspectcorrect = vec2(1.0, uAspectRatio);
  vec4 depth = texture(uDepthTexture, vTexCoord0);
  float factor = depth.x - uFocalDepth;
  
  vec2 dofblur = vec2(clamp(factor * uAperture, -uMaxBlur, uMaxBlur));
  vec2 dofblur9 = dofblur * 0.9;
  vec2 dofblur7 = dofblur * 0.7;
  vec2 dofblur4 = dofblur * 0.4;
  vec4 col = vec4(0.0);
  
  col += texture(uRenderedTexture, vTexCoord0.xy);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.0,   0.4  ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.15,  0.37 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.29,  0.29 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.37,  0.15 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.40,  0.0  ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.37, -0.15 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.29, -0.29 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.15, -0.37 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.0,  -0.4  ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.15,  0.37 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.29,  0.29 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.37,  0.15 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.4,   0.0  ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.37, -0.15 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.29, -0.29 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.15, -0.37 ) * aspectcorrect) * dofblur);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.15,  0.37 ) * aspectcorrect) * dofblur9);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.37,  0.15 ) * aspectcorrect) * dofblur9);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.37, -0.15 ) * aspectcorrect) * dofblur9);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.15, -0.37 ) * aspectcorrect) * dofblur9);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.15,  0.37 ) * aspectcorrect) * dofblur9);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.37,  0.15 ) * aspectcorrect) * dofblur9);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.37, -0.15 ) * aspectcorrect) * dofblur9);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.15, -0.37 ) * aspectcorrect) * dofblur9);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.29,  0.29 ) * aspectcorrect) * dofblur7);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.40,  0.0  ) * aspectcorrect) * dofblur7);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.29, -0.29 ) * aspectcorrect) * dofblur7);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.0,  -0.4  ) * aspectcorrect) * dofblur7);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.29,  0.29 ) * aspectcorrect) * dofblur7);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.4,   0.0  ) * aspectcorrect) * dofblur7);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.29, -0.29 ) * aspectcorrect) * dofblur7);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.0,   0.4  ) * aspectcorrect) * dofblur7);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.29,  0.29 ) * aspectcorrect) * dofblur4);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.4,   0.0  ) * aspectcorrect) * dofblur4);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.29, -0.29 ) * aspectcorrect) * dofblur4);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.0,  -0.4  ) * aspectcorrect) * dofblur4);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.29,  0.29 ) * aspectcorrect) * dofblur4);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.4,   0.0  ) * aspectcorrect) * dofblur4);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2( -0.29, -0.29 ) * aspectcorrect) * dofblur4);
  col += texture(uRenderedTexture, vTexCoord0.xy + (vec2(  0.0,   0.4  ) * aspectcorrect) * dofblur4);
  
  oColor = col / 41.0;
  oColor.a = 1.0;
}
