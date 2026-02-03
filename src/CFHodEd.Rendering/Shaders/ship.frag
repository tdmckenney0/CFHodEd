#version 330 core

// Homeworld 2 Ship Shader - GLSL port of ship.ps
// Handles team colors, stripe colors, glow, specular

in vec3 vWorldPos;
in vec3 vNormal;
in vec2 vTexCoord;
in vec3 vViewDir;

uniform sampler2D uDiffuseMap;   // Diffuse texture (stage 0)
uniform sampler2D uGlowMap;      // Glow texture (stage 1)
uniform sampler2D uTeamMap;      // Team color mask (stage 2)

uniform vec4 uTeamColor;         // Team color (c0)
uniform vec4 uStripeColor;       // Stripe color (c1)
uniform vec4 uAmbient;           // Ambient light

// Light data (up to 8 lights)
struct Light {
    vec3 position;
    vec3 diffuse;
    vec3 specular;
    vec3 attenuation;
    bool enabled;
};
uniform Light uLights[8];

out vec4 FragColor;

void main()
{
    // Sample textures
    vec4 diffuse = texture(uDiffuseMap, vTexCoord);
    vec4 glow = texture(uGlowMap, vTexCoord);
    vec4 team = texture(uTeamMap, vTexCoord);
    
    // Adjust color underlying base (from ship.ps)
    // Make darker/lighter for team/stripe
    vec4 darker = diffuse + 0.5;
    vec3 teamAdjusted = clamp(darker.rgb * uTeamColor.rgb + (diffuse.rgb - 0.5), 0.0, 1.0);
    vec3 stripeAdjusted = clamp(darker.rgb * uStripeColor.rgb + (diffuse.rgb - 0.5), 0.0, 1.0);
    
    // Blend team color based on team mask R channel
    vec3 colorWithTeam = mix(diffuse.rgb, teamAdjusted, team.r);
    // Blend stripe color based on team mask G channel
    vec3 colorWithStripe = mix(colorWithTeam, stripeAdjusted, team.g);
    
    // Calculate lighting
    vec3 N = normalize(vNormal);
    vec3 V = normalize(vViewDir);
    vec3 totalLight = uAmbient.rgb;
    vec3 totalSpec = vec3(0.0);
    
    for (int i = 0; i < 8; i++)
    {
        if (!uLights[i].enabled) continue;
        
        vec3 L = uLights[i].position - vWorldPos;
        float dist = length(L);
        L = normalize(L);
        
        // Attenuation
        float atten = 1.0 / (uLights[i].attenuation.x + 
                             uLights[i].attenuation.y * dist + 
                             uLights[i].attenuation.z * dist * dist);
        
        // Diffuse
        float NdotL = max(dot(N, L), 0.0);
        totalLight += uLights[i].diffuse * NdotL * atten;
        
        // Specular (Blinn-Phong)
        vec3 H = normalize(L + V);
        float NdotH = max(dot(N, H), 0.0);
        totalSpec += uLights[i].specular * pow(NdotH, 32.0) * atten;
    }
    
    // Glow contribution (from glow.g channel as in original)
    vec3 lighting = mix(totalLight, vec3(glow.g), glow.g);
    
    // Add specular (weighted by glow.b as in original)
    vec3 specContrib = totalSpec * glow.b;
    lighting += specContrib;
    
    // Final color
    vec3 finalColor = colorWithStripe * lighting;
    
    FragColor = vec4(finalColor, 1.0);
}
