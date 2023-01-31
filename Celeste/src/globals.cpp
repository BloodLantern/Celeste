#include "globals.hpp"

#define CELESTE_RESOLUTION_X 320.f
#define CELESTE_RESOLUTION_Y 180.f

namespace celeste
{
    calc::Vector2 Globals::gWindowPosition;
    calc::Vector2 Globals::gWindowSize;

    calc::Vector2 Globals::gPixelSizeMultiplier;

    ImDrawList* Globals::gDrawList;

    Input Globals::gInput;

    calc::Vector2 Globals::mCameraPosition;

    void celeste::Globals::Init()
    {
    }

    void celeste::Globals::Update(GLFWwindow* const window)
    {
        // Window position
        int windowX, windowY;
        glfwGetWindowPos(window, &windowX, &windowY);
        gWindowPosition.x = (float) windowX;
        gWindowPosition.y = (float) windowY;
        
        // Window size
        int windowWidth, windowHeight;
        glfwGetWindowSize(window, &windowWidth, &windowHeight);
        gWindowSize.x = (float) windowWidth;
        gWindowSize.y = (float) windowHeight;

        // Pixel size
        gPixelSizeMultiplier.x = windowWidth / CELESTE_RESOLUTION_X;
        gPixelSizeMultiplier.y = windowHeight / CELESTE_RESOLUTION_Y;

        // ImGui draw list
        gDrawList = ImGui::GetBackgroundDrawList();

        // Inputs update
        gInput.Update();
    }

    void celeste::Globals::Shutdown()
    {
    }

    calc::Vector2 celeste::Globals::ToWorld(calc::Vector2 pixelPosition)
    {
        return (pixelPosition - mCameraPosition) / gPixelSizeMultiplier;
    }

    calc::Vector2 celeste::Globals::ToPixels(calc::Vector2 worldPosition)
    {
        return mCameraPosition + worldPosition * gPixelSizeMultiplier;
    }
}
