#include "input.hpp"

#include "globals.hpp"

namespace celeste
{
    void Input::Update()
    {
        const ImGuiIO& io = ImGui::GetIO();
        
        // Direction
        float x = (float) (io.KeysDown[ImGuiKey_RightArrow] - io.KeysDown[ImGuiKey_LeftArrow]);
        float y = (float) (io.KeysDown[ImGuiKey_DownArrow] - io.KeysDown[ImGuiKey_UpArrow]);
        direction.active = x != 0 || y != 0;
        direction.vec.x = x;
        direction.vec.y = y;
        direction.moving = std::abs(Globals::gInput.direction.vec.x) > 0;

        // Other inputs
        jump.active = io.KeysDown[ImGuiKey_C];

        // Debugging
        ImGui::Begin("Inputs");
        ImGui::Text("Direction:");
        ImGui::Text("\tActive: %s", direction.active ? "true" : "false");
        ImGui::Text("\tVec: %.3f, %.3f", direction.vec.x, direction.vec.y);
        ImGui::Text("\tMoving: %s", direction.moving ? "true" : "false");
        ImGui::Text("Jump:");
        ImGui::Text("\tActive: %s", jump.active ? "true" : "false");
        ImGui::End();
    }
}
