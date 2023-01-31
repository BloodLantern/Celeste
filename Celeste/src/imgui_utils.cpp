#include <stdio.h>
#include <math.h>
#include <ImGui/stb_image.h>
#include <GLFW/glfw3.h>

#include "imgui_utils.hpp"

typedef void (*glGenerateMipmapFuncType)(GLenum);

Texture ImGuiUtils::loadTexture(const char* file)
{
    Texture result = {};
    stbi_uc* pixels = stbi_load(file, &result.width, &result.height, NULL, STBI_rgb_alpha);
    if (pixels == nullptr)
    {
        fprintf(stderr, "Cannot load texture '%s'\n", file);
        return result;
    }

    // Create texture on OpenGL side
    GLuint texture;
    glGenTextures(1, &texture);
    glBindTexture(GL_TEXTURE_2D, texture);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, result.width, result.height, 0, GL_RGBA, GL_UNSIGNED_BYTE, pixels);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP);
    
    // Gen mipmaps
    glGenerateMipmapFuncType glGenerateMipmapFunc = (glGenerateMipmapFuncType)glfwGetProcAddress("glGenerateMipmap");
    if (glGenerateMipmapFunc == nullptr)
        fprintf(stderr, "Cannot load glGenerateMipmap func\n");
    else
        glGenerateMipmapFunc(GL_TEXTURE_2D);

    // Unbind
    glBindTexture(GL_TEXTURE_2D, 0);
    
    // Free ram
    stbi_image_free(pixels);

    result.id = (ImTextureID)((size_t)texture);

    return result;
}

void ImGuiUtils::unloadTexture(const Texture& texture)
{
    GLuint tex = (GLuint)((size_t)texture.id);
    glDeleteTextures(1, &tex);
}


void ImGuiUtils::drawTextureEx(ImDrawList& dl, const Texture& tex, ImVec2 pos, ImVec2 scale, float angle)
{
    // Unit quad centered in 0
    ImVec2 p[4] = {
        { -0.5f, -0.5f },
        {  0.5f, -0.5f },
        {  0.5f,  0.5f },
        { -0.5f,  0.5f },
    };

    ImVec2 uv[4] = {
        { 0.f, 0.f },
        { 1.f, 0.f },
        { 1.f, 1.f },
        { 0.f, 1.f },
    };

    // Transform quad (scale + rotate + translate)
    float c = cosf(angle);
    float s = sinf(angle);
    for (int i = 0; i < 4; ++i)
    {
        p[i].x *= tex.width  * scale.x;
        p[i].y *= tex.height * scale.y;
        float px = p[i].x;
        p[i].x = p[i].x * c - p[i].y * s;
        p[i].y = p[i].y * c + px     * s;
        p[i].x += pos.x;
        p[i].y += pos.y;
    }

    dl.AddImageQuad(tex.id,
        p[0],  p[1],  p[2],  p[3],
        uv[0], uv[1], uv[2], uv[3], 
        IM_COL32_WHITE
    );
}

ImVec2 ToImVec2(const calc::Vector2 &vec)
{
    return ImVec2(vec.x, vec.y);
}
