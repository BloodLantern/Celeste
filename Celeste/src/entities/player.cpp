#include "entities/player.hpp"

#include <ImGui/imgui.h>

#include "imgui_utils.hpp"
#include "globals.hpp"
#include "calc/calc.hpp"

namespace celeste
{
    celeste::Player::Player()
    {
        mPosition = 30;
        //mApplyGravity = true;
    }

    void Player::Update(float deltaTime)
    {
        if (Globals::gInput.direction.moving)
            calc::Approach(mVelocity.x, Globals::gInput.direction.vec.x * PlayerSpeedHorizontalMax, PlayerAccelerationHorizontal * deltaTime);

        // Debugging
        ImGui::Begin("Player");
        ImGui::Text("Update:");
        ImGui::Text("\tPosition: %f, %f", mPosition.x, mPosition.y);
        ImGui::Text("\tVelocity: %f, %f", mVelocity.x, mVelocity.y);
        ImGui::Text("\tDelta Time: %f", deltaTime);
        ImGui::End();

        // Frictions
        if (!Globals::gInput.direction.moving)
        {
            if (std::abs(mVelocity.x) > 90)
                calc::Approach(mVelocity.x, calc::Sign(mVelocity.x) * 90.f, 2500 * deltaTime);
            else
                calc::Approach(mVelocity.x, 0, 1000 * deltaTime);
        }

        // Gravity
        Entity::Update(deltaTime);
    }

    void Player::Draw()
    {
        ImGuiIO& io = ImGui::GetIO();

        calc::Vector2 pixelPosition = Globals::ToPixels(mPosition);
        calc::Vector2 pixelVelocity = Globals::ToPixels(mVelocity);

        ImGui::Begin("Player");
        ImGui::Text("Draw:");
        ImGui::Text("\tPixel position: %f, %f", pixelPosition.x, pixelPosition.y);
        ImGui::Text("\tPixel velocity: %f, %f", pixelVelocity.x, pixelVelocity.y);
        ImGui::Text("\tWindow position: %f, %f", Globals::gWindowPosition.x, Globals::gWindowPosition.y);
        ImGui::Text("\tMouse position: %f, %f", io.MousePos.x, io.MousePos.y);
        ImGui::End();

        Globals::gDrawList->AddRectFilled(ToImVec2(pixelPosition), ToImVec2(pixelPosition + 60), IM_COL32_WHITE);
        Globals::gDrawList->AddLine(ToImVec2(pixelPosition + 30), ToImVec2(pixelPosition + 30 + pixelVelocity / 2), IM_COL32(0xFF, 0x0, 0x0, 0xFF));
    }
}
