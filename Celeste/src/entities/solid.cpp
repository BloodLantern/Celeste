#include "entities/solid.hpp"

#include "imgui_utils.hpp"
#include "globals.hpp"

namespace celeste
{
    Solid::Solid()
    {
        mPosition = calc::Vector2(20, 80);
    }

    void Solid::Draw()
    {
        calc::Vector2 pixelPosition = Globals::gWindowPosition + Globals::ToPixels(mPosition);
        Globals::gDrawList->AddRectFilled(ToImVec2(pixelPosition), ToImVec2(pixelPosition + 60), IM_COL32_WHITE);
    }
}
