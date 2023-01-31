#include "celeste.hpp"
#include "globals.hpp"

#include <iostream>
#include <ImGui/imgui.h>

#define FRAME_TIME_60 (1.f / 60.f)

namespace celeste
{
    void Celeste::Init()
    {
        //t = ImGuiUtils::loadTexture("assets/error.png");
    }

    void Celeste::Update()
    {
        //ImGuiUtils::drawTextureEx(*Globals::dl, t, ImVec2(200, 200));
        //Globals::gDrawList->AddImage(t.id, ImVec2(200, 200), ImVec2(200.f + t.width, 200.f + t.height));

        player.Update(deltaTime);

        // Fixed update things
        mTimeSinceLastFixedUpdate += deltaTime;

        if (mTimeSinceLastFixedUpdate >= FRAME_TIME_60)
        {
            int nbFrames = (int)(mTimeSinceLastFixedUpdate / FRAME_TIME_60);
            if (nbFrames > 10)
                nbFrames = 10;
            float dt = FRAME_TIME_60 * nbFrames;
            mTimeSinceLastFixedUpdate -= dt;
            FixedUpdate(dt);
        }
    }

    void Celeste::FixedUpdate(float)
    {
    }

    void Celeste::Draw()
    {
        player.Draw();
        ground.Draw();
    }

    void Celeste::Shutdown()
    {
        //ImGuiUtils::unloadTexture(t);
    }
}
