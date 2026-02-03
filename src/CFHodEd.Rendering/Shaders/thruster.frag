#version 330 core

// Thruster shader - glowing exhaust effect

in vec3 vWorldPos;
in vec3 vNormal;
in vec2 vTexCoord;
in vec3 vViewDir;

uniform sampler2D uDiffuseMap;
uniform sampler2D uGlowMap;
uniform float uThrusterPower;  // 0-1, from c2.w

out vec4 FragColor;

void main()
{
    vec4 diffuse = texture(uDiffuseMap, vTexCoord);
    vec4 glow = texture(uGlowMap, vTexCoord);
    
    // Thruster glow based on power
    vec3 thrusterGlow = glow.rgb * uThrusterPower * 2.0;
    
    // Blend with additive glow
    vec3 finalColor = diffuse.rgb + thrusterGlow;
    
    FragColor = vec4(finalColor, diffuse.a);
}
