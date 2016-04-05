#version 150

uniform mat4	ciModelViewProjection;
uniform mat4	ciModelView;
uniform mat3	ciNormalMatrix;

in vec4	ciPosition;
in vec4	ciColor;
in vec3	ciNormal;

out vec4 vertPosition;
out vec4 vertColor;
out vec3 vertNormal;

void main()
{
	vertPosition = ciModelView * ciPosition;
  vertColor = ciColor;
	vertNormal = ciNormalMatrix * ciNormal;
  gl_Position = ciModelViewProjection * ciPosition;
}
