#version 330 core

// Background shader - for skybox/background meshes

in vec3 vWorldPos;
in vec3 vNormal;
in vec2 vTexCoord;
in vec3 vViewDir;

uniform sampler2D uDiffuseMap;

out vec4 FragColor;

void main()
{
    vec4 diffuse = texture(uDiffuseMap, vTexCoord);
    FragColor = diffuse;
}
