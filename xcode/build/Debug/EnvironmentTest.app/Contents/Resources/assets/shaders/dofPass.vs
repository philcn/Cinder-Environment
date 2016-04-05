#version 150

uniform mat4 ciModelViewProjection;

in vec4 ciPosition;
in vec2 ciTexCoord0;

out vec2 vTexCoord0;

void main() {
  gl_Position = ciModelViewProjection * ciPosition;
  vTexCoord0 = ciTexCoord0;
}