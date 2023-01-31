#pragma once

#include <ImGui/imgui.h>
#include <GLFW/glfw3.h>
#include <calc/vector2.hpp>

#include "input.hpp"

namespace celeste
{
	class Globals {
	public:
		static calc::Vector2 gWindowPosition;
		static calc::Vector2 gWindowSize;

		static calc::Vector2 gPixelSizeMultiplier;

		static ImDrawList* gDrawList;

		static Input gInput;

		// You can't instantiate the Globals class
		Globals() = delete;

		static void Init();
		static void Update(GLFWwindow* const window);
		static void Shutdown();

		static calc::Vector2 ToWorld(calc::Vector2 pixelPosition);
		static calc::Vector2 ToPixels(calc::Vector2 worldPosition);

	private:
		// In world coordinates
		static calc::Vector2 mCameraPosition;
	};
}
