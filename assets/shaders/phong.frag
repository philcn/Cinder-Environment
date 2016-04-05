#version 150

in vec4 vertPosition;
in vec4 vertColor;
in vec3 vertNormal;

out vec4 fragColor;

const vec3 kAmbientColor = vec3(0.2);
const float kShininess = 13.0;
const float kNormalization = (kShininess + 8.0) / (3.14159265 * 8.0);
const vec3 kSpecularColor = vec3(0.2, 0.2, 0.1);
const vec3 kLightPosition = vec3(10, 0, 20);

void main(){
  // Calculate lighting vectors.
  vec3 L = normalize( kLightPosition - vertPosition.xyz );
  vec3 E = normalize( -vertPosition.xyz );
  vec3 N = normalize( vertNormal );
  vec3 H = normalize( L + E );

  // Calculate diffuse lighting component.
  vec3 kDiffuseColor = vertColor.rgb;
  vec3 diffuse = kDiffuseColor * max( dot( N, L ), 0.0 );

  // Calculate specular lighting component.
  vec3 specular = kNormalization * kSpecularColor * pow( max( dot( N, H ), 0.0 ), kShininess );

  // Output final color.
  fragColor = vec4( kAmbientColor + diffuse + specular, 1.0 );
}
