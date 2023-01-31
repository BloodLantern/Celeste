#pragma once

#include "imgui_utils.hpp"
#include "entities/player.hpp"
#include "entities/solid.hpp"

namespace celeste
{
	class Celeste
	{
	public:
		float deltaTime = 0;

		void Init();
		// Game update function.
		void Update();
		// 60 fps game update function.
		// dt: 60 fps delta time
		void FixedUpdate(float dt);
		void Draw();
		void Shutdown();

	private:
		double mTimeSinceLastFixedUpdate = 0;
		Player player;
		Solid ground;
		//Texture t;
	};
}
