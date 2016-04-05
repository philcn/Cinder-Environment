#version 150

out vec3 e;
out vec3 n;

uniform mat4 ciProjectionMatrix;
uniform mat4 ciModelView;
uniform mat3 ciNormalMatrix;

in vec4	ciPosition;
in vec3	ciNormal;

void main() {
  e = normalize(vec3(ciModelView * ciPosition));
  n = normalize(ciNormalMatrix * ciNormal);
  
  gl_Position = ciProjectionMatrix * ciModelView * ciPosition;
}
