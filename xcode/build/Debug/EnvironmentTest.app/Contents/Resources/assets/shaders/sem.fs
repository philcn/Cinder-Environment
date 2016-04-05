#version 150

uniform sampler2D tMatCap;
uniform sampler2D tReflection;

in vec3 e;
in vec3 n;
in vec4 vPosition;

out vec4 oColor;

const vec3 kDiffuseColor = vec3(0.541, 0.106, 0.137);
const float kShininess = 10.0;
const float kNormalization = (kShininess + 8.0) / (3.14159265 * 8.0);
const vec3 kSpecularColor = vec3(0.25, 0.2, 0.1);
const vec3 kLightPosition = vec3(10, 0, 20);
const float kReflectionFactor = 2;

void main() {
  // spherical texture mapping
  vec3 r = reflect(e, n);
  float m = 2 * sqrt(pow(r.x, 2) + pow(r.y, 2) + pow(r.z + 1, 2));
  vec2 vN = r.xy / m + 0.5;
  
  vec3 reflection = texture(tReflection, vN).rgb;
  vec3 bakedIllumination = texture(tMatCap, vN).rgb;
  
  // use this if you do linear space lighting calculation
//  vec3 reflection = pow(texture(tReflection, vN).rgb, vec3(2.2));
//  vec3 bakedIllumination = pow(texture(tMatCap, vN).rgb, vec3(2.2));
  
	// Calculate lighting vectors.
	vec3 L = normalize( kLightPosition - e.xyz );
	vec3 E = normalize( -e.xyz );
	vec3 N = normalize( n );
	vec3 H = normalize( L + E );

	// Calculate diffuse lighting component.
	vec3 diffuse = kDiffuseColor * max( dot( N, L ), 0.0 );

	// Calculate specular lighting component.
	float specularFactor = kNormalization * pow( max( dot( N, H ), 0.0 ), kShininess );
  
  // specular reflection
  reflection *= specularFactor;
  reflection *= kReflectionFactor;
  reflection = clamp(reflection, 0, 1);
  
  oColor.rgb = sqrt(bakedIllumination + reflection);
  oColor.a = 1;
  
  oColor.rgb = pow(oColor.rgb, vec3(2.2));
}
