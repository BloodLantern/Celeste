#pragma once

#include <ImGui/imgui.h>
#include <calc/vector2.hpp>

struct Texture
{
    ImTextureID id;
    int width;
    int height;
};

class ImGuiUtils
{
public:
    static Texture loadTexture(const char* file);
    static void unloadTexture(const Texture& texture);

    static void drawTextureEx(ImDrawList& dl, const Texture& tex, ImVec2 pos, ImVec2 scale = { 1.f, 1.f }, float angle = 0.f);
};

ImVec2 ToImVec2(const calc::Vector2& vec);
